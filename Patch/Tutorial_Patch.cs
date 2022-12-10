using HarmonyLib;
using UnityEngine.SceneManagement;
using static TribeClasses.Plugin;
using static Player;


namespace TribeClasses
{
    internal class Tutorial_Patch
    {
        public static void ShowTutorial(string name, bool force = false)
        {
            _self.Debug($"ShowTutorial {name}");
            if (m_localPlayer.HaveSeenTutorial(name)) return;
            Tutorial.instance.ShowText(name, force);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.OnInventoryChanged))]
        internal static void PlayerOnInventoryChangedPatch(Player __instance)
        {
            if (SceneManager.GetActiveScene().name == "main" && __instance == m_localPlayer)
            {
                Inventory inventory = __instance.GetInventory();
                foreach (ItemDrop.ItemData itemData in inventory.GetAllItems())
                {
                    if (itemData.m_shared.m_name == "$item_ruby")
                    {
                        ShowTutorial("tutorial_JF_item_ruby", false);
                    }
                    else if (itemData.m_shared.m_name == "$item_rubyLarge")
                    {
                        ShowTutorial("tutorial_JF_item_rubyLarge", false);
                    }
                    else if (itemData.m_shared.m_name == "$item_rubyLargeСharged")
                    {
                        ShowTutorial("tutorial_JF_item_rubyLargeСharged", false);
                    }
                }

            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tutorial), nameof(Tutorial.Awake))]
        internal static void TutorialAwakePatch(Tutorial __instance)
        {
            __instance.m_texts.Add(new()
            {
                m_name = $"tutorial_JF_item_ruby",
                m_topic = $"$tutorial_JF_item_ruby_topic",
                m_text = $"$tutorial_JF_item_ruby_text",
                m_label = $"$tutorial_JF_item_ruby_label"
            });
            __instance.m_texts.Add(new()
            {
                m_name = $"tutorial_JF_item_rubyLarge",
                m_topic = $"$tutorial_JF_item_rubyLarge_topic",
                m_text = $"$tutorial_JF_item_rubyLarge_text",
                m_label = $"$tutorial_JF_item_rubyLarge_label"
            });
            __instance.m_texts.Add(new()
            {
                m_name = $"tutorial_JF_item_rubyLargeСharged",
                m_topic = $"$tutorial_JF_item_rubyLargeСharged_topic",
                m_text = $"$tutorial_JF_item_rubyLargeСharged_text",
                m_label = $"$tutorial_JF_item_rubyLargeСharged_label"
            });
            __instance.m_texts.Add(new()
            {
                m_name = "tutorial_JF_ancients_horn",
                m_topic = $"$tutorial_JF_ancients_horn_topic",
                m_text = $"$tutorial_JF_ancients_horn_text",
                m_label = $"$tutorial_JF_ancients_horn_label"
            });
        }
    }
}
