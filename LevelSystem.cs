using BepInEx.Configuration;
using Groups;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DualWield.DualWield;
using static Player;
using static Skills;
using static TribeClasses.LevelsSystemTree;
using static TribeClasses.Plugin;
using static TribeClasses.Tutorial_Patch;

namespace TribeClasses
{
    internal class LevelSystem
    {
        #region values
        #region configs
        internal ConfigEntry<int> maxLevelConfig;
        internal ConfigEntry<Toggle> healOnLevelUpConfig;
        internal ConfigEntry<int> firstLevelExpConfig;
        internal ConfigEntry<string> levelsSystemTreeConfig;
        internal ConfigEntry<string> monstersSettingsConfig;
        internal ConfigEntry<KeyCode> openMenuKeyConfig;
        internal ConfigEntry<KeyCode> closeMenuKeyConfig;
        internal ConfigEntry<float> groupExpFactorConfig;
        internal LevelsSystemTree levelsSystemTree = new();
        internal MonstersSettings monstersSettings = new();
        internal KeyCode openMenuKey = KeyCode.None;
        internal KeyCode closeMenuKey = KeyCode.None;
        internal int maxLevel = 100;
        internal int firstLevelExp = 200;
        internal bool healOnLevelUp = false;
        internal float groupExpFactor;
        #endregion
        internal bool canUseSuper = true;
        internal DateTime LastX2Damage;
        internal DateTime LastReturnDmg;
        internal DateTime LastNotTakeDmg;
        internal DateTime LastSuper;
        private bool updateBonuces = true;
        private static LevelSystem _instance;
        private Bonuses bonuses = new();
        private List<Super> supers = new();
        internal List<GameObject> superObj = new();
        internal static LevelSystem Instance
        {
            get
            {
                bool flag = _instance == null;
                LevelSystem result;
                if(flag)
                {
                    _instance = new LevelSystem();
                    result = _instance;
                }
                else
                {
                    result = _instance;
                }
                return result;
            }
        }
        private List<string> DualWieldExclusionOriginal = null;
        internal Action onLevelUp;
        internal Action onGetExpp;
        internal Action onClassGetReset;
        #endregion

        internal void Update()
        {
            if(updateBonuces && HaveClass())
            {
                UpdateBonuses();
            }
        }
        internal void UpdateBonuses()
        {
            if(!HaveClass())
            {
                bonuses = null;
                return;
            }
            if(!m_localPlayer)
            {
                bonuses = null;
                return;
            }
            LevelInfo levelTree = GetFullLevelTree();
            if(levelTree == null)
            {
                bonuses = null;
                return;
            }
            if(supers != null && supers.Count > 0)
            {
                for(int i = 0; i < supers.Count; i++)
                {
                    levelTree.Modify(supers[i].data.bonuses);
                }
            }
            bonuses = levelTree.bonuses;

            UI.Instance.UpdateStats();

            ZDO? zdo = m_localPlayer?.m_nview?.GetZDO();
            if(zdo == null)
            {
                return;
            }

            zdo.Set("TribeClasses_ChanceToNotTakeDmg", bonuses.ChanceToNotTakeDmg);
            zdo.Set("TribeClasses_ChanceToReturnDmg", bonuses.ChanceToReturnDmg);
            zdo.Set("TribeClasses_ChanceToX2Dmg", bonuses.ChanceToX2Dmg);
            zdo.Set("TribeClasses_ReturnDmg", bonuses.ReturnDmg);

        }
        internal void RemoveExp(int exp)
        {
            int currentExp = GetCurrentExp();

            if(currentExp >= exp)
            {
                currentExp -= exp;
            }
            else
            {
                currentExp = 0;
            }

            SetCurrentExp(currentExp);
            return;
        }
        internal void AddExp(int exp)
        {
            int currentExp = GetCurrentExp();
            int currentLevel = GetLevel();
            if(exp < 1 || currentLevel >= maxLevel)
            {
                m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$max_level");
                return;
            }
            currentExp += exp;
            SetCurrentExp(currentExp);
            UI.Instance.UpdateStats();
            m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"$exp_added {exp}");

