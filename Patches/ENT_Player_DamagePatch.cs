using System;
using HarmonyLib;

namespace CustomPerks.Patches
{
    [HarmonyPatch(typeof(ENT_Player), "Damage")]
    public static class ENT_Player_DamagePatch
    {
        public static Action<float, string> _OnDamage;

        public static void Postfix(ENT_Player __instance, float amount, string type, bool __result)
        {
            if (!__result && _OnDamage != null)
            {
                _OnDamage.Invoke(amount, type);
            }
        }
    }
}

