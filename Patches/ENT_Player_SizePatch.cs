using HarmonyLib;
using UnityEngine;

namespace CustomPerks.Patches
{
    [HarmonyPatch(typeof(ENT_Player), "LateUpdate")]
    public static class ENT_Player_SizePatch
    {
        private static Vector3 _originalScale = Vector3.one;
        private static bool _hasAppliedSize = false;

        public static void Postfix(ENT_Player __instance)
        {
            if (__instance == null) return;

            float sizeBuff = __instance.curBuffs.GetBuff("addSize");
            
            if (sizeBuff > 0f)
            {
                if (!_hasAppliedSize)
                {
                    _originalScale = __instance.transform.localScale;
                    _hasAppliedSize = true;
                }
                
                Vector3 newScale = _originalScale * (1f + sizeBuff);
                __instance.transform.localScale = newScale;
            }
            else if (_hasAppliedSize)
            {
                __instance.transform.localScale = _originalScale;
                _hasAppliedSize = false;
            }
        }
    }
}
