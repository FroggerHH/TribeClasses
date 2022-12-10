using ItemManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Player;
using static TribeClasses.LevelsSystemTree;
using static TribeClasses.LevelsSystemTree.Bonuses;
using static TribeClasses.Plugin;
using static UnityEngine.Object;

namespace TribeClasses
{
    public class UI
    {
        #region UIRefs
        public GameObject menu;
        public GameObject noneSkill;
        public GameObject superG;
        public GameObject superB;
        public GameObject superR;
        public GameObject superD;
        public GameObject skillParent;
        private Button closeButton;
        private Text className;
        private Text level;
        private Text exp;
        private Text health;
        private Text eitr;
        private Text healthRegeneration;
        private Text eitrRegeneration;
        private Text stamina;
        private Text staminaRegeneration;
        private Text armor;
        private Text defense;
        private Text vampirism;
        private Text chanceToNotTakeDmg;
        private Text chanceToReturnDmg;
        private Text returnDmg;
        private Text x2DmgChance;
        private Text weight;
        private Text moveSpeed;
        private Text attackSpeedAll;
        private Text attackSpeedSpell;
        private Text attackSpeedMele;
        private Text bowReloadTime;
        private Text damageModAll;
        private Text damageModMele;
        private Text damageModBow;
        private Text damageModSpell;
        private Image classLogo;
        internal Image menuBkg;
        #endregion
        #region values
        private List<SkillObject> skills = new();
        private GameObject currentSuper;

        private static UI _instance;

        public static UI Instance
        {
            get
            {
                bool flag = _instance == null;
                UI result;
                if (flag)
                {
                    _instance = new UI();
                    result = _instance;
                }
                else
                {
                    result = _instance;
                }
                return result;
            }
        }

        internal Sprite guardianLogo;
        internal Sprite berserkerLogo;
        internal Sprite rangerLogo;
        internal Sprite druidLogo;
        #endregion

