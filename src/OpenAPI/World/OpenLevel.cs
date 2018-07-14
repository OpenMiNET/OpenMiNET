using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using log4net;
using MiNET;
using MiNET.BlockEntities;
using MiNET.Blocks;
using MiNET.Entities;
using MiNET.Items;
using MiNET.Net;
using MiNET.Worlds;
using OpenAPI.Events;
using OpenAPI.Events.Block;
using OpenAPI.Events.Level;
using OpenAPI.Events.Player;
using OpenAPI.Player;
using OpenAPI.Utils;

namespace OpenAPI.World
{
	public class OpenLevel : Level
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenLevel));
		
		public EventDispatcher EventDispatcher { get; }
		public TickScheduler TickScheduler { get; }
		public OpenAPI OpenAPI { get; }
		private CancellationTokenSource CancelationToken { get; }
		public OpenLevel(OpenAPI openApi, 
			OpenLevelManager levelManager,
			string levelId,
			IWorldProvider worldProvider, 
			EntityManager entityManager,
			GameMode gameMode = GameMode.Survival,
			Difficulty difficulty = Difficulty.Normal,
			int viewDistance = 11) 
			: base(levelManager, levelId, worldProvider, entityManager, gameMode, difficulty, viewDistance)
		{
			OpenAPI = openApi;
			CancelationToken = new CancellationTokenSource();
			TickScheduler = new TickScheduler();

			EventDispatcher = new EventDispatcher(openApi, OpenAPI.EventDispatcher);
		}

		private bool _closed;
	/*	public override void AddEntity(Entity entity)
		{
		    if (Entities == null) return;

            lock (Entities)
			{
				EntityManager.AddEntity(entity);

				if (Entities.TryAdd(entity.EntityId, entity))
				{
					if (entity.HealthManager is HealthManager)
					{
						
					}
					LevelEntityAddedEvent addedEvent = new LevelEntityAddedEvent(this, entity);
					EventDispatcher.DispatchEvent(addedEvent);

					entity.SpawnToPlayers(GetAllPlayers());
				}
				else
				{
					throw new Exception("Entity existed in the players list when it should not");
				}
			}
		}

		public override void RemoveEntity(Entity entity)
		{
            if (Entities == null) return;

			lock (Entities)
			{
				if (!Entities.TryRemove(entity.EntityId, out entity)) return; // It's ok. Holograms destroy this play..

				LevelEntityRemovedEvent removedEvent = new LevelEntityRemovedEvent(this, entity);
				EventDispatcher.DispatchEvent(removedEvent);

				entity.DespawnFromPlayers(GetAllPlayers());
			}
		}*/

		public override void AddPlayer(MiNET.Player newPlayer, bool spawn)
		{
			base.AddPlayer(newPlayer, spawn);
			if (Players.TryGetValue(newPlayer.EntityId, out MiNET.Player p))
			{
				LevelEntityAddedEvent addedEvent = new LevelEntityAddedEvent(this, p);
				EventDispatcher.DispatchEvent(addedEvent);
			}
		}

		public override void RemovePlayer(MiNET.Player player, bool despawn = true)
		{
			if (player == null || player.EntityId == null) return;
			if (Players == null)
			{
				Log.Warn("OpenLevel.Players is null!");
				return;
			}

			if (Players.ContainsKey(player.EntityId))
			{
				base.RemovePlayer(player, despawn);
				if (!Players.ContainsKey(player.EntityId))
				{
					LevelEntityRemovedEvent removedEvent = new LevelEntityRemovedEvent(this, player);
					EventDispatcher.DispatchEvent(removedEvent);
				}
			}
		}

		public override void DropItem(Vector3 coordinates, Item drop)
		{
			//if (GameMode == GameMode.Creative) return;

			if (drop == null) return;
			if (drop.Id == 0) return;
			if (drop.Count == 0) return;

			//PlayerItemDropEvent dropEvent = new PlayerItemDropEvent();

			base.DropItem(coordinates, drop);
		}

		public bool BreakBlock(Block block, OpenPlayer player = null, Item tool = null)
		{
			BlockEntity blockEntity = GetBlockEntity(block.Coordinates);

			bool canBreak = player == null || tool == null || tool.BreakBlock(this, player, block, blockEntity);
			if (canBreak)
			{
				block.BreakBlock(this);
				List<Item> drops = new List<Item>();
				drops.AddRange(block.GetDrops(tool ?? new ItemAir()));

				if (blockEntity != null)
				{
					RemoveBlockEntity(block.Coordinates);
					drops.AddRange(blockEntity.GetDrops());
				}

				foreach (Item drop in drops)
				{
					DropItem(block.Coordinates, drop);
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		public override bool OnBlockPlace(BlockPlaceEventArgs e)
		{
			if (e.Player == null) return true;
			BlockPlaceEvent bb = new BlockPlaceEvent((OpenPlayer) e.Player, e.TargetBlock);
			EventDispatcher.DispatchEvent(bb);
			return !bb.IsCancelled;
		}

		public override void BroadcastMessage(string text, MessageType type = MessageType.Chat, MiNET.Player sender = null, MiNET.Player[] sendList = null)
		{
			if (type == MessageType.Chat || type == MessageType.Raw)
			{
				foreach (var line in text.Split(new[] { "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
				{
					string sen = sender == null ? "" : (sender.DisplayName ?? sender.Username) + ": ";
					McpeText message = McpeText.CreateObject();
					message.type = 0;
					message.source = ""; //sender == null ? "" : (sender.DisplayName ?? sender.Username);
					message.message =$"{sen}{line}";
				    //message.parameters = new string[0];
				  //  message.islocalized = false;

                    RelayBroadcast(sendList, message);
				}
				return;
			}
			else
			{
			    McpeText message = McpeText.CreateObject();
			    message.type = (byte)type;
			    message.source = sender == null ? "" : sender.Username;
			    message.message = text;
             //   message.parameters = new string[0];
			 //   message.islocalized = false;

			    RelayBroadcast(sendList, message);
			}
           // base.BroadcastMessage(text, type, sender, sendList);
		}

		public override void Close()
		{
			if (_closed) return;

			_closed = true;
			CancelationToken.Dispose();
			TickScheduler.Close();
			base.Close();

			//OpenAPI.LevelManager.UnloadLevel(this);
		}
	}
}
