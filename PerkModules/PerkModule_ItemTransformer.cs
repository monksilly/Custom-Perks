using System.Collections.Generic;
using CustomPerks.Patches;
using UnityEngine;

namespace CustomPerks.PerkModules
{
    public class PerkModule_ItemTransformer : PerkModule
    {
        public string transformToItemID;
        public List<string> excludedItemNames;
        public bool preservePosition = true;

        public override void Initialize(Perk p)
        {
            base.Initialize(p);
            ItemTransformer_Patch.RegisterTransformer(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ItemTransformer_Patch.UnregisterTransformer(this);
        }

        public bool ShouldTransform(Item item)
        {
            if (item == null || string.IsNullOrEmpty(transformToItemID))
                return false;

            string itemName = item.itemName.ToLower();
            if (itemName.Equals("Item_Hammer"))
                return false;
            
            // Don't transform if it's already the target item
            string targetName = transformToItemID.Replace("Item_", "").ToLower();
            if (itemName.Contains(targetName))
                return false;

            // Check excluded items
            if (excludedItemNames != null)
            {
                foreach (var excluded in excludedItemNames)
                {
                    if (itemName.Contains(excluded.ToLower()))
                        return false;
                }
            }

            return true;
        }

        public Item GetTransformedItem(Item originalItem)
        {
            var targetItemObject = CL_AssetManager.GetAssetGameObject(transformToItemID, "");
            if (targetItemObject == null)
                return originalItem;

            var targetItemData = targetItemObject.GetComponent<Item_Object>().itemData;
            if (targetItemData == null)
                return originalItem;

            var transformedItem = targetItemData.GetClone();
            
            if (preservePosition)
            {
                transformedItem.bagPosition = originalItem.bagPosition;
                transformedItem.bagRotation = originalItem.bagRotation;
            }

            return transformedItem;
        }
    }
}
