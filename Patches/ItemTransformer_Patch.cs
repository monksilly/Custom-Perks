using System.Collections.Generic;
using CustomPerks.PerkModules;
using HarmonyLib;

namespace CustomPerks.Patches
{
    [HarmonyPatch(typeof(Inventory), "AddItemToHand", new[] { typeof(Item), typeof(ENT_Player.Hand) })]
    public static class ItemTransformer_Patch
    {
        private static readonly List<PerkModule_ItemTransformer> _activeTransformers = new List<PerkModule_ItemTransformer>();

        public static void RegisterTransformer(PerkModule_ItemTransformer transformer)
        {
            if (!_activeTransformers.Contains(transformer))
            {
                _activeTransformers.Add(transformer);
            }
        }

        public static void UnregisterTransformer(PerkModule_ItemTransformer transformer)
        {
            _activeTransformers.Remove(transformer);
        }

        public static void Prefix(ref Item i)
        {
            if (i == null || _activeTransformers.Count == 0)
                return;

            foreach (var transformer in _activeTransformers)
            {
                if (transformer.ShouldTransform(i))
                {
                    var transformedItem = transformer.GetTransformedItem(i);
                    if (transformedItem != null && transformedItem != i)
                    {
                        i = transformedItem;
                        return;
                    }
                }
            }
        }
    }
}