            onGetExpp?.Invoke();
        }
        internal int GetCurrentExp()
        {
            if(!Player.m_localPlayer)
            {
                return 0;
            }

            if(!Player.m_localPlayer.m_knownTexts.ContainsKey("TribeClasses_CurrentExp"))
            {
                return 0;
            }
            return int.Parse(Player.m_localPlayer.m_knownTexts["TribeClasses_CurrentExp"]);
        }
        internal int GetLevel()
        {
            if(!Player.m_localPlayer)
            {
                return 0;
            }

            if(!Player.m_localPlayer.m_knownTexts.ContainsKey("TribeClasses_Level"))
            {
                return 0;
            }
            return int.Parse(Player.m_localPlayer.m_knownTexts["TribeClasses_Level"]);
        }
        internal void AddLevel(int count)
        {
            if(count <= 0)
            {
                return;
            }

            int current = GetLevel();
            current += count;
            SetLevel(Mathf.Clamp(current, 1, maxLevel));
        }
        internal bool isOnMaxLevel()
        {
            return GetLevel() >= maxLevel;
        }
        internal void SetLevel(int value)
        {
            if(!m_localPlayer)
            {
                return;
            }

            m_localPlayer.m_knownTexts["TribeClasses_Level"] = value.ToString();
            SetCurrentExp(0);
            PlayerFVX.Instance.LevelUp();
            ZDO zdo = m_localPlayer.m_nview.GetZDO();
            zdo.Set($"TribeClasses_Level", GetLevel());
            ZDOMan.instance.ForceSendZDO(zdo.m_uid);

            int newLevel = GetLevel();
            m_localPlayer.Message(MessageHud.MessageType.Center, $"$new_level {newLevel}!");
            onLevelUp?.Invoke();

            ApplyBonuses();

            if(GetFullBonuses().unlockSuper)
            {
                ShowTutorial("tutorial_JF_ancients_horn");
            }

            if(healOnLevelUp)
            {
                m_localPlayer.Heal(m_localPlayer.GetMaxHealth());
                m_localPlayer.AddStamina(m_localPlayer.GetMaxStamina());
            }
        }

        internal void ApplyBonuses()
        {
            UpdateBonuses();
            Bonuses bonuses = GetFullBonuses();
            if(bonuses == null)
            {
                return;
            }

            _self.Debug("Bonuses Applied");
        }
        internal void AddSuperBonuses(Super super)
        {
            supers.Add(super);
            UpdateBonuses();
            ApplyBonuses();
        }
        internal void RemooveSuperBonuses()
        {
            if(supers == null || supers.Count == 0)
            {
                return;
            }

            _self.StopAllCoroutines();
            canUseSuper = true;
            for(int i = 0; i < superObj.Count; i++)
            {
                if(superObj.Count > 0 && superObj[i])
                {
                    DestroyWithDelay(superObj[i].GetComponent<ZNetView>(), 0.2f);
                }
            }
            foreach(Super super in supers)
            {
                m_localPlayer.GetSEMan().RemoveStatusEffect(super.data.name);
            }
            UpdateBonuses();
            ApplyBonuses();
            supers.Clear();
            superObj.Clear();

            ApplyBonuses();
        }
        internal void RemoveBonuses()
        {
            _self.Debug("Bonuses Removed");
        }
        internal int GetExpForNewLevel()
        {
            return (int)(firstLevelExp * Math.Pow(GetClassTree().levelExpModifier, GetLevel()));
        }
        internal int GetExpFromMonster(Character character)
        {
            return GetExpFromMonster(character.m_name);
        }
        internal int GetExpFromMonster(string name)
        {
            int returnExp = 0;
            name = name.Replace("(Clone)", "");


            foreach(MonstersSettings.MonstersInfo header in monstersSettings.blocks)
            {
                if(header.Name == name)
                {
                    returnExp = header.exp;
                    break;
                }
            }

            return returnExp;
        }
        internal Bonuses GetFullBonuses()
        {
            return bonuses;
        }
        internal Bonuses GetBonuses()
        {
            if(!HaveClass())
            {
                return null;
            }

            LevelInfo levelTree = GetLevelTree();
            if(levelTree == null)
            {
                return null;
            }

            return levelTree.bonuses;
        }
        private int LevelsToAdd()
        {
            int currentExp = GetCurrentExp();
            int needExpForNewLevel = GetExpForNewLevel();
            if(needExpForNewLevel == 0)
            {
                return 1;
            }

            float levels = currentExp / needExpForNewLevel;
            int returnLevels = Mathf.FloorToInt(levels);
            if(returnLevels <= 0)
            {
                returnLevels = 0;
            }

            return returnLevels;
        }
        internal ClassInfo GetClassTree(string Class = "local")
        {
            if(Class == "local")
            {
                Class = GetClass();
            }

            ClassInfo classTree = null;

            foreach(ClassInfo _header in levelsSystemTree.blocks)
            {
                if(_header.className == Class)
                {
                    classTree = _header;
                    break;
                }
            }

            return classTree;
        }
        private LevelInfo GetFullLevelTree()
        {
            if(!HaveClass())
            {
                return null;
            }

            int level = GetLevel();
            ClassInfo classTree = GetClassTree();
            LevelInfo levelTree = null;

            if(classTree == null)
            {
                return null;
            }

            foreach(LevelInfo item in classTree.levelTree)
            {
                if(item.level <= level)
                {
                    if(levelTree == null) levelTree = new();
                    levelTree.Modify(item.bonuses);
                }
            }
            levelTree.Modify(classTree.everyLevelBonuses, level);

            return levelTree;
        }
        private LevelInfo GetLevelTree()
        {
            if(!HaveClass())
            {
                return new();
            }

            int level = GetLevel();
            ClassInfo classTree = GetClassTree();
            LevelInfo levelTree = null;

            if(classTree == null)
            {
                return null;
            }

            foreach(LevelInfo item in classTree.levelTree)
            {
                if(item.level == level)
                {
                    levelTree = item;
                }
            }

            if(levelTree == null)
            {
                levelTree = new();
            }

            levelTree.Modify(classTree.everyLevelBonuses);

            return levelTree;
        }
        private void SetCurrentExp(int value)
        {
            m_localPlayer.m_knownTexts["TribeClasses_CurrentExp"] = value.ToString();
            ZDO zdo = m_localPlayer.m_nview.GetZDO();
            zdo.Set("TribeClasses_CurrentExp", value);

            int levelsToAdd = LevelsToAdd();
            if(levelsToAdd > 0)
            {
                AddLevel(levelsToAdd);
            }
        }
        internal static string GetSkillName(SkillType skillType)
        {
            string returnSkillName = skillType.ToString().ToLower();
            return returnSkillName;
        }
        internal void Load()
        {
            int currentExp = GetCurrentExp();
            int currentLevel = GetLevel();
            string currentClass = GetClass();
            _self.Debug($"Load --- currentClass = {currentClass}, currentLevel = {currentLevel}, currentExp = {currentExp}");
            ZDO zdo = m_localPlayer.m_nview.GetZDO();
            zdo.Set("TribeClasses_Level", currentLevel);
            zdo.Set("TribeClasses_CurrentExp", currentExp);
            zdo.Set($"TribeClasses_Class", currentClass);

            if(haveDualWieldInstaled)
            {
                UpdateDualWield();
            }

            LoadBonuses();
        }
        internal void LoadBonuses()
        {
            UpdateBonuses();
            Bonuses bonuses = GetFullBonuses();
            if(bonuses == null)
            {
                return;
            }

            /*m_localPlayer.m_baseHP += bonuses.Health;
            HPAdded += bonuses.Health;
            m_localPlayer.m_baseStamina += bonuses.Stamina;
            staminaAdded += bonuses.Stamina;*/
            _self.Debug("Bonuses Loaded");
        }
        internal void UpdateDualWield()
        {
            if(!haveDualWieldInstaled)
            {
                return;
            }

            ClassInfo classTree = GetClassTree();

            if(classTree != null && haveDualWieldInstaled)
            {
                if(DualWieldExclusionOriginal == null)
                {
                    DualWieldExclusionOriginal = DualWieldExclusion;
                }
                else
                {
                    DualWieldExclusion.Clear();
                }

                for(int i = 0; i < DualWieldExclusionOriginal.Count; i++)
                {
                    if(DualWieldExclusion.Contains(DualWieldExclusionOriginal[i]))
                    {
                        continue;
                    }

                    DualWieldExclusion.Add(DualWieldExclusionOriginal[i]);
                }
                for(int i = 0; i < classTree.dualWieldExclusionCertainItems.Count; i++)
                {
                    if(DualWieldExclusion.Contains(classTree.dualWieldExclusionCertainItems[i]))
                    {
                        continue;
                    }

                    DualWieldExclusion.Add(classTree.dualWieldExclusionCertainItems[i]);
                }
            }
        }

