using HarmonyLib;
using System;
using UnityEngine;
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
            LevelSystem.Instance.EitrAdded = bonuses.Eitr;
        }

        [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_CharacterID)), HarmonyPostfix]
        internal static void SetZDOPeer()
        {
            foreach(ZNetPeer peer in ZNet.instance.m_peers)
            {
                ZDOMan.instance.ForceSendZDO(peer.m_characterID);
            }
        }

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

                    attacker.Heal(bonuses.Vampirism);
                }
            }
            #endregion

            return true;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetBodyArmor)), HarmonyPostfix]
        private static void AddArmorANDAplyDefense(ref float __result, Player __instance)
        {
            if(__instance != m_localPlayer || !HaveClass())
            {
                return;
            }

            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
            __result += bonuses.Armor;

            float Defense = bonuses.Defense;
            if(Defense > 0)
            {
                __result *= Defense / 100 + 1;
            }
            else if(Defense < 0)
            {
                __result *= Defense * -1 / 100;
            }
        }

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
            Bonuses? bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                _self.DebugError("AddAttackSpeedALL: bonuses == null");
                return;
            }
            if(bonuses.AllAttackSpeed > 0)
            {
                __instance.m_animator.speed += __instance.m_animator.speed * bonuses.AllAttackSpeed / 100;
            }
            else if(bonuses.AllAttackSpeed < 0)
            {
                __instance.m_animator.speed -= __instance.m_animator.speed * bonuses.AllAttackSpeed * -1 / 100;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Update)), HarmonyPostfix]
        private static void AddAttackSpeedSpell(Player __instance)
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
            Bonuses? bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                _self.DebugError("SpellAttackSpeed: bonuses == null");
                return;
            }

            if(__instance.m_currentAttack.m_weapon.m_shared.m_animationState != Staves)
            {
                return;
            }

            if(bonuses.SpellAttackSpeed > 0)
            {
                __instance.m_animator.speed += __instance.m_animator.speed * bonuses.SpellAttackSpeed / 100;
            }
            else if(bonuses.SpellAttackSpeed < 0)
            {
                __instance.m_animator.speed -= __instance.m_animator.speed * bonuses.SpellAttackSpeed * -1 / 100;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Update)), HarmonyPostfix]
        private static void AddAttackSpeedMele(Player __instance)
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
            Bonuses? bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                _self.DebugError("SpellAttackSpeed: bonuses == null");
                return;
            }

            SkillType skill = __instance.m_currentAttack.m_weapon.m_shared.m_skillType;
            if(skill != SkillType.Axes &&
                skill != SkillType.Swords &&
                skill != SkillType.Knives &&
                skill != SkillType.Clubs &&
                skill != SkillType.Spears &&
                skill != SkillType.Polearms)
            {
                return;
            }

            if(bonuses.MeleAttackSpeed > 0)
            {
                __instance.m_animator.speed += __instance.m_animator.speed * bonuses.MeleAttackSpeed / 100;
            }
            else if(bonuses.MeleAttackSpeed < 0)
            {
                __instance.m_animator.speed -= __instance.m_animator.speed * bonuses.MeleAttackSpeed * -1 / 100;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Update)), HarmonyPostfix]
        private static void AddBowReloadTime(Player __instance)
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
            Bonuses? bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                return;
            }

            SkillType skill = __instance.m_currentAttack.m_weapon.m_shared.m_skillType;
            if(skill != SkillType.Bows)
            {
                return;
            }

            if(bonuses.BowReloadTime > 0)
            {
                m_localPlayer.GetCurrentWeapon().m_shared.m_attack.m_reloadTime *= bonuses.BowReloadTime / 100;
            }
            else if(bonuses.BowReloadTime < 0)
            {
                m_localPlayer.GetCurrentWeapon().m_shared.m_attack.m_reloadTime /= bonuses.BowReloadTime * -1 / 100;
            }
        }

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

                if(eff > 0)
                {
                    eff = Mathf.Min(1, eff / 100f);
                    if(__instance.m_character.IsSwiming())
                    {
                        speed *= 1 + eff * 0.5f;
                    }
                    else
                    {
                        speed *= 1 + eff * 0.5f;
                    }
                }
                else if(eff < 0)
                {
                    eff = Mathf.Min(1, eff / 100f);
                    if(__instance.m_character.IsSwiming())
                    {

                        speed *= eff * 0.5f;
                    }
                    else
                    {
                        speed *= eff;
                    }
                }

                if(__instance.m_character.IsSwiming())
                {
                    speed += baseSpeed * bonuses.MoveSpeed / 100f * 0.5f;
                }
                else
                {
                    speed += baseSpeed * bonuses.MoveSpeed / 100f;
                }
                if(speed < 0f)
                {
                    speed = 0f;
                }
            }
        }

        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyHealthRegen)), HarmonyPostfix]
        private static void AddHealthRegen(ref float regenMultiplier, SEMan __instance)
        {
            if(__instance.m_character.IsPlayer() && __instance.m_character == m_localPlayer && HaveClass())
            {
                Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
                float eff = bonuses.HealthRegeneration;

                if(eff > 0)
                {
                    eff = Mathf.Min(1, eff / 100f);
                    regenMultiplier *= 1 + eff;
                }
                else if(eff < 0)
                {
                    eff = Mathf.Min(1, eff / 100f);
                    regenMultiplier *= eff;
                }
            }
        }

        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyEitrRegen)), HarmonyPostfix]
        private static void AddEitrRegen(ref float eitrMultiplier, SEMan __instance)
        {
            if(__instance.m_character.IsPlayer() && __instance.m_character == m_localPlayer && HaveClass())
            {
                Bonuses bonuses = LevelSystem.Instance.GetFullBonuses();
                float eff = bonuses.EitrRegeneration;

                if(eff > 0)
                {
                    eff = Mathf.Min(1, eff / 100f);
                    eitrMultiplier *= 1 + eff;
                }
                else if(eff < 0)
                {
                    eff = Mathf.Min(1, eff / 100f);
                    eitrMultiplier *= eff;
                }
            }
        }

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

                if(eff > 0)
                {
                    eff = Mathf.Min(1, eff / 100f);
                    staminaMultiplier *= 1 + eff;
                }
                else if(eff < 0)
                {
                    eff = Mathf.Min(1, eff / 100f);
                    staminaMultiplier *= eff;
                }
            }
        }

        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetWeaponLoadingTime)), HarmonyPostfix]
        private static void BowReloadTime(ref float __result)
        {
            Bonuses bonuses = LevelSystem.Instance.GetFullBonuses(); if(bonuses == null)
            {
                return;
            }
            float eff = bonuses.BowReloadTime; // например 20, если увеличить, -20, если уменьшить

            if(eff > 0)
            {
                eff = Mathf.Min(1, eff / 100f);
                __result *= 1 + eff;
            }
            else if(eff < 0)
            {
                eff = Mathf.Min(1, eff / 100f);
                __result *= eff;
            }

        }


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
                float damageMod;
                SkillType skill = m_localPlayer.GetCurrentWeapon().m_shared.m_skillType;
                if(skill == SkillType.Axes ||
                    skill == SkillType.Swords ||
                    skill == SkillType.Knives ||
                    skill == SkillType.Clubs ||
                    skill == SkillType.Spears ||
                    skill == SkillType.Polearms)
                {
                    damageMod = bonuses.MeleDamageMod;
                }
                else
                if(skill == SkillType.BloodMagic ||
                    skill == SkillType.ElementalMagic)
                {
                    damageMod = bonuses.SpellDamageMod + bonuses.AllDamageMod;
                }
                else
                if(skill == SkillType.Bows)
                {
                    damageMod = bonuses.BowDamageMod + bonuses.AllDamageMod;
                }
                else
                {
                    damageMod = bonuses.AllDamageMod;
                }

                if(damageMod > 0)
                {
                    hit.m_damage.Modify(damageMod / 100 + 1);
                }
                else if(damageMod < 0)
                {
                    if(damageMod <= -101)
                    {
                        hit.m_damage.Modify(0);
                    }
                    else
                    {
                        hit.m_damage.Modify(damageMod * -1 / 100);
                    }
                }
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
                    hit2.m_damage.m_blunt = hit.m_damage.m_blunt * bonuses.ReturnDmg / 100;
                    hit2.m_damage.m_chop = hit.m_damage.m_chop * bonuses.ReturnDmg / 100;
                    hit2.m_damage.m_damage = hit.m_damage.m_damage * bonuses.ReturnDmg / 100;
                    hit2.m_damage.m_fire = hit.m_damage.m_fire * bonuses.ReturnDmg / 100;
                    hit2.m_damage.m_frost = hit.m_damage.m_frost * bonuses.ReturnDmg / 100;
                    hit2.m_damage.m_lightning = hit.m_damage.m_lightning * bonuses.ReturnDmg / 100;
                    hit2.m_damage.m_pickaxe = hit.m_damage.m_pickaxe * bonuses.ReturnDmg / 100;
                    hit2.m_damage.m_pierce = hit.m_damage.m_pierce * bonuses.ReturnDmg / 100;
                    hit2.m_damage.m_poison = hit.m_damage.m_poison * bonuses.ReturnDmg / 100;
                    hit2.m_damage.m_slash = hit.m_damage.m_slash * bonuses.ReturnDmg / 100;
                    hit2.m_damage.m_spirit = hit.m_damage.m_spirit * bonuses.ReturnDmg / 100;
                    hit2.SetAttacker(m_localPlayer);
                    attacker.Damage(hit2);
                    _self.Debug($"ReturnDmg = {hit2.m_damage.GetTotalDamage()}");
                }
            }
            #endregion

            return true;

        }
    }
}
