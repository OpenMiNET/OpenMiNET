using MiNET.Plugins.Attributes;
using OpenAPI.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAPI.TestPlugin.FactionsExample
{
    public class FactionCommands
    {
        private FactionManager Manager { get; set; }

        public FactionCommands(FactionManager manager)
        {
            Manager = manager;
        }

        [Command(Name = "f rankme", Description = "Give yourself a factions rank!")]
        public void FactionRank(OpenPlayer player, FactionsPermissions rank)
        {
            Manager.SetPlayerPermission(player, rank);

            player.SendMessage($"There you go!");
        }

        [Command(Name = "f create", Description = "Create a faction!")]
        [FactionPermission(FactionsPermissions.None)]
        public void FactionCreate(OpenPlayer player)
        {
            player.SendMessage($"There you go!");
        }

        [Command(Name = "f home", Description = "TP to faction home!")]
        [FactionPermission(FactionsPermissions.Member)]
        public void FactionHome(OpenPlayer player)
        {
            player.SendMessage($"There you go!");
        }

        [Command(Name = "f kick", Description = "Kick a player!")]
        [FactionPermission(FactionsPermissions.Mod)]
        public void FactionKick(OpenPlayer player)
        {
            player.SendMessage($"There you go!");
        }

        [Command(Name = "f delete", Description = "Delete your faction!")]
        [FactionPermission(FactionsPermissions.Admin)]
        public void FactionDelete(OpenPlayer player)
        {
            player.SendMessage($"There you go!");
        }
    }
}
