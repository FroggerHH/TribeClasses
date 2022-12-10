using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TribeClasses
{
    public static class TerminalCommands
    {
        private static bool isServer => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        private static string modName => Plugin.ModName;

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        internal static class ZrouteMethodsServerFeedback
        {
            private static void Postfix()
            {
                if (isServer) return;
                ZRoutedRpc.instance.Register($"{modName} terminal_SetLevel",
                    new Action<long, int>(RPC_SetLevel));
                ZRoutedRpc.instance.Register($"{modName} terminal_ResetClass",
                    new Action<long>(RPC_ResetClass));
                ZRoutedRpc.instance.Register($"{modName} terminal_AddExp",
                    new Action<long, int>(RPC_AddExp));
                ZRoutedRpc.instance.Register($"{modName} terminal_ResetSuper",
                    new Action<long>(RPC_ResetSuper));
            }
        }

        private static void RPC_SetLevel(long sender, int level)
        {
            LevelSystem.Instance.SetLevel(level);
        }
        private static void RPC_ResetClass(long sender)
        {
            Plugin.ResetClass();
        }
        private static void RPC_AddExp(long sender, int count)
        {
            LevelSystem.Instance.AddExp(count);
        }
        private static void RPC_ResetSuper(long sender)
        {
            LevelSystem.Instance.RemooveSuperBonuses();
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))] 
        internal class AddChatCommands
        {
            private static void Postfix()
            {
                long? getPlayerId(string name)
                {
                    if (name == null || name == string.Empty)
                    {
                        name = Player.m_localPlayer.GetPlayerName();
                    }
                    string clearName = name.Replace('&', ' ');
                    clearName = name.Replace('_', ' ');
                    List<ZNet.PlayerInfo> players = ZNet.instance.GetPlayerList();
                    foreach (ZNet.PlayerInfo playerInfo in players)
                    {
                        if (playerInfo.m_name == clearName)
                        {
                            return playerInfo.m_characterID.m_userID;
                        }
                    }
                    return null;
                }
                _ = new Terminal.ConsoleCommand(modName, $"Manages the {modName.Replace(".", "")} commands.",

                        args =>
                        {
                            if (!Plugin.configSync.IsAdmin && !ZNet.instance.IsServer())
                            {
                                args.Context.AddString("You are not an admin on this server.");
                                return;
                            }

                            if (args.Length > 3 && args.Length <= 4 && args[1] == "level")
                            {
                                int level = int.Parse(args[2]);
                                string name = args[3];
                                long? userId = getPlayerId(name);
                                if (userId == null)
                                {
                                    userId = Player.m_localPlayer.GetPlayerID();
                                }
                                ZRoutedRpc.instance.InvokeRoutedRPC(userId ?? 200, $"{modName} terminal_SetLevel", level);
                            }
                            else if (args.Length > 2 && args.Length <= 3 && args[1] == "reset_class")
                            {
                                string name = args[2];
                                long? userId = getPlayerId(name);
                                if (userId == null)
                                {
                                    userId = Player.m_localPlayer.GetPlayerID();
                                }
                                ZRoutedRpc.instance.InvokeRoutedRPC(userId ?? 200, $"{modName} terminal_ResetClass");
                            }
                            else if (args.Length > 2 && args.Length <= 3 && args[1] == "reset_super")
                            {
                                string name = args[2];
                                long? userId = getPlayerId(name);
                                if (userId == null)
                                {
                                    userId = Player.m_localPlayer.GetPlayerID();
                                }
                                ZRoutedRpc.instance.InvokeRoutedRPC(userId ?? 200, $"{modName} terminal_ResetSuper");
                            }
                            else if (args.Length > 3 && args.Length <= 4 && args[1] == "add_exp")
                            {
                                int count = int.Parse(args[2]);
                                string name = args[3];
                                long? userId = getPlayerId(name);
                                if (userId == null)
                                {
                                    userId = Player.m_localPlayer.GetPlayerID();
                                }
                                ZRoutedRpc.instance.InvokeRoutedRPC(userId ?? 200, $"{modName} terminal_AddExp", count);
                            }

                            args.Context.AddString("level [value] [name] - set level for player name");
                            args.Context.AddString("reset_class [name] - reset class for player name");
                            args.Context.AddString("add_exp [value] [name] - add exp for player name");
                            args.Context.AddString("reset_super [name] - remove super and reset its cooldown for player name");
                        },
                    optionsFetcher: () => new List<string>
                        { "level", "reset_class", "add_exp", "reset_super" });
            }

        }
    }
}