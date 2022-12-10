using HarmonyLib;
using UnityEngine.UI;
using static Trader;
using static TribeClasses.Plugin;


namespace TribeClasses
{
    internal class Trader_Path
    {
        [HarmonyPatch(typeof(Trader), nameof(Trader.Start)), HarmonyPostfix]
        [HarmonyPriority(1000)]
        public static void TraderPath(Trader __instance)
        {
            for (int i = 0; i < __instance.m_items.Count; i++)
            {
                if (__instance.m_items[i].m_prefab.name == "Ruby") return;
            }
            TradeItem RubyTradeItem = new()
            {
                m_prefab = ObjectDB.instance.GetItemPrefab("Ruby").GetComponent<ItemDrop>(),
                m_price = 250,
                m_stack = 10
            };
            __instance.m_items.Add(RubyTradeItem);
        }
    }
}
