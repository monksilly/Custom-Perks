using System.Collections.Generic;
using UnityEngine;

namespace CustomPerks.PerkModules
{
    public class PerkModule_ItemTransformer : PerkModule
    {
        private readonly HashSet<Item> _transformedItems = new HashSet<Item>();
        private float _lastCheckTime = 0f;
        private const float CheckInterval = 0.1f;

        public override void Update()
        {
            base.Update();
            
            if (string.IsNullOrEmpty(this.transformToItemID))
                return;

            if (Time.time - _lastCheckTime < CheckInterval)
                return;

            _lastCheckTime = Time.time;
            CleanupTransformedItems();
            var itemsToTransform = GetItemsToTransform();
            
            foreach (var item in itemsToTransform)
            {
                TransformItem(item);
            }
        }

        private void CleanupTransformedItems()
        {
            var currentItems = GetAllInventoryItems();
            _transformedItems.RemoveWhere(item => !currentItems.Contains(item));
        }

        private List<Item> GetAllInventoryItems()
        {
            var items = new List<Item>();
            
            if (Inventory.instance.bagItems != null)
            {
                foreach (var item in Inventory.instance.bagItems)
                {
                    if (item != null)
                        items.Add(item);
                }
            }
            
            if (Inventory.instance.itemHands != null)
            {
                foreach (var hand in Inventory.instance.itemHands)
                {
                    if (hand.currentItem != null)
                        items.Add(hand.currentItem);
                }
            }
            
            return items;
        }

        private List<Item> GetItemsToTransform()
        {
            var itemsToTransform = new List<Item>();
            
            if (string.IsNullOrEmpty(this.transformToItemID))
                return itemsToTransform;
            
            if (Inventory.instance.bagItems != null)
            {
                for (int i = 0; i < Inventory.instance.bagItems.Count; i++)
                {
                    var item = Inventory.instance.bagItems[i];
                    if (item != null && ShouldTransformItem(item))
                    {
                        itemsToTransform.Add(item);
                    }
                }
            }
            
            if (Inventory.instance.itemHands != null)
            {
                for (int i = 0; i < Inventory.instance.itemHands.Length; i++)
                {
                    var hand = Inventory.instance.itemHands[i];
                    if (hand.currentItem != null && ShouldTransformItem(hand.currentItem))
                    {
                        itemsToTransform.Add(hand.currentItem);
                    }
                }
            }
            
            return itemsToTransform;
        }

        private bool ShouldTransformItem(Item item)
        {
            if (item == null || string.IsNullOrEmpty(this.transformToItemID))
                return false;

            if (_transformedItems.Contains(item))
                return false;

            string itemName = item.itemName.ToLower();
            
            if (itemName.Equals("Item_Hammer"))
                return false;
            
            string targetName = this.transformToItemID.Replace("Item_", "").ToLower();
            if (itemName.Contains(targetName))
                return false;

            if (this.excludedItemNames != null && this.excludedItemNames.Count > 0)
            {
                for (int i = 0; i < this.excludedItemNames.Count; i++)
                {
                    if (itemName.Contains(this.excludedItemNames[i].ToLower()))
                        return false;
                }
            }

            return true;
        }

        private void TransformItem(Item item)
        {
            if (item == null)
                return;

            var targetItemObject = CL_AssetManager.GetAssetGameObject(this.transformToItemID, "");
            if (targetItemObject == null)
                return;

            var targetItemData = targetItemObject.GetComponent<Item_Object>().itemData;
            if (targetItemData == null)
                return;

            var transformedItem = targetItemData.GetClone();
            
            if (this.preservePosition)
            {
                transformedItem.bagPosition = item.bagPosition;
                transformedItem.bagRotation = item.bagRotation;
            }

            int handIndex = -1;
            for (int i = 0; i < Inventory.instance.itemHands.Length; i++)
            {
                if (Inventory.instance.itemHands[i].currentItem == item)
                {
                    handIndex = i;
                    break;
                }
            }

            if (handIndex >= 0)
            {
                Inventory.instance.DestroyItemInHand(handIndex);
                Inventory.instance.AddItemToHand(transformedItem, handIndex);
                _transformedItems.Add(transformedItem);
            }
            else
            {
                for (int i = 0; i < Inventory.instance.bagItems.Count; i++)
                {
                    if (Inventory.instance.bagItems[i] == item)
                    {
                        item.DestroyItem();
                        Inventory.instance.bagItems[i] = transformedItem;
                        _transformedItems.Add(transformedItem);
                        break;
                    }
                }
            }
        }

        public string transformToItemID;
        public List<string> excludedItemNames;
        public bool preservePosition = true;
        public GameObject transformationEffect;
    }
}

