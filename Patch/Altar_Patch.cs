using HarmonyLib;
using UnityEngine;
using static Player;
using static TribeClasses.Plugin;

namespace TribeClasses
{
    internal class Altar_Patch
    {
        [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.GetLevel))]
        [HarmonyPriority(1000)]
        internal static class CraftingStationGetLevel
        {
            public static void Postfix(CraftingStation __instance, ref int __result)
            {
                if (!__instance || __instance.m_name != "$piece_JF_ClassAltar") return;
                PulsatingGlow pulsatingGlow = __instance.GetComponent<ClassAltar>().pulsatingGlow;
                switch (__result)
                {
                    case 1:
                        pulsatingGlow.color = Color.white;
                        pulsatingGlow.active = false;
                        break;
                    case 2:
                        pulsatingGlow.color = new Color(20, 106, 191);
                        pulsatingGlow.active = true;
                        break;
                    case 3:
                        pulsatingGlow.color = new Color(191, 12, 13);
                        pulsatingGlow.active = true;
                        break;
                    case 4:
                        pulsatingGlow.color = new Color(191, 117, 48);
                        pulsatingGlow.active = true;
                        break;
                    case 5:
                        pulsatingGlow.color = new Color(0, 191, 25);
                        pulsatingGlow.active = true;
                        break;
                }
            }
        }
    }
}