        internal bool UseSuper()
        {
            if(canUseSuper)
            {
                Super super = new(GetSuper());
                super.SetOwner();
                SendSuper(super);
                LastSuper = DateTime.Now;
                return true;
            }
            else
            {
                return false;
            }
        }
        internal bool HaveAnySuper()
        {
            if(supers != null && supers.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        internal bool HaveOwnSuper()
        {
            if(supers == null || supers.Count == 0)
            {
                return false;
            }

            for(int i = 0; i < supers.Count; i++)
            {
                if(supers[i].owner == ZNet.instance.GetUID())
                {
                    return true;
                }
            }

            return false;
        }
        internal Super.SuperData GetSuper(string supersName = "local")
        {
            if(supersName == "local")
            {
                supersName = GetClass();
            }

            return GetClassTree(supersName.Replace("_super", "")).super;
        }
        internal void SendSuper(Super super)
        {
            List<Player> playersInRage = new();
            List<PlayerReference> playerReferences = API.GroupPlayers();
            GetPlayersInRange(m_localPlayer.transform.position, super.data.range, playersInRage);

            for(int i = 0; i < Math.Min(playerReferences.Count, playersInRage.Count); i++)
            {
                PlayerReference plInR = new()
                {
                    name = playersInRage[i].GetPlayerName(),
                    peerId = playersInRage[i].GetPlayerID()
                };

                for(int ii = 0; ii < playerReferences.Count; ii++)
                {
                    if(playerReferences[ii].name == plInR.name && playerReferences[ii].name != m_localPlayer.GetPlayerName())
                    {
                        playersInRage[i].m_nview.InvokeRPC("ApplySuper", super.data.name);
                    }
                }
            }

            m_localPlayer.m_nview.InvokeRPC("ApplySuper", super.data.name);
        }
        internal void RPC_ApplySuper(long sender, string supersName)
        {
            Super super = new(GetSuper(supersName));
            super.SetOwner(sender);
            StatusEffect statusEffect = m_localPlayer.GetSEMan().AddStatusEffect($"_JF_SE_{supersName}");
            m_localPlayer.Message(MessageHud.MessageType.Center, $"${supersName}_start");
            _self.StartCoroutine(_self.AddSuper(super));
            statusEffect.m_ttl = super.data.time;
        }
    }
}