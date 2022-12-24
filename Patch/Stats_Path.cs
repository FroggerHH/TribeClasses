using HarmonyLib;
using System;
using UnityEngine.SceneManagement;
using static ItemDrop.ItemData.AnimationState;
using static Player;
using static Skills;
using static TribeClasses.LevelsSystemTree;
using static TribeClasses.Plugin;
using Random = UnityEngine.Random;

namespace TribeClasses
{
    internal class Stats_Path
    {
        [HarmonyPatch(typeof(Player), nameof(Player.Start)), HarmonyPostfix]
        [HarmonyPriority(1000)]
        internal static void PlayerStart(Player __instance)
        {
            if(__instance != m_localPlayer)
            {
                return;
            }

            if(SceneManager.GetActiveScene().name == "main" && HaveClass())
            {
                LevelSystem.Instance.Load();
            }

            LevelSystem.Instance.LastNotTakeDmg = DateTime.Now;
            LevelSystem.Instance.LastReturnDmg = DateTime.Now;
            LevelSystem.Instance.LastX2Damage = DateTime.Now;
            LevelSystem.Instance.LastSuper = DateTime.Now;

            __instance.m_nview.Register("ApplySuper", new Action<long, string>(LevelSystem.Instance.RPC_ApplySuper));
        }

        [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_CharacterID)), HarmonyPostfix]
        internal static void SetZDOPeer()
        {
            foreach(ZNetPeer peer in ZNet.instance.m_peers)
            {
                ZDOMan.instance.ForceSendZDO(peer.m_characterID);
            }
        }

        #region Eitr
        [HarmonyPatch(typeof(Player), nameof(Player.SetMaxEitr)), HarmonyPrefix]
        [HarmonyPriority(1000)]
        internal static void PlayerSetMaxEitr(ref float eitr, Player __instance)
        {
            if(__instance != m_localPlayer)
            {
                return;
            }

            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
            if(bonuses == null)
            {
                return;
            }

            eitr += bonuses.Eitr;
        }
        #endregion
        #region EitrRegen
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyEitrRegen)), HarmonyPostfix]
        private static void AddEitrRegen(ref float eitrMultiplier, SEMan __instance)
        {
            if(__instance.m_character && __instance.m_character.IsPlayer() && __instance.m_character == m_localPlayer && HaveClass())
            {
                Bonuses bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null) return;
                float eff = bonuses.EitrRegeneration;
                if(eff <= 0) return;
                if(eff > 0)
                {
                    eitrMultiplier += eitrMultiplier * eff * 0.01f;
                }
                if(eff < 0)
                {
                    eitrMultiplier -= eitrMultiplier * eff * 0.01f;
                }
            }
        }
        #endregion

        #region Health
        [HarmonyPatch(typeof(Player), nameof(Player.SetMaxHealth)), HarmonyPrefix]
        [HarmonyPriority(1000)]
        internal static void PlayerSetMaxHealth(ref float health, Player __instance)
        {
            if(__instance != m_localPlayer)
            {
                return;
            }

            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
            if(bonuses == null)
            {
                return;
            }

            health += bonuses.Health;
        }
        #endregion
        #region HealthRegen
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyHealthRegen)), HarmonyPostfix]
        private static void AddHealthRegen(ref float regenMultiplier, SEMan __instance)
        {
            if(__instance.m_character.IsPlayer() && __instance.m_character == m_localPlayer && HaveClass())
            {
                Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
                float eff = bonuses.HealthRegeneration;
                if(eff <= 0) return;
                if(eff > 0)
                {
                    regenMultiplier = regenMultiplier + regenMultiplier * eff * 0.01f;
                }
                if(eff < 0)
                {
                    regenMultiplier = regenMultiplier - regenMultiplier * eff * 0.01f;
                }
            }
        }
        #endregion

        #region Stamina
        [HarmonyPatch(typeof(Player), nameof(Player.SetMaxStamina)), HarmonyPrefix]
        [HarmonyPriority(1000)]
        internal static void PlayerSetMaxStamina(ref float stamina, Player __instance)
        {
            if(__instance != m_localPlayer)
            {
                return;
            }

            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
            if(bonuses == null)
            {
                return;
            }

            stamina += bonuses.Stamina;
        }
        #endregion
        #region StaminaRegen
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyStaminaRegen)), HarmonyPostfix]
        private static void AddStaminaRegen(ref float staminaMultiplier, SEMan __instance)
        {
            if(__instance != null && __instance.m_character.IsPlayer() && __instance.m_character == m_localPlayer && HaveClass())
            {
                Bonuses bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
                {
                    return;
                }
                float eff = bonuses.StaminaRegeneration;
                if(eff <= 0) return;
                if(eff > 0)
                {
                    staminaMultiplier = staminaMultiplier + staminaMultiplier * eff * 0.01f;
                }
                if(eff < 0)
                {
                    staminaMultiplier = staminaMultiplier - staminaMultiplier * eff * 0.01f;
                }
            }
        }
        #endregion

        #region Characters
        [HarmonyPatch(typeof(Character), nameof(Character.Damage)), HarmonyPrefix]
        private static bool CharacterDeathAndDamage(HitData hit, Character __instance)
        {
            int random = Random.Range(0, 101);
            #region NotTakeDmg
            if(__instance.IsPlayer() && __instance == m_localPlayer && HaveClass())
            {
                Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();

                if(random < bonuses.ChanceToNotTakeDmg && (DateTime.Now - LevelSystem.Instance.LastNotTakeDmg).TotalSeconds >= 1.0)
                {
                    LevelSystem.Instance.LastNotTakeDmg = DateTime.Now;
                    _self.Debug($"CharacterDeathAndDamage Prefix ChanceToNotTakeDmg");
                    return false;
                }
            }
            #endregion

            #region Vampirism
            if(__instance.IsMonsterFaction() && HaveClass())
            {
                Character attacker = hit.GetAttacker();

                if(attacker && attacker.IsPlayer() && m_localPlayer == attacker && HaveClass())
                {
                    Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
                    if(bonuses == null) return true;

                    attacker.Heal(hit.GetTotalDamage() * bonuses.Vampirism * 0.01f);
                }
            }
            #endregion

            return true;
        }
        #endregion

        #region Armor
        [HarmonyPatch(typeof(Player), nameof(Player.GetBodyArmor)), HarmonyPostfix]
        private static void AddANDAplyDefense(ref float __result, Player __instance)
        {
            if(__instance != m_localPlayer || !HaveClass())
            {
                return;
            }

            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
            __result += bonuses.Armor;

            float eff = bonuses.Defense; //10% , armor = 15
            if(eff <= 0) return;
            if(eff > 0)
            {
                __result = __result + __result * eff * 0.01f;
            }
            if(eff < 0)
            {
                __result = __result - __result * eff * 0.01f;
            }
        }
        #endregion

        #region AttackSpeedALL
        [HarmonyPatch(typeof(Player), nameof(Player.Update)), HarmonyPostfix]
        private static void AddAttackSpeedALL(Player __instance)
        {
            if(__instance != m_localPlayer || !HaveClass())
            {
                return;
            }

            if(!__instance.InAttack())
            {
                __instance.m_animator.speed = 1;
                return;
            }
            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                return;
            }
            float eff = bonuses.AllAttackSpeed;
            if(eff <= 0) return;
            if(eff > 0)
            {
                __instance.m_animator.speed = __instance.m_animator.speed + __instance.m_animator.speed * eff * 0.01f;
            }
            if(eff < 0)
            {
                __instance.m_animator.speed = __instance.m_animator.speed - __instance.m_animator.speed * eff * 0.01f;
            }
        }
        #endregion

        #region AttackSpeedSpell
        [HarmonyPatch(typeof(Player), nameof(Player.Update)), HarmonyPostfix]
        private static void AddAttackSpeedSpell(Player __instance)
        {
            if(!__instance || __instance != m_localPlayer || !HaveClass())
            {
                return;
            }
            if(!__instance.InAttack())
            {
                __instance.m_animator.speed = 1;
                return;
            }
            Bonuses? bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                _self.DebugError("SpellAttackSpeed: bonuses == null");
                return;
            }
            if(__instance?.m_currentAttack?.m_weapon.m_shared.m_animationState != Staves)
            {
                return;
            }

            float eff = bonuses.SpellAttackSpeed;
            if(eff <= 0) return;
            if(eff > 0)
            {
                __instance.m_animator.speed = __instance.m_animator.speed + __instance.m_animator.speed * eff * 0.01f;
            }
            if(eff < 0)
            {
                __instance.m_animator.speed = __instance.m_animator.speed - __instance.m_animator.speed * eff * 0.01f;
            }
        }
        #endregion

        #region AttackSpeedMele
        [HarmonyPatch(typeof(Player), nameof(Player.Update)), HarmonyPostfix]
        private static void AddAttackSpeedMele(Player __instance)
        {
            if(!__instance || __instance != m_localPlayer || !HaveClass() || !__instance.m_animator)
            {
                return;
            }
            if(!__instance.InAttack())
            {
                __instance.m_animator.speed = 1;
                return;
            }
            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                return;
            }

            SkillType? skill = __instance.m_currentAttack?.m_weapon?.m_shared.m_skillType; if(skill == null)
            {
                return;
            }
            if(skill != SkillType.Axes &&
                skill != SkillType.Swords &&
                skill != SkillType.Knives &&
                skill != SkillType.Clubs &&
                skill != SkillType.Spears &&
                skill != SkillType.Polearms)
            {
                return;
            }
            float eff = bonuses.MeleAttackSpeed;
            if(eff <= 0) return;
            if(eff > 0)
            {
                __instance.m_animator.speed = __instance.m_animator.speed + __instance.m_animator.speed * eff * 0.01f;
            }
            if(eff < 0)
            {
                __instance.m_animator.speed = __instance.m_animator.speed - __instance.m_animator.speed * eff * 0.01f;
            }
        }
        #endregion

        #region Speed
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ApplyStatusEffectSpeedMods)), HarmonyPostfix]
        private static void AddSpeed(ref float speed, SEMan __instance)
        {
            if(__instance.m_character.IsPlayer() && __instance.m_character == m_localPlayer && HaveClass())
            {
                float baseSpeed = speed;
                Bonuses bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
                {
                    return;
                }
                float eff = bonuses.MoveSpeed;
                if(eff <= 0) return;

                if(eff > 0)
                {
                    if(__instance.m_character.IsSwiming())
                        speed = (speed + speed * eff * 0.01f) * 0.5f;
                    else
                        speed = speed + speed * eff * 0.01f;
                }
                if(eff < 0)
                {
                    if(__instance.m_character.IsSwiming())
                        speed = (speed - speed * eff * 0.01f) * 0.5f;
                    else
                        speed = speed - speed * eff * 0.01f;
                }
            }
        }
        #endregion

        #region BowReloadTime
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetWeaponLoadingTime)), HarmonyPostfix]
        private static void BowReloadTime(ref float __result)
        {
            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                return;
            }
            float eff = bonuses.BowReloadTime; // например 20, если увеличить, -20, если уменьшить
            if(eff <= 0) return;
            if(eff > 0)
            {
                __result = __result - __result * eff * 0.01f;
            }
            if(eff < 0)
            {
                __result = __result + __result * eff * 0.01f;
            }
        }
        #endregion

        #region DrawStaminaDrain
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetDrawStaminaDrain)), HarmonyPostfix]
        private static void DrawStaminaDrain(ref float __result)
        {
            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                return;
            }
            float eff = bonuses.DrawStaminaDrain;
            if(eff <= 0) return;
            if(eff > 0)
            {
                __result = __result - __result * eff * 0.01f;
            }
            if(eff < 0)
            {
                __result = __result + __result * eff * 0.01f;
            }
        }
        #endregion

        #region DrawDruration
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GetAttackDrawPercentage)), HarmonyPostfix]
        private static void DrawDruration(ref float __result)
        {
            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                return;
            }
            float eff = bonuses.DrawDrurationMin;
            if(eff <= 0) return;
            if(eff > 0)
            {
                __result = __result - __result * eff * 0.01f;
            }
            if(eff < 0)
            {
                __result = __result + __result * eff * 0.01f;
            }
        }
        #endregion

        #region NoAmmo
        [HarmonyPatch(typeof(Attack), nameof(Attack.UseAmmo)), HarmonyPostfix]
        private static void NoAmmo(Attack __instance, ItemDrop.ItemData ammoItem)
        {
            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                return;
            }
            bool flag = bonuses.NoAmmo;
            if(!flag)
            {
                return;
            }
            if(ammoItem == null || !ammoItem.m_dropPrefab)
            {
                return;
            }

            //string ammoName = ammoItem?.m_dropPrefab?.name;
            //_self.Debug($"ammo {ammoName}");
            //if(!string.IsNullOrEmpty(ammoName)) __instance.m_character.Pickup(ObjectDB.instance.GetItemPrefab(ammoName));
        }
        #endregion

        #region Skills
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifySkillLevel)), HarmonyPrefix]
        private static void ModifySkillLevel(SkillType skill, ref float level, SEMan __instance)
        {
            if(__instance.m_character.IsPlayer() && __instance.m_character == m_localPlayer && HaveClass())
            {
                Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();

                foreach(Bonuses.ModifySkill item in bonuses.m_ModifySkill)
                {
                    SkillType skillType = SkillTypeFromName(item.skillName);
                    if(skillType == SkillType.None)
                    {
                        return;
                    }

                    if(skillType == SkillType.All || skillType == skill)
                    {
                        level += item.add;
                    }
                }
            }
        }
        #endregion

        #region Damage
        [HarmonyPatch(typeof(Character), nameof(Character.Damage)), HarmonyPrefix]
        private static bool ModifyDamage(HitData hit, Character __instance)
        {
            Character attacker = hit.GetAttacker();
            if(!attacker)
            {
                return true;
            }

            #region X2Damage
            int random = Random.Range(0, 101);
            if(hit.GetAttacker().IsPlayer() && hit.GetAttacker() == m_localPlayer && HaveClass())
            {
                Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
                _self.Debug($"ModifyDamage random = {random}");

                if(random < bonuses.ChanceToX2Dmg && (DateTime.Now - LevelSystem.Instance.LastX2Damage).TotalSeconds >= 1.0)
                {
                    float total = hit.GetTotalDamage();
                    hit.m_damage.Modify(2);
                    LevelSystem.Instance.LastX2Damage = DateTime.Now;
                    _self.Debug($"X2Damage = {hit.GetTotalDamage()}, old = {total} random = {random}");
                }

                #region DamageMod
                float eff;
                SkillType skill = m_localPlayer.GetCurrentWeapon().m_shared.m_skillType;
                if(skill == SkillType.Axes ||
                    skill == SkillType.Swords ||
                    skill == SkillType.Knives ||
                    skill == SkillType.Clubs ||
                    skill == SkillType.Spears ||
                    skill == SkillType.Polearms)
                {
                    eff = bonuses.MeleDamageMod;
                }
                else
                if(skill == SkillType.BloodMagic ||
                    skill == SkillType.ElementalMagic)
                {
                    eff = bonuses.SpellDamageMod + bonuses.AllDamageMod;
                }
                else
                if(skill == SkillType.Bows)
                {
                    eff = bonuses.BowDamageMod + bonuses.AllDamageMod;
                }
                else
                {
                    eff = bonuses.AllDamageMod;
                }

                float resultEff = 1;
                if(eff > 0)
                {
                    resultEff = hit.GetTotalDamage() + hit.GetTotalDamage() * eff * 0.01f;
                }
                if(eff < 0)
                {
                    resultEff = hit.GetTotalDamage() - hit.GetTotalDamage() * eff * 0.01f;
                }

                hit.m_damage.Modify(resultEff);
                #endregion
            }
            #endregion

            #region ReturnDmg
            if(__instance.IsPlayer() && __instance == m_localPlayer && HaveClass())
            {
                Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();

                if(random < bonuses.ChanceToReturnDmg && (DateTime.Now - LevelSystem.Instance.LastReturnDmg).TotalSeconds >= 1.0)
                {
                    LevelSystem.Instance.LastReturnDmg = DateTime.Now;
                    HitData hit2 = hit.Clone();
                    float eff = bonuses.ReturnDmg;
                    float resultEff = 1;
                    if(eff > 0)
                    {
                        resultEff = hit.GetTotalDamage() + hit.GetTotalDamage() * eff * 0.01f;
                    }
                    if(eff < 0)
                    {
                        resultEff = hit.GetTotalDamage() - hit.GetTotalDamage() * eff * 0.01f;
                    }
                    hit2.m_damage.m_damage = resultEff;

                    hit2.SetAttacker(m_localPlayer);
                    attacker.Damage(hit2);
                    _self.Debug($"ReturnDmg = {hit2.m_damage.GetTotalDamage()}");
                }
            }
            #endregion

            return true;

        }
        #endregion
    }
}
