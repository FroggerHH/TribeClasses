using UnityEngine;
using UnityEngine.UI;

namespace TribeClasses
{
    internal class SkillObject : MonoBehaviour
    {
        public Text Name;
        public Text level;
        public Image icon;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
        private void Reset()
        {
            Name = transform.Find("name").GetComponent<Text>();
            level = transform.Find("levelbar").GetComponent<Text>();
            icon = transform.Find("icon_bkg").Find("icon").GetComponent<Image>();
        }
    }
}