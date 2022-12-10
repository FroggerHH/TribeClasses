using Groups;
using HarmonyLib;
using System;
using UnityEngine;
using static TribeClasses.Plugin;
using static Player;
namespace TribeClasses
{
    public static class MonsterDeath_Path
    {
        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        public static class RegisterRpc
        {
            public static void Postfix()
            {
                ZRoutedRpc.instance.Register($"{ModName}_AddGroupExp", new Action<long, int, Vector3>(RPC_AddGroupExp));
            }
        }

        public static void RPC_AddGroupExp(long sender, int exp, Vector3 position)
        {
            if ((double)Vector3.Distance(position, Player.m_localPlayer.transform.position) >= 50f) return;

            LevelSystem.Instance.AddExp(exp);
        }

        [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
        internal static class GetExpFromMonster
        {
            private static void Postfix(ref HitData hit, Character __instance)
            {
                Character attacker = hit.GetAttacker();
                if (!__instance || !attacker || __instance.IsTamed() ||
                    __instance.IsPlayer() || !attacker.IsPlayer() ||
                     m_localPlayer != attacker ||
                    (m_localPlayer == attacker && !HaveClass()) ||
                    __instance.GetHealth() > 0f || __instance.IsDead())
                    return;
                int exp = 0;

                if (__instance.IsMonsterFaction() && (__instance.IsDead() || __instance.GetHealth() <= 0f))
                {
                    _self.Debug($"GetExpFromMonster 2");
                    exp = LevelSystem.Instance.GetExpFromMonster(__instance.gameObject.name) * __instance.GetLevel();
                    LevelSystem.Instance.AddExp(exp);
                }

                if (!API.IsLoaded()) return;
                _self.Debug($"Adding group exp...");

                float groupFactor = LevelSystem.Instance.groupExpFactor / 100;
                foreach (PlayerReference playerReference in API.GroupPlayers())
                {
                    if (playerReference.name != m_localPlayer.GetPlayerName())
                    {
                        float sendExp = exp * groupFactor;
                        ZRoutedRpc.instance.InvokeRoutedRPC(
                            playerReference.peerId,
                            $"{ModName}_AddGroupExp", new object[] { (int)sendExp });
                    }
                }

            }
        }
    }
}