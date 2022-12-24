using System.Collections.Generic;

namespace TribeClasses
{
    [System.Serializable]
    public class MonstersSettings
    {
        public List<MonstersInfo> blocks = new();

        [System.Serializable]
        public class MonstersInfo
        {
            public string Name = "";
            public int exp = 10;
        }
    }
}
