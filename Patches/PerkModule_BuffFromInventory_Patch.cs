using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPerks.Patches
{
    [HarmonyPatch(typeof(PerkModule_BuffFromInventory), "Update")]
    public static class PerkModule_BuffFromInventory_Update_Patch
    {
        private static Dictionary<PerkModule_BuffFromInventory, string> _itemNames = new();
        
        public static bool Prefix(PerkModule_BuffFromInventory __instance)
        {
            if (_itemNames.TryGetValue(__instance, out string itemName) && !string.IsNullOrWhiteSpace(itemName))
            {
                float num = CountItemsByName(itemName, __instance.includeHands);
                
                float num2 = num;
                if (__instance.useCurve)
                {
                    num2 = __instance.itemCurve.Evaluate(num2 / (float)__instance.curveMax);
                }
                var currentBuffField = typeof(PerkModule_BuffFromInventory).GetField("currentBuff", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var perkField = typeof(PerkModule_BuffFromInventory).GetField("perk", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (currentBuffField != null && perkField != null)
                {
                    var currentBuff = currentBuffField.GetValue(__instance);
                    var perk = perkField.GetValue(__instance);
                    
                    if (currentBuff != null && perk != null)
                    {
                        var setMultiplierMethod = currentBuff.GetType().GetMethod("SetMultiplier");
                        var getStackAmountMethod = perk.GetType().GetMethod("GetStackAmount");
                        
                        if (setMultiplierMethod != null && getStackAmountMethod != null)
                        {
                            float stackAmount = (float)getStackAmountMethod.Invoke(perk, null);
                            setMultiplierMethod.Invoke(currentBuff, new object[] { num2 * __instance.buffMultiplier * stackAmount });
                        }
                    }
                }
                
                return false;
            }
            
            return true;
        }
        
        public static void SetItemName(PerkModule_BuffFromInventory module, string itemName)
        {
            if (!string.IsNullOrWhiteSpace(itemName))
            {
                _itemNames[module] = itemName;
            }
        }
        
        public static void RemoveItemName(PerkModule_BuffFromInventory module)
        {
            _itemNames.Remove(module);
        }
        
        private static int CountItemsByName(string itemName, bool includeHands)
        {
            int count = 0;
            
            if (includeHands)
            {
                foreach (var hand in Inventory.instance.itemHands)
                {
                    if (hand.currentItem != null && hand.currentItem.itemName.ToLower().Contains(itemName.ToLower()))
                    {
                        count++;
                    }
                }
            }
            
            if (Inventory.instance.bagItems != null)
            {
                foreach (var item in Inventory.instance.bagItems)
                {
                    if (item != null && item.itemName.ToLower().Contains(itemName.ToLower()))
                    {
                        count++;
                    }
                }
            }
            
            return count;
        }
    }
}
