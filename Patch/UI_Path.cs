using HarmonyLib;
using UnityEngine.UI;
using static TribeClasses.Plugin;

namespace TribeClasses
{
    internal class UI_Path
    {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide)), HarmonyPostfix]
        [HarmonyPriority(1000)]
        public static void InventoryGuiHide()
        {
            UI.Instance.HideMenu();
        }
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake)), HarmonyPostfix]
        [HarmonyPriority(1000)]
        public static void SkillsDialogPatch()
        {
            if (!UI.Instance.menuBkg) return;
            Image skillsImage = InventoryGui.instance.m_skillsDialog.transform.Find("SkillsFrame").Find("bkg").GetComponent<Image>();
            UI.Instance.menuBkg.sprite = skillsImage.sprite;
            UI.Instance.menuBkg.material = skillsImage.material;
        }

        [HarmonyPatch(typeof(InputField), nameof(InputField.OnSelect)), HarmonyPostfix]
        public static void InputFieldSelectPatch()
        {
            isInputField = true;
        }

        [HarmonyPatch(typeof(InputField), nameof(InputField.OnDeselect)), HarmonyPostfix]
        public static void InputFieldDeselectPatch()
        {
            isInputField = false;
        }
    }
}
