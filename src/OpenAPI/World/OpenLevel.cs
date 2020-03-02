using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading;
using fNbt;
using log4net;
using MiNET;
using MiNET.BlockEntities;
using MiNET.Blocks;
using MiNET.Entities;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Utils.Skins;
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
		public OpenApi OpenAPI { get; }
		private CancellationTokenSource CancelationToken { get; }

		public OpenLevel(OpenApi openApi,
			OpenLevelManager levelManager,
			string levelId,
			IWorldProvider worldProvider,
			EntityManager entityManager,
			GameMode gameMode = GameMode.Survival,
			Difficulty difficulty = Difficulty.Normal,
			int viewDistance = 11)
			: base(levelManager, levelId,
				(worldProvider is ICachingWorldProvider)
					? new WrappedCachedWorldProvider(openApi, worldProvider)
					: new WrappedWorldProvider(openApi, worldProvider), entityManager, gameMode, difficulty,
				viewDistance)
		{
			OpenAPI = openApi;
			CancelationToken = new CancellationTokenSource();
			TickScheduler = new TickScheduler();

			EventDispatcher = new EventDispatcher(openApi, OpenAPI.EventDispatcher);

			if (WorldProvider is WrappedWorldProvider wrapped)
			{
				wrapped.Level = this;
			}
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
			if (newPlayer.Skin == null)
			{
				newPlayer.Skin = new Skin()
				{
					Animations = new List<Animation>(),
					Cape = new Cape()
					{
						Data = new byte[0]
					},
					Data = ArrayOf<byte>.Create(8192, 0xFF),
					Height = 64,
					Width = 32,
					IsPremiumSkin = false,
					IsPersonaSkin = false,
					Slim = false
				};
			}
			
			base.AddPlayer(newPlayer, spawn);
			if (Players.TryGetValue(newPlayer.EntityId, out MiNET.Player p))
			{
				LevelEntityAddedEvent addedEvent = new LevelEntityAddedEvent(this, p);
				EventDispatcher.DispatchEventAsync(addedEvent);
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
					EventDispatcher.DispatchEventAsync(removedEvent);
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

		protected override bool OnBlockBreak(BlockBreakEventArgs e)
		{
			return false;
		}

		public bool BreakBlock(Block block, BlockFace face, OpenPlayer player = null, Item tool = null)
		{
			BlockEntity blockEntity = GetBlockEntity(block.Coordinates);

			bool canBreak = player == null || tool == null || tool.BreakBlock(this, player, block, blockEntity);
			if (!canBreak || !AllowBreak || player?.GameMode == GameMode.Spectator)
			{
				if (player != null)
					RevertBlockAction(player, block, blockEntity);
				
				return false;
			}
			
			//	block.BreakBlock(this, face);
			List<Item> drops = new List<Item>();
			drops.AddRange(block.GetDrops(tool ?? new ItemAir()));

			if (blockEntity != null)
			{
				//	RemoveBlockEntity(block.Coordinates);
				drops.AddRange(blockEntity.GetDrops());
			}
			
			BlockBreakEvent e = new BlockBreakEvent(player, block, drops);
			EventDispatcher.DispatchEvent(e);
			if (e.IsCancelled)
			{
				if (player != null)
					RevertBlockAction(player, block, blockEntity);
						
				return false;
			}

			block.BreakBlock(this, face);
				
			if (blockEntity != null)
				RemoveBlockEntity(block.Coordinates);

			foreach (Item drop in e.Drops)
			{
				DropItem(block.Coordinates, drop);
			}

			e.OnComplete();
				
			return true;
		}
		
		private static void RevertBlockAction(OpenPlayer player, Block block, BlockEntity blockEntity)
		{
			var message = McpeUpdateBlock.CreateObject();
			message.blockRuntimeId = (uint) block.GetRuntimeId();
			message.coordinates = block.Coordinates;
			message.blockPriority = 0xb;
			player.SendPacket(message);

			// Revert block entity if exists
			if (blockEntity != null)
			{
				Nbt nbt = new Nbt
				{
					NbtFile = new NbtFile
					{
						BigEndian = false,
						RootTag = blockEntity.GetCompound()
					}
				};

				var entityData = McpeBlockEntityData.CreateObject();
				entityData.namedtag = nbt;
				entityData.coordinates = blockEntity.Coordinates;

				player.SendPacket(entityData);
			}
		}

		public override bool OnBlockPlace(BlockPlaceEventArgs e)
		{
			if (e.Player == null) return true;
			BlockPlaceEvent bb = new BlockPlaceEvent((OpenPlayer) e.Player, e.TargetBlock);
			EventDispatcher.DispatchEvent(bb);
			return !bb.IsCancelled;
		}

		public override void BroadcastMessage(string text, MessageType type = MessageType.Chat, MiNET.Player sender = null, MiNET.Player[] sendList = null,
			bool needsTranslation = false, string[] parameters = null)
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
