using System;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace LegitGrayscreen
{
    [HarmonyPatch(typeof(VRRig), "OnDisable")]
    public class RigPatcher
    {

        [HarmonyPrefix]
        public static bool Prefix(VRRig __instance)
        {
            return __instance != GorillaTagger.Instance.offlineVRRig;
        }
    }
}