        public void Init()
        {
            if (menu)
            {
                return;
            }

            menu = PrefabManager.RegisterPrefab(_self.assetBundle, "_JF_CLassMenu");
            menu = Instantiate(menu);
            DontDestroyOnLoad(menu);

            closeButton = menu.transform.Find("Skills").Find("SkillsFrame").Find("Closebutton").gameObject.GetComponent<Button>();
            closeButton.onClick.AddListener(HideMenu);
            className = menu.transform.Find("Skills").Find("SkillsFrame").Find("topic").gameObject.GetComponent<Text>();
            exp = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("Exp").gameObject.GetComponent<Text>();
            level = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("Level").gameObject.GetComponent<Text>();
            health = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("Health").Find("levelbar").gameObject.GetComponent<Text>();
            eitr = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("Eitr").Find("levelbar").gameObject.GetComponent<Text>();
            stamina = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("Stamina").Find("levelbar").gameObject.GetComponent<Text>();
            staminaRegeneration = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("StaminaRegeneration").Find("levelbar").gameObject.GetComponent<Text>();
            armor = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("Armor").Find("levelbar").gameObject.GetComponent<Text>();
            defense = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("Defense").Find("levelbar").gameObject.GetComponent<Text>();
            vampirism = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("Vampirism").Find("levelbar").gameObject.GetComponent<Text>();
            chanceToNotTakeDmg = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("CTNTD").Find("levelbar").gameObject.GetComponent<Text>();
            chanceToReturnDmg = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("ReturnableDamageChance").Find("levelbar").gameObject.GetComponent<Text>();
            returnDmg = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("ReturnableDamage").Find("levelbar").gameObject.GetComponent<Text>();
            x2DmgChance = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("X2DmgChance").Find("levelbar").gameObject.GetComponent<Text>();
            weight = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("Weight").Find("levelbar").gameObject.GetComponent<Text>();
            moveSpeed = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("MoveSpeed").Find("levelbar").gameObject.GetComponent<Text>();
            healthRegeneration = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("HealthRegeneration").Find("levelbar").gameObject.GetComponent<Text>();
            eitrRegeneration = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("EitrRegeneration").Find("levelbar").gameObject.GetComponent<Text>();
            attackSpeedAll = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("AttackSpeedAll").Find("levelbar").gameObject.GetComponent<Text>();
            attackSpeedSpell = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("AttackSpeedSpell").Find("levelbar").gameObject.GetComponent<Text>();
            attackSpeedMele = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("AttackSpeedMele").Find("levelbar").gameObject.GetComponent<Text>();
            bowReloadTime = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("BowReloadTime").Find("levelbar").gameObject.GetComponent<Text>();
            damageModAll = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("DamageModAll").Find("levelbar").gameObject.GetComponent<Text>();
            damageModMele = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("DamageModMele").Find("levelbar").gameObject.GetComponent<Text>();
            damageModBow = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("DamageModBow").Find("levelbar").gameObject.GetComponent<Text>();
            damageModSpell = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").Find("DamageModSpell").Find("levelbar").gameObject.GetComponent<Text>();
            noneSkill = menu.transform.Find("Skills").Find("None").gameObject;
            superG = menu.transform.Find("Skills").Find("SuperG").gameObject;
            superB = menu.transform.Find("Skills").Find("SuperB").gameObject;
            superR = menu.transform.Find("Skills").Find("SuperR").gameObject;
            superD = menu.transform.Find("Skills").Find("SuperD").gameObject;
            skillParent = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").Find("Scroll View").Find("Viewport").Find("Content").gameObject;
            classLogo = menu.transform.Find("Skills").Find("SkillsFrame").Find("Skills").GetComponent<Image>();
            menuBkg = menu.transform.Find("Skills").Find("SkillsFrame").Find("bkg").GetComponent<Image>();

            Text healthStatName = health.transform.parent.Find("name").GetComponent<Text>();
            Text eitrStatName = eitr.transform.parent.Find("name").GetComponent<Text>();
            Text staminaStatName = stamina.transform.parent.Find("name").GetComponent<Text>();
            Text staminaRegenerationStatName = staminaRegeneration.transform.parent.Find("name").GetComponent<Text>();
            Text eitrRegenerationStatName = eitrRegeneration.transform.parent.Find("name").GetComponent<Text>();
            Text armorStatName = armor.transform.parent.Find("name").GetComponent<Text>();
            Text defenseStatName = defense.transform.parent.Find("name").GetComponent<Text>();
            Text vampirismStatName = vampirism.transform.parent.Find("name").GetComponent<Text>();
            Text chanceToNotTakeDmgStatName = chanceToNotTakeDmg.transform.parent.Find("name").GetComponent<Text>();
            Text chanceToReturnDmgStatName = chanceToReturnDmg.transform.parent.Find("name").GetComponent<Text>();
            Text returnDmgStatName = returnDmg.transform.parent.Find("name").GetComponent<Text>();
            Text x2DmgStatName = x2DmgChance.transform.parent.Find("name").GetComponent<Text>();
            Text weightStatName = weight.transform.parent.Find("name").GetComponent<Text>();
            Text moveSpeedStatName = moveSpeed.transform.parent.Find("name").GetComponent<Text>();
            Text healthRegenerationStatName = healthRegeneration.transform.parent.Find("name").GetComponent<Text>();
            Text attackSpeedAllStatName = attackSpeedAll.transform.parent.Find("name").GetComponent<Text>();
            Text attackSpeedSpellStatName = attackSpeedSpell.transform.parent.Find("name").GetComponent<Text>();
            Text attackSpeedMeleStatName = attackSpeedMele.transform.parent.Find("name").GetComponent<Text>();
            Text attackSpeedBowStatName = bowReloadTime.transform.parent.Find("name").GetComponent<Text>();
            Text damageModAllStatName = damageModAll.transform.parent.Find("name").GetComponent<Text>();
            Text damageModMeleStatName = damageModMele.transform.parent.Find("name").GetComponent<Text>();
            Text damageModBowStatName = damageModBow.transform.parent.Find("name").GetComponent<Text>();
            Text damageModSpellStatName = damageModSpell.transform.parent.Find("name").GetComponent<Text>();
            Text closeButtonStatName = closeButton.transform.Find("Text").GetComponent<Text>();
            Text superGName = superG.transform.Find("name").GetComponent<Text>();
            Text superBName = superB.transform.Find("name").GetComponent<Text>();
            Text superRName = superR.transform.Find("name").GetComponent<Text>();
            Text superDName = superD.transform.Find("name").GetComponent<Text>();

            closeButtonStatName.text = Localization.instance.Localize(closeButtonStatName.text);
            healthStatName.text = Localization.instance.Localize(healthStatName.text);
            eitrStatName.text = Localization.instance.Localize(eitrStatName.text);
            staminaStatName.text = Localization.instance.Localize(staminaStatName.text);
            staminaRegenerationStatName.text = Localization.instance.Localize(staminaRegenerationStatName.text);
            eitrRegenerationStatName.text = Localization.instance.Localize(eitrRegenerationStatName.text);
            armorStatName.text = Localization.instance.Localize(armorStatName.text);
            defenseStatName.text = Localization.instance.Localize(defenseStatName.text);
            vampirismStatName.text = Localization.instance.Localize(vampirismStatName.text);
            chanceToNotTakeDmgStatName.text = Localization.instance.Localize(chanceToNotTakeDmgStatName.text);
            chanceToReturnDmgStatName.text = Localization.instance.Localize(chanceToReturnDmgStatName.text);
            returnDmgStatName.text = Localization.instance.Localize(returnDmgStatName.text);
            returnDmgStatName.text = Localization.instance.Localize(returnDmgStatName.text);
            x2DmgStatName.text = Localization.instance.Localize(x2DmgStatName.text);
            weightStatName.text = Localization.instance.Localize(weightStatName.text);
            moveSpeedStatName.text = Localization.instance.Localize(moveSpeedStatName.text);
            attackSpeedAllStatName.text = Localization.instance.Localize(attackSpeedAllStatName.text);
            attackSpeedSpellStatName.text = Localization.instance.Localize(attackSpeedSpellStatName.text);
            attackSpeedMeleStatName.text = Localization.instance.Localize(attackSpeedMeleStatName.text);
            attackSpeedBowStatName.text = Localization.instance.Localize(attackSpeedBowStatName.text);
            damageModAllStatName.text = Localization.instance.Localize(damageModAllStatName.text);
            damageModMeleStatName.text = Localization.instance.Localize(damageModMeleStatName.text);
            damageModBowStatName.text = Localization.instance.Localize(damageModBowStatName.text);
            damageModSpellStatName.text = Localization.instance.Localize(damageModSpellStatName.text);
            healthRegenerationStatName.text = Localization.instance.Localize(healthRegenerationStatName.text);
            superGName.text = Localization.instance.Localize(superGName.text);
            superBName.text = Localization.instance.Localize(superBName.text);
            superRName.text = Localization.instance.Localize(superRName.text);
            superDName.text = Localization.instance.Localize(superDName.text);

            guardianLogo = _self.assetBundle.LoadAsset<Sprite>("GuardianLogo");
            berserkerLogo = _self.assetBundle.LoadAsset<Sprite>("BerserkerLogo");
            rangerLogo = _self.assetBundle.LoadAsset<Sprite>("RangerLogo");
            druidLogo = _self.assetBundle.LoadAsset<Sprite>("DruidLogo");

            UpdateClassLogo();
            HideMenu();
        }

        private void UpdateClassLogo()
        {
            classLogo.sprite = GetClass() switch
            {
                "guardian" => guardianLogo,
                "berserker" => berserkerLogo,
                "ranger" => rangerLogo,
                "druid" => druidLogo,
                _ => null
            };
        }

        public bool IsMenuVisble()
        {
            if (!menu)
            {
                return false;
            }

            return menu.activeInHierarchy;
        }

        internal void Update()
        {
            if (Input.GetKeyDown(LevelSystem.Instance.openMenuKey))
            {
                ShowMenu();
            }

            if (Input.GetKeyDown(LevelSystem.Instance.closeMenuKey))
            {
                HideMenu();
            }
        }

        internal void UpdateStats()
        {
            if (!IsMenuVisble() || !HaveClass())
            {
                return;
            }

            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
            if (bonuses == null)
            {
                return;
            }

            string className_ = GetClass();
            UpdateClassLogo();

            className.text = Localization.instance.Localize($"${className_}");
            if (LevelSystem.Instance.isOnMaxLevel())
            {
                level.gameObject.SetActive(false);
                exp.gameObject.SetActive(false);
            }
            else
            {
                level.gameObject.SetActive(true);
                exp.gameObject.SetActive(true);
                level.text = $"{Localization.instance.Localize("$level")} {LevelSystem.Instance.GetLevel()}";
                exp.text = $"{LevelSystem.Instance.GetCurrentExp()} / {LevelSystem.Instance.GetExpForNewLevel()}";
            }


            if (bonuses.unlockSuper)
            {
                if (currentSuper)
                {
                    Destroy(currentSuper);
                }

                if (className_ == "guardian")
                {
                    currentSuper = Instantiate(superG, skillParent.transform);
                }
                else if (className_ == "berserker")
                {
                    currentSuper = Instantiate(superB, skillParent.transform);
                }
                else if (className_ == "ranger")
                {
                    currentSuper = Instantiate(superR, skillParent.transform);
                }
            }
            else
            {
                Destroy(currentSuper);
                currentSuper = null;
            }

            foreach (SkillObject item in skills)
            {
                Destroy(item.gameObject);
            }
            skills = new();
            foreach (ModifySkill item in bonuses.m_ModifySkill)
            {
                Skills.SkillType skillType = SkillTypeFromName(item.skillName);
                if (skillType == Skills.SkillType.None)
                {
                    continue;
                }

                bool flag = true;
                for (int i = 0; i < skills.Count; i++)
                {
                    if (skills[i].Name.text.ToLower().Contains(item.skillName.ToLower()))
                    {
                        flag = false;
                        _self.Debug("UpdateStats 2");
                    }
                }
                if (flag)
                {
                    SkillObject skillObj = Instantiate(noneSkill, skillParent.transform).GetComponent<SkillObject>();

                    skillObj.Name.text = Localization.instance.Localize("$skill_" + item.skillName.ToLower());
                    skillObj.icon.sprite = m_localPlayer.m_skills.GetSkill(skillType).m_info.m_icon;
                    skillObj.level.text = item.add.ToString();

                    skills.Add(skillObj);
                }
            }

            if (bonuses.Health == 0)
            {
                health.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                health.transform.parent.gameObject.SetActive(true);
                health.text = $"{bonuses.Health}";
            }
            if (bonuses.Eitr == 0)
            {
                eitr.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                eitr.transform.parent.gameObject.SetActive(true);
                eitr.text = $"{bonuses.Health}";
            }
            if (bonuses.HealthRegeneration == 0)
            {
                healthRegeneration.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                healthRegeneration.transform.parent.gameObject.SetActive(true);
                healthRegeneration.text = $"{bonuses.HealthRegeneration}%";
            }
            if (bonuses.EitrRegeneration == 0)
            {
                eitrRegeneration.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                eitrRegeneration.transform.parent.gameObject.SetActive(true);
                eitrRegeneration.text = $"{bonuses.EitrRegeneration}%";
            }
            if (bonuses.AllAttackSpeed == 0)
            {
                attackSpeedAll.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                attackSpeedAll.transform.parent.gameObject.SetActive(true);
                attackSpeedAll.text = $"{bonuses.AllAttackSpeed}%";
            }
            if (bonuses.MeleAttackSpeed == 0)
            {
                attackSpeedMele.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                attackSpeedMele.transform.parent.gameObject.SetActive(true);
                attackSpeedMele.text = $"{bonuses.MeleAttackSpeed}%";
            }
            if (bonuses.BowReloadTime == 0)
            {
                bowReloadTime.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                bowReloadTime.transform.parent.gameObject.SetActive(true);
                bowReloadTime.text = $"{bonuses.BowReloadTime}%";
            }
            if (bonuses.SpellAttackSpeed == 0)
            {
                attackSpeedSpell.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                attackSpeedSpell.transform.parent.gameObject.SetActive(true);
                attackSpeedSpell.text = $"{bonuses.SpellAttackSpeed}%";
            }
            if (bonuses.Defense == 0)
            {
                defense.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                defense.transform.parent.gameObject.SetActive(true);
                defense.text = $"{bonuses.Defense}%";
            }
            if (bonuses.AllDamageMod == 0)
            {
                damageModAll.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                damageModAll.transform.parent.gameObject.SetActive(true);
                damageModAll.text = $"{bonuses.AllDamageMod}%";
            }
            if (bonuses.MeleDamageMod == 0)
            {
                damageModMele.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                damageModMele.transform.parent.gameObject.SetActive(true);
                damageModMele.text = $"{bonuses.MeleDamageMod}%";
            }
            if (bonuses.BowDamageMod == 0)
            {
                damageModBow.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                damageModBow.transform.parent.gameObject.SetActive(true);
                damageModBow.text = $"{bonuses.BowDamageMod}%";
            }
            if (bonuses.SpellDamageMod == 0)
            {
                damageModSpell.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                damageModSpell.transform.parent.gameObject.SetActive(true);
                damageModSpell.text = $"{bonuses.SpellDamageMod}%";
            }
            if (bonuses.Stamina == 0)
            {
                stamina.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                stamina.transform.parent.gameObject.SetActive(true);
                stamina.text = bonuses.Stamina.ToString();
            }
            if (bonuses.StaminaRegeneration == 0)
            {
                staminaRegeneration.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                staminaRegeneration.transform.parent.gameObject.SetActive(true);
                staminaRegeneration.text = $"{bonuses.StaminaRegeneration}%";
            }
            if (bonuses.Armor == 0)
            {
                armor.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                armor.transform.parent.gameObject.SetActive(true);
                armor.text = bonuses.Armor.ToString();
            }
            if (bonuses.MoveSpeed == 0)
            {
                moveSpeed.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                moveSpeed.transform.parent.gameObject.SetActive(true);
                moveSpeed.text = $"{bonuses.MoveSpeed}%";
            }
            if (bonuses.Vampirism == 0)
            {
                vampirism.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                vampirism.transform.parent.gameObject.SetActive(true);
                vampirism.text = bonuses.Vampirism.ToString();
            }
            if (bonuses.ChanceToNotTakeDmg == 0)
            {
                chanceToNotTakeDmg.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                chanceToNotTakeDmg.transform.parent.gameObject.SetActive(true);
                chanceToNotTakeDmg.text = $"{bonuses.ChanceToNotTakeDmg}%";
            }
            if (bonuses.ChanceToReturnDmg == 0)
            {
                chanceToReturnDmg.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                chanceToReturnDmg.transform.parent.gameObject.SetActive(true);
                chanceToReturnDmg.text = $"{bonuses.ChanceToReturnDmg}%";
            }
            if (bonuses.ReturnDmg == 0)
            {
                returnDmg.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                returnDmg.transform.parent.gameObject.SetActive(true);
                returnDmg.text = $"{bonuses.ReturnDmg}%";
            }
            if (bonuses.ChanceToX2Dmg == 0)
            {
                x2DmgChance.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                x2DmgChance.transform.parent.gameObject.SetActive(true);
                x2DmgChance.text = $"{bonuses.ChanceToX2Dmg}%";
            }
            if (bonuses.MaxCarryWeight == 0)
            {
                weight.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                weight.transform.parent.gameObject.SetActive(true);
                weight.text = bonuses.MaxCarryWeight.ToString();
            }
        }
        internal void HideMenu()
        {
            menu?.SetActive(false);
        }
        internal void ShowMenu()
        {
            _self.Debug($"ShowMenu");
            InventoryGui.instance.Show(null);
            menu.SetActive(true);
            UpdateStats();
        }
    }
}