using System;
using System.Collections.Generic;
using static TribeClasses.LevelsSystemTree.Bonuses;

namespace TribeClasses
{
    [Serializable]
    public partial class LevelsSystemTree
    {
        public List<ClassInfo> blocks = new();

        [Serializable]
        public class ClassInfo
        {
            public string className = "";
            public List<LevelInfo> levelTree = new();
            public float levelExpModifier = 1.04f;
            public Bonuses everyLevelBonuses = new();
            public List<string> dualWieldExcludedTypesOfWeapons = new();
            public List<string> dualWieldExclusionCertainItems = new();
            public Super.SuperData super = new();
        }
        [Serializable]
        public class LevelInfo
        {
            public int level = 999;
            public Bonuses bonuses = new();

            public bool Modify(Bonuses NewBonuses, int count = 1)
            {
                if (NewBonuses == null) return false;

                bonuses.Health += NewBonuses.Health * count;
                bonuses.Eitr += NewBonuses.Eitr * count;
                bonuses.HealthRegeneration += NewBonuses.HealthRegeneration * count;
                bonuses.EitrRegeneration += NewBonuses.EitrRegeneration * count;
                bonuses.Stamina += NewBonuses.Stamina * count;
                bonuses.StaminaRegeneration += NewBonuses.StaminaRegeneration * count;
                bonuses.Armor += NewBonuses.Armor * count;
                bonuses.Defense += NewBonuses.Defense * count;
                bonuses.MoveSpeed += NewBonuses.MoveSpeed * count;
                bonuses.Vampirism += NewBonuses.Vampirism * count;
                bonuses.ChanceToNotTakeDmg += NewBonuses.ChanceToNotTakeDmg * count;
                bonuses.ChanceToReturnDmg += NewBonuses.ChanceToReturnDmg * count;
                bonuses.ReturnDmg += NewBonuses.ReturnDmg * count;
                bonuses.ChanceToX2Dmg += NewBonuses.ChanceToX2Dmg * count;
                bonuses.MaxCarryWeight += NewBonuses.MaxCarryWeight * count;
                bonuses.AllAttackSpeed += NewBonuses.AllAttackSpeed * count;
                bonuses.SpellAttackSpeed += NewBonuses.SpellAttackSpeed * count;
                bonuses.MeleAttackSpeed += NewBonuses.MeleAttackSpeed * count;
                bonuses.BowAttackSpeed += NewBonuses.BowAttackSpeed * count;
                bonuses.AllDamageMod += NewBonuses.AllDamageMod * count;
                bonuses.MeleDamageMod += NewBonuses.MeleDamageMod * count;
                bonuses.BowDamageMod += NewBonuses.BowDamageMod * count;
                bonuses.SpellDamageMod += NewBonuses.SpellDamageMod * count;
                if (NewBonuses.unlockSuper) bonuses.unlockSuper = true;

                if (bonuses.ChanceToNotTakeDmg > 100) bonuses.ChanceToNotTakeDmg = 100;
                if (bonuses.ChanceToReturnDmg > 100) bonuses.ChanceToReturnDmg = 100;
                if (bonuses.ChanceToX2Dmg > 100) bonuses.ChanceToX2Dmg = 100;

                foreach (ModifySkill item in NewBonuses.m_ModifySkill) if (!bonuses.m_ModifySkill.Contains(item)) bonuses.m_ModifySkill.Add(item);

                return true;
            }
        }
        [Serializable]
        public class Bonuses
        {
            public float Health = 0;
            public float Eitr = 0;
            public float HealthRegeneration = 0;
            public float EitrRegeneration = 0;
            public float Stamina = 0;
            public float StaminaRegeneration = 0;
            public int Armor = 0;
            public int Defense = 0;
            public float MoveSpeed = 0;
            public float Vampirism = 0;
            public float ChanceToNotTakeDmg = 0;
            public float ChanceToReturnDmg = 0;
            public int ReturnDmg = 0;
            public float ChanceToX2Dmg = 0;
            public float AllAttackSpeed = 0;
            public float SpellAttackSpeed = 0;
            public float MeleAttackSpeed = 0;
            public float BowAttackSpeed = 0;
            public int MaxCarryWeight = 0;
            public float AllDamageMod = 0;
            public float MeleDamageMod = 0;
            public float BowDamageMod = 0;
            public float SpellDamageMod = 0;
            public List<ModifySkill> m_ModifySkill = new();
            public bool unlockSuper = false;

            [Serializable]
            public class ModifySkill
            {
                public string skillName = Skills.SkillType.None.ToString();
                public float add = 0;
            }
        }
        [Serializable]
        public class Super
        {
            public long owner;
            public SuperData data = new()
            {
                name = "none",
                range = 10,
                cooldown = 15,
                time = 5,
                bonuses = new()
            };

            [Serializable]
            public struct SuperData
            {
                public string name;
                public float range;
                public float cooldown;
                public float time;
                public Bonuses bonuses;
            }

            internal bool IsMine()
            {
                return owner == ZNet.instance?.GetUID();
            }
            internal Super(SuperData data)
            {
                this.data = data;
            }
            internal long SetOwner(long sender = 0)
            {
                if (ZNet.instance && sender == 0) owner = ZNet.instance.GetUID();
                else owner = sender;
                return owner;
            }
        }
    }
}
