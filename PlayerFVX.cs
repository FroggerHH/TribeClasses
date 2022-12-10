using ItemManager;
using UnityEngine;
using static TribeClasses.LevelsSystemTree;
using static TribeClasses.Plugin;

namespace TribeClasses
{
    public class PlayerFVX
    {
        public static PlayerFVX Instance
        {
            get
            {
                bool flag = _instance == null;
                PlayerFVX result;
                if (flag)
                {
                    _instance = new PlayerFVX();
                    result = _instance;
                }
                else
                {
                    result = _instance;
                }
                return result;
            }
        }
        private static PlayerFVX _instance;
        GameObject levelUpPrefab;
        GameObject superPrefabG;
        GameObject superPrefabB;
        GameObject superPrefabR;
        GameObject superPrefabD;

        public static void Init()
        {
            Instance.levelUpPrefab = PrefabManager.RegisterPrefab(_self.assetBundle, "_JF_VFX_level_up");
            Instance.superPrefabG = PrefabManager.RegisterPrefab(_self.assetBundle, "_JF_VFX_guardian_super");
            Instance.superPrefabB = PrefabManager.RegisterPrefab(_self.assetBundle, "_JF_VFX_berserker_super");
            Instance.superPrefabR = PrefabManager.RegisterPrefab(_self.assetBundle, "_JF_VFX_ranger_super");
            Instance.superPrefabD = PrefabManager.RegisterPrefab(_self.assetBundle, "_JF_VFX_druid_super");
        }

        public void LevelUp()
        {
            EffectList effectList = new();
            effectList.m_effectPrefabs = new[] { new EffectList.EffectData { m_prefab = levelUpPrefab, m_attach = true } };
            GameObject[] effects = effectList.Create(Player.m_localPlayer.transform.position, Quaternion.identity, Player.m_localPlayer.transform);
            for (int i = 0; i < effects.Length; i++)
            {
                DestroyWithDelay(effects[i].GetComponent<ZNetView>(), 3f);
            }
        }
        public void Super(Super.SuperData super)
        {
            EffectList effectList = new();
            if (super.name == "guardian_super") effectList.m_effectPrefabs = new[] { new EffectList.EffectData { m_prefab = superPrefabG, m_attach = true } };
            else if (super.name == "berserker_super") effectList.m_effectPrefabs = new[] { new EffectList.EffectData { m_prefab = superPrefabB, m_attach = true } };
            else if (super.name == "ranger_super") effectList.m_effectPrefabs = new[] { new EffectList.EffectData { m_prefab = superPrefabR, m_attach = true } };
            else if (super.name == "druid_super") effectList.m_effectPrefabs = new[] { new EffectList.EffectData { m_prefab = superPrefabD, m_attach = true } };
            else _self.DebugError($"Couldn't find super {super.name}");
            GameObject[] effects = effectList.Create(Player.m_localPlayer.transform.position, Quaternion.identity, Player.m_localPlayer.transform);

            LevelSystem.Instance.superObj.Clear();

            for (int i = 0; i < effects.Length; i++)
            {
                LevelSystem.Instance.superObj.Add(effects[i]);
            }
        }
    }
}