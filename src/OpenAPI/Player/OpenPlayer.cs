using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Resources;
using System.Threading;
using fNbt;
using log4net;
using MiNET;
using MiNET.BlockEntities;
using MiNET.Blocks;
using MiNET.Effects;
using MiNET.Entities;
using MiNET.Entities.World;
using MiNET.Items;
using MiNET.Net;
using MiNET.Plugins;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.Entities;
using OpenAPI.Events;
using OpenAPI.Events.Block;
using OpenAPI.Events.Entity;
using OpenAPI.Events.Player;
using OpenAPI.Locale;
using OpenAPI.Permission;
using OpenAPI.Player.Inventory;
using OpenAPI.Utils;
using OpenAPI.World;

namespace OpenAPI.Player
{
	/// <summary>
	/// 	The Player class used for all Players connected to an OpenAPI server.
	/// </summary>
	public class OpenPlayer : MiNET.Player, ILocaleReceiver
    {
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPlayer));

		private readonly ConcurrentDictionary<Type, IOpenPlayerAttribute> _attributes =
			new ConcurrentDictionary<Type, IOpenPlayerAttribute>();

		/// <summary>
		///		The <see cref="EventDispatcher"/> that can be used to listen to any events for this player.
		/// </summary>
		public EventDispatcher EventDispatcher => Level.EventDispatcher ?? _plugin.EventDispatcher;

        private OpenApi _plugin;
        
        /// <summary>
        ///		Returns true if player is authenticated with an online-mode account
        /// </summary>
		public bool IsXbox => !string.IsNullOrWhiteSpace(CertificateData.ExtraData.Xuid);
        
        /// <summary>
        ///		The player's culture info, can be used for localization purposes
        /// </summary>
        public CultureInfo Culture { get; private set; } = CultureInfo.CurrentCulture;
        
        /// <summary>
        ///		The <see cref="PermissionManager"/> for this player
        /// </summary>
        public PermissionManager Permissions { get; }

        internal CommandSet Commands { get; set; } = null;
        
        /// <summary>
        ///		Creates a new <see cref="OpenPlayer"/> instance.
        /// </summary>
        /// <param name="server">The server instance the player connected through</param>
        /// <param name="endPoint">The player's remote endpoint</param>
        /// <param name="api">An instance of the API</param>
        public OpenPlayer(OpenServer server, IPEndPoint endPoint, OpenApi api) : base(server, endPoint)
        {
            EnableCommands = true;
            _plugin = api;
	        IsFlying = false;

            Permissions = new PermissionManager();
	        Inventory = new OpenPlayerInventory(this);

	        _serverHaveResources = api.ResourcePackProvider.HasData;
	        Commands = _plugin.CommandManager.GenerateCommandSet(this);
	        //if (Config.GetProperty("useResourcePack"))
        }

        /// <summary>
        ///		The <see cref="OpenLevel"/> instance the player is currently in.
        /// </summary>
        public new OpenLevel Level => (OpenLevel)base.Level;

        /// <summary>
        ///		Initializes the player
        /// </summary>
        public override void InitializePlayer()
        {
	        PlayerLoginCompleteEvent e = new PlayerLoginCompleteEvent(this, DateTime.UtcNow);
	        EventDispatcher.DispatchEventAsync(e).Then(result =>
	        {
		        if (result.IsCancelled)
		        {
			        Disconnect("Error #357. Please report this error.");
		        }
		        else
		        {
			        base.InitializePlayer();

			        Culture = CultureInfo.CreateSpecificCulture(PlayerInfo.LanguageCode.Replace('_', '-'));

			        HungerManager = new OpenHungerManager(this);
			        HealthManager = new OpenHealthManager(this);
		        }
	        });
	        // HealthManager.PlayerTakeHit += HealthManagerOnPlayerTakeHit;
        }

        /// <inheritdoc />
        public override void HandleMcpeBlockEntityData(McpeBlockEntityData message)
        {
	        var playerPosition = KnownPosition.ToBlockCoordinates();

	        if (playerPosition.DistanceTo(message.coordinates) > 1000)
		        return;

	        var nbt = message.namedtag.NbtFile.RootTag;

	        if (nbt is NbtCompound compound)
	        {
		        var blockEntity = Level.GetBlockEntity(message.coordinates);
		        EventDispatcher.DispatchEventAsync(new PlayerSetBlockEntityDataEvent(this, blockEntity, compound)).Then(
			        (result) =>
			        {
				        if (result.IsCancelled)
					        return;

				        blockEntity.SetCompound(compound);
				        Level.SetBlockEntity(blockEntity);
			        });
	        }

	        //base.HandleMcpeBlockEntityData(message);
        }

        /// <inheritdoc />
        public override void HandleMcpeSetPlayerGameType(McpeSetPlayerGameType message)
        {
	        // Fallback is the "inherit the level's mode" sentinel that StartGame sends as the
	        // player's mode, and the client acknowledges it verbatim, so it arrives here on every
	        // join. It is not a mode: storing it leaves GameMode matching nothing at all, which
	        // silently disables every creative-gated path, block breaking included.
	        var requested = (GameMode) message.gamemode;
	        GameMode gameMode = requested == GameMode.Fallback ? Level.GameMode : requested;

	        if (!Enum.IsDefined(gameMode))
	        {
		        Log.Warn($"Ignoring SetPlayerGameType with unknown game mode {message.gamemode}");
		        return;
	        }

	        EventDispatcher.DispatchEventAsync(
		        new PlayerGamemodeChangeEvent(
			        this, GameMode, gameMode,
			        PlayerGamemodeChangeEvent.PlayerGamemodeChangeTrigger.Self)).Then(
		        response =>
		        {
			        if (response.IsCancelled)
			        {
				        SetGamemode(response.OldGameMode);
				        return;
			        }

			        SetGamemode(response.NewGameMode);
		        });
        }

        // HandleMcpeAdventureSettings used to guard against a client sending another player's
        // entity id to change their adventure settings. The packet split into
        // UpdateAdventureSettings and UpdateAbilities back in 1.19.30 and its id is retired, so
        // there is nothing left for a client to spoof. Abilities are granted by the server and a
        // client that wants one asks with McpeRequestAbility, which MiNET ignores.

        /// <summary>
        ///		Handles any incoming commands.
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMcpeCommandRequest(McpeCommandRequest message)
        {
	        var result = _plugin.CommandManager.HandleCommand(this, message.command);
	        if (result is string)
	        {
		        string sRes = result as string;
		        SendMessage(sRes);
	        }
        }

        /// <summary>
        ///		Sends the players available command set
        /// </summary>
        protected override void SendAvailableCommands()
        {
	        McpeAvailableCommands commands = McpeAvailableCommands.CreateObject();
	        commands.CommandSet = Commands;
	        
	        SendPacket(commands);
        }

        /// <summary>
        ///		Reloads & sends the players available commands.
        /// </summary>
        public void RefreshCommands()
        {
            Commands = _plugin.CommandManager.GenerateCommandSet(this);
            SendAvailableCommands();
        }

        /* private void HealthManagerOnPlayerTakeHit(object sender, HealthEventArgs e)
	    {
	        if (!FormsOpened.IsEmpty)
	        {
                CloseAllForms();
	        }
	    }*/

	    private bool _previousIsSpawned = false;
        protected override void OnTicking(PlayerEventArgs e)
        {

        }

        protected override async void OnTicked(PlayerEventArgs e)
        {
	        if (Monitor.TryEnter(_breakSync))
	        {
		        try
		        {
			        if (IsBreakingBlock)
			        {
				        var elapsedTicks = BlockBreakTimer.Elapsed.TotalMilliseconds / 50;
						if (elapsedTicks - BlockBreakTime >= 3) //3 ticks late?
				        {
							StopBreak(BreakingBlockCoordinates);
						}
			        }
		        }
		        finally
		        {
			        Monitor.Exit(_breakSync);
		        }
	        }

            var isSpawned = IsSpawned;

            if (isSpawned && !_previousIsSpawned)
            {
                PlayerSpawnedEvent ev = new PlayerSpawnedEvent(this);
                await EventDispatcher.DispatchEventAsync(ev);
            }
            else if (!isSpawned && _previousIsSpawned)
            {
                PlayerDespawnedEvent ev = new PlayerDespawnedEvent(this);
                await EventDispatcher.DispatchEventAsync(ev);
            }

            _previousIsSpawned = isSpawned;

            _disguise?.Tick();
        }

		private bool _hasJoinedServer = false;
		private bool _isFirstJoining = true;
		protected override void OnPlayerJoining(PlayerEventArgs e)
		{
			if (!_isFirstJoining)
				return;

			_isFirstJoining = false;
			
			if (_plugin.LevelManager.HasDefaultLevel)
			{
				base.Level = _plugin.LevelManager.GetDefaultLevel();
			}
		}

		protected override async void OnPlayerJoin(PlayerEventArgs e)
        {
	        if (_hasJoinedServer) return; //Make sure this is only called once when we join the server for the first time.
	        _hasJoinedServer = true;

			await EventDispatcher.DispatchEventAsync(new PlayerJoinEvent(this));
        }

        protected override async void OnPlayerLeave(PlayerEventArgs e)
        {
	        await EventDispatcher.DispatchEventAsync(new PlayerQuitEvent(this));
        }

        private bool PlayerMoveEvent(PlayerLocation from, PlayerLocation to, bool teleport = false)
        {
            PlayerMoveEvent playerMoveEvent = new PlayerMoveEvent(this, from, to, teleport);
            EventDispatcher.DispatchEvent(playerMoveEvent);
            return !playerMoveEvent.IsCancelled;
        }

      /*  private int _lastPlayerMoveSequenceNUmber;
        private int _lastOrderingIndex;
        private object _moveSyncLock = new object();
        public override void HandleMcpeMovePlayer(McpeMovePlayer message)
        {
	        if (!IsSpawned || HealthManager.IsDead) return;

	        if (_plugin.OpenServer.ServerRole != ServerRole.Node)
	        {
		        lock (_moveSyncLock)
		        {
			        if (_lastPlayerMoveSequenceNUmber > message.DatagramSequenceNumber)
			        {
				        return;
			        }

			        _lastPlayerMoveSequenceNUmber = message.DatagramSequenceNumber;

			        if (_lastOrderingIndex > message.OrderingIndex)
			        {
				        return;
			        }

			        _lastOrderingIndex = message.OrderingIndex;
		        }
	        }

	        var newPosition = new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw,
		        message.pitch);
	        
	        EventDispatcher.DispatchEventAsync(new PlayerMoveEvent(this, KnownPosition, newPosition, false))
		        .Then(
			        result =>
			        {
				        if (result.IsCancelled)
					        return;
				        
				        base.HandleMcpeMovePlayer(message);
				        //base.Teleport(result.To);
			        });
        }*/

        protected override bool AcceptPlayerMove(McpeMovePlayer message, bool isOnGround, bool isFlyingHorizontally)
        {
	      //  return true;
	        
            if (!PlayerMoveEvent(KnownPosition, new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch)))
            {
                return false;
            }

            return base.AcceptPlayerMove(message, isOnGround, isFlyingHorizontally);
        }

        /// <summary>
        ///		Teleports the player to specified position
        /// </summary>
        /// <param name="newPosition">The position to teleport the player to</param>
        public override void Teleport(PlayerLocation newPosition)
        {
	        EventDispatcher.DispatchEventAsync(new PlayerMoveEvent(this, KnownPosition, newPosition, true))
		        .Then(
			        result =>
			        {
				        if (result.IsCancelled)
					        return;
				        
				        base.Teleport(result.To);
			        });
        }

        /// <summary>
        ///		Transfers the player to a different server
        ///		Note, only supports IPv4 addresses
        /// </summary>
        /// <param name="endpoint">The endpoint to transfer to</param>
        /// <exception cref="NotSupportedException">Thrown when the endpoint isn't an IPv4 Address</exception>
		public void TransferToServer(IPEndPoint endpoint)
		{
			if (endpoint.AddressFamily == AddressFamily.InterNetworkV6)
				throw new NotSupportedException("IPV6 is currently not supported!");

			McpeTransfer transfer = McpeTransfer.CreateObject();
			transfer.port = (ushort) endpoint.Port;
			transfer.serverAddress = endpoint.Address.ToString();
			SendPacket(transfer);
		}

        /// <summary>
        ///		Handles incoming chat messages
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMcpeText(McpeText message)
        {
            string text = message.message;

            if (string.IsNullOrEmpty(text)) return;
            PlayerChatEvent chatEvent = new PlayerChatEvent(this, text);
	        EventDispatcher.DispatchEventAsync(chatEvent).Then(result =>
	        {
		        if (result.IsCancelled)
			        return;
		        
		        Level.BroadcastMessage(chatEvent.Message, sender: this);
	        });
        }

        /// <inheritdoc />
        public override void HandleMcpeInventoryTransaction(McpeInventoryTransaction message)
        {
	        base.HandleMcpeInventoryTransaction(message);
        }
        
      

        public Item GetInvItem(int inventoryId, int slot)
        {
	        if (inventoryId == 0)
		        return Inventory.Slots[slot];
	        
	        return GetContainerItem(inventoryId, slot);
        }

        public void SetInvItem(int inventoryId, int slot, Item item)
        {
	        // Rebuilt through the factory rather than stored as given, so a custom item registered
	        // with OpenItemFactory comes back as its own type. Keyed by registry name since 1.26.
	        var newItem = ItemFactory.GetItemByName(item.Name, item.Metadata, item.Count);
	        newItem.ExtraData = item.ExtraData;

	        if (inventoryId == 0) //Player Inventory
	        {
		        Inventory.Slots[slot] = newItem;
		        return;
	        }
	        SetContainerItem(inventoryId, slot, newItem);
        }

        private void InventoryMisMatch()
        {
	        SendPlayerInventory();
        }
        
        /// <inheritdoc />
        protected override void HandleNormalTransaction(NormalTransaction normal)
        {
	        ObservableCollection<InventoryAction> actions = new ObservableCollection<InventoryAction>();
	        actions.CollectionChanged += (sender, args) =>
	        {
		        if (args.Action == NotifyCollectionChangedAction.Add)
		        {
			        foreach (InventoryAction add in args.NewItems)
			        {
				        add.OnAddToTransaction();
			        }
		        }
	        };
	        
	        foreach (var transaction in normal.TransactionRecords)
	        {
		        var newItem = transaction.NewItem;
		        var oldItem = transaction.OldItem;

		        if (SlotChangeAction.EqualsExactly(newItem, oldItem))
		        {
			        continue;
		        }

		        switch (transaction)
		        {
			        case WorldInteractionTransactionRecord wit:
			        {
				        if (wit.Slot != 0)
				        {
					        Log.Warn($"Got non item-drop in WorldInteractionTransactionRecord!");
					        InventoryMisMatch();

					        break;
				        }

				        actions.Add(new DropItemAction(newItem));

				        bool didMatch = false;

				        foreach (var record in normal.RequestRecords)
				        {
					        foreach (var slot in record.Slots)
					        {
						        var item = GetContainerItem(record.ContainerId, slot);

						        // Name, not RuntimeId: the container item is server built and only
						        // block items carry a runtime id, so comparing those would match on 0.
						        if (string.Equals(item.Name, wit.NewItem.Name, StringComparison.OrdinalIgnoreCase)
						                                      && item.Metadata == wit.NewItem.Metadata
						                                      && item.Count >= wit.NewItem.Count)
						        {
							        item.Count -= newItem.Count;

							        if (DropItem(newItem, item))
							        {
								        // base: the event was already raised just above, and the
								        // override would raise it a second time.
								        base.DropItem(wit.NewItem);
								        didMatch = true;

								        break;
							        }
							        else
							        {
								        item.Count += wit.NewItem.Count;
							        }
						        }
					        }

					        if (didMatch)
						        break;
				        }

				        if (normal.RequestRecords.Count > 0 && !didMatch)
				        {
					        Log.Warn($"WorldInteractionTransactionRecord: No matching item found.");
					        InventoryMisMatch();

					        return;
				        }

				        // Log.Info($"WorldInteractionTransactionRecord: (Flags={wit.Flags} Slot={wit.Slot} NewItem={wit.NewItem} OldItem={wit.OldItem} StackId={wit.StackNetworkId})");
			        } break;
			        
			        case ContainerTransactionRecord ctr:
			        {
				        //  var item = GetInvItem(ctr.InventoryId, ctr.Slot);
				        /*
				        if (item.Count != newItem.Count)
				        {
					        Log.Warn($"ContainerTransactionRecord invalid! Expected: {item.Count} Got: {newItem.Count} (OldItem={oldItem})");
					        InventoryMisMatch();

					        return;
				        }
				        */

				        actions.Add(new SlotChangeAction(this, ctr.InventoryId, ctr.Slot, oldItem, newItem));
				        //Log.Info($"ContainerTransactionRecord (InventoryId={ctr.InventoryId} Slot={ctr.Slot} StackId={ctr.StackNetworkId}) (NewItem={ctr.NewItem}) (OldItem={ctr.OldItem})");
			        } break;
		        }
	        }

	        foreach (var action in actions)
	        {
		        if (!action.IsValid(this))
		        {
			        Log.Info($"Invalid action: {action}");
			        break;
		        }
	        }

	        foreach (var action in actions)
	        {
		        if (action.PreExecute(this))
		        {
			        if (action.Execute(this))
			        {
				        action.ExecutionSucceeded(this);
			        }
			        else
			        {
				        action.ExecutionFailed(this);
			        }
		        }
	        }
        }


        protected override void HandleItemUseOnEntityTransaction(ItemUseOnEntityTransaction transaction)
        {
	        if (!Level.TryGetEntity<Entity>(transaction.EntityId, out var entity) || !entity.IsSpawned || entity.HealthManager.IsDead || entity.HealthManager.IsInvulnerable)
	        {
		        return;
	        }
	        //     var entity = Level.GetEntity(transaction.EntityId);
	        //  if (entity == null || !entity.IsSpawned || entity.HealthManager.IsDead || entity.HealthManager.IsInvulnerable)
	        //      return;

	        var actionType = (McpeInventoryTransaction.ItemUseOnEntityAction) transaction.ActionType;
			
	        EntityInteractEvent interactEvent = new EntityInteractEvent(entity, this, actionType);
	        EventDispatcher.DispatchEventAsync(interactEvent).Then(result =>
	        {
		        if (result.IsCancelled)
			        return;
		        
		        base.HandleItemUseOnEntityTransaction(transaction);
	        });
        }
        
        protected override void HandleItemReleaseTransaction(ItemReleaseTransaction transaction)
        {
	        Log.Warn($"Got old ItemReleaseTransaction...");
	        return;
		    Item itemInHand = Inventory.GetItemInHand();
			switch ((McpeInventoryTransaction.ItemReleaseAction) transaction.ActionType)
		    {
			    case McpeInventoryTransaction.ItemReleaseAction.Release:
			    {
				    if (!DropItem(itemInHand, new ItemAir()))
				    {
					    //HandleNormalTransaction(transaction);
					    HandleTransactionRecords(transaction.TransactionRecords);
					    return;
				    }

				    break;
			    }
			    case McpeInventoryTransaction.ItemReleaseAction.Use:
			    {
				    if (!UseItem(itemInHand))
				    {
					    HandleTransactionRecords(transaction.TransactionRecords);
					    //HandleNormalTransaction(transaction);
					    return;
				    }

				    break;
			    }
		    }

		    base.HandleItemReleaseTransaction(transaction);
	    }
	    
	    protected override void HandleItemUseTransaction(ItemUseTransaction transaction)
	    {
		    var itemInHand = Inventory.GetItemInHand();

		    switch ((McpeInventoryTransaction.ItemUseAction) transaction.ActionType)
		    {
			    case McpeInventoryTransaction.ItemUseAction.Destroy:
			    {
				    var target = Level.GetBlock(transaction.Position);

				    PlayerInteractEvent interactEvent = new PlayerInteractEvent(this, itemInHand, transaction.Position,
					    (BlockFace) transaction.Face,
					    (target is Air)
						    ? PlayerInteractEvent.PlayerInteractType.LeftClickAir
						    : PlayerInteractEvent.PlayerInteractType.LeftClickBlock);
				    
				    EventDispatcher.DispatchEventAsync(interactEvent).Then(result =>
				    {
					    if (result.IsCancelled)
						    return;
					    
					    base.HandleItemUseTransaction(transaction);
				    });
				    
				    return;
			    }
			    case McpeInventoryTransaction.ItemUseAction.Use:
			    {
				    if (!UseItem(itemInHand))
				    {
					    //HandleNormalTransaction(transaction);
					    HandleTransactionRecords(transaction.TransactionRecords);
					    return;
				    }

				    break;
			    }
			    case McpeInventoryTransaction.ItemUseAction.Place:
			    {
				    var target = Level.GetBlock(transaction.Position);
				    
				    PlayerInteractEvent interactEvent = new PlayerInteractEvent(this, itemInHand, transaction.Position,
					    (BlockFace) transaction.Face, (target is Air)
						    ? PlayerInteractEvent.PlayerInteractType.RightClickAir
						    : PlayerInteractEvent.PlayerInteractType.RightClickBlock);
				    
				    EventDispatcher.DispatchEventAsync(interactEvent).Then(result =>
				    {
					    if (result.IsCancelled)
						    return;
					    
					    base.HandleItemUseTransaction(transaction);
				    });

				    return;
			    }
		    }
	    }

	    /// <inheritdoc />
	    protected override void HandleTransactionRecords(List<TransactionRecord> records)
	    {
		
	    }

	    private bool UseItem(Item usedItem)
	    {
		    PlayerItemUseEvent useEvent = new PlayerItemUseEvent(this, usedItem);
		    EventDispatcher.DispatchEvent(useEvent);
		    if (useEvent.IsCancelled)
		    {
			    return false;
		    }

		    return true;
	    }

	    internal bool DropItem(Item droppedItem, Item newInventoryItem)
		{
			PlayerItemDropEvent dropEvent = new PlayerItemDropEvent(this, this.KnownPosition, droppedItem, newInventoryItem);
			EventDispatcher.DispatchEvent(dropEvent);
			if (dropEvent.IsCancelled)
			{
				SendPlayerInventory();
				return false;
			}

			return true;
			//base.DropItem(droppedItem, newInventoryItem);
		}

		/// <summary>
		///		Drops an item into the world, raising <see cref="PlayerItemDropEvent"/> first.
		/// </summary>
		/// <remarks>
		///		Inventories are server authoritative since 1.26, so a drop reaches the server as an
		///		ItemStackRequest DropAction handled by MiNET's ItemStackInventoryManager rather than
		///		as the world-interaction record HandleNormalTransaction used to see. That manager
		///		calls this method, which makes it the one place every drop passes through.
		/// </remarks>
		/// <returns>The spawned item entity, or null when a handler cancelled the drop.</returns>
		public override ItemEntity DropItem(Item item)
		{
			if (!DropItem(item, new ItemAir()))
				return null;

			return base.DropItem(item);
		}

	    /*public override void HandleMcpeServerSettingsRequest(McpeServerSettingsRequest message)
	    {
		    PlayerSettingsRequestEvent e = new PlayerSettingsRequestEvent(this, message);
		    EventDispatcher.DispatchEvent(e);

		    if (!e.IsCancelled)
			    base.HandleMcpeServerSettingsRequest(message);
	    }*/

		private object _breakSync = new object();
		private bool IsBreakingBlock { get; set; } = false;
		private double BlockBreakTime { get; set; } = -1;
		private Stopwatch BlockBreakTimer = new Stopwatch();
		private BlockCoordinates BreakingBlockCoordinates { get; set; }
		private BlockFace BreakingFace { get; set; } = BlockFace.None;

		/// <summary>
		///		Handles player actions like Start & Stop break
		/// </summary>
		/// <param name="message"></param>
	    public override void HandleMcpePlayerAction(McpePlayerAction message)
	    {
		    if (HandleBlockAction((PlayerAction) message.actionId, message.coordinates, (BlockFace) message.face))
			    return;

		    base.HandleMcpePlayerAction(message);
	    }

		/// <summary>
		///		Whether OpenAPI takes this action over from MiNET so block breaking runs through
		///		the event pipeline. Everything else stays with the base implementation.
		/// </summary>
		private static bool IsBreakAction(PlayerAction action)
		{
			switch (action)
			{
				case PlayerAction.StartBreak:
				case PlayerAction.AbortBreak:
				case PlayerAction.StopBreak:
				case PlayerAction.CreativeDestroy:
				case PlayerAction.PredictDestroyBlock:
				case PlayerAction.ContinueDestroyBlock:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		///		Runs a block break action through OpenAPI's events. Shared by the
		///		<see cref="McpePlayerAction"/> packet and the block actions carried on
		///		<see cref="McpePlayerAuthInput"/>, which is where a 1.26 client sends them now
		///		that block breaking is server authoritative.
		/// </summary>
		/// <returns>True when OpenAPI handled it and the base implementation must not run.</returns>
		private bool HandleBlockAction(PlayerAction action, BlockCoordinates coordinates, BlockFace face)
	    {
		    if (!IsBreakAction(action))
			    return false;

			lock (_breakSync)
			{
				if (GameMode == GameMode.Creative)
				{
					// Creative destroys in one hit and the client predicts it, so the destroy
					// arrives as PredictDestroyBlock. CreativeDestroy is sent for the same swing
					// and acting on both would break the block twice.
					if (action == PlayerAction.PredictDestroyBlock)
						BreakBlock(coordinates, face);

					return true;
				}

				Block block;
				if (action == PlayerAction.StartBreak || action == PlayerAction.ContinueDestroyBlock)
				{
					// ContinueDestroyBlock repeats while the client stays on the same block.
					// Restarting the timer for those would stop it ever reaching BlockBreakTime.
					if (IsBreakingBlock && BreakingBlockCoordinates == coordinates)
					{
						SendBlockCracking(coordinates, face);
						return true;
					}

					block = Level.GetBlock(coordinates);
					var inHand = Inventory.GetItemInHand();
					var drops = block.GetDrops(inHand);
					
					float tooltypeFactor = drops == null || drops.Length == 0 ? 5f : 1.5f; // 1.5 if proper tool
					
					var multiplier = 1f;
					switch (inHand.ItemMaterial)
					{
						case ItemMaterial.None:
							break;
						case ItemMaterial.Wood:
							multiplier = 2f;
							break;
						case ItemMaterial.Stone:
							multiplier = 4f;
							break;
						case ItemMaterial.Gold:
							multiplier = 12f;
							break;
						case ItemMaterial.Iron:
							multiplier = 6f;
							break;
						case ItemMaterial.Diamond:
							multiplier = 8f;
							break;
					}

					foreach (var enchantment in inHand.GetEnchantings())
					{
						if (enchantment.Id == EnchantingType.Efficiency && enchantment.Level > 0)
						{
							multiplier += MathF.Sqrt(enchantment.Level) + 1;
						}
					}

					if (Effects.TryGetValue(EffectType.Haste, out var effect))
					{
						if (effect is Haste haste && haste.Level > 0f)
						{
							multiplier *= 1f + (haste.Level * 0.2f);
						}
					}

					var hardness = block.Hardness;
					
					double breakTime = MathF.Ceiling((hardness * tooltypeFactor * 20f));

					McpeLevelEvent message1 = McpeLevelEvent.CreateObject();
					message1.eventId = 3600;
					message1.position = coordinates;
					message1.data = (int) ((double) ushort.MaxValue / (breakTime / multiplier));

					Level.RelayBroadcast(message1);

					IsBreakingBlock = true;
					BlockBreakTimer.Restart();
					BreakingBlockCoordinates = block.Coordinates;
					BlockBreakTime = breakTime / multiplier;
					BreakingFace = face;

					var blockStartBreak = new BlockStartBreakEvent(this, block);
					EventDispatcher.DispatchEventAsync(blockStartBreak).Then(result =>
					{
						if (result.IsCancelled)
						{
							SendBlockBreakEnd(block.Coordinates);
							return;
						}
					});

					return true;
				}
				else if (action == PlayerAction.AbortBreak)
				{
					block = Level.GetBlock(coordinates);
					if (IsBreakingBlock && BreakingBlockCoordinates == block.Coordinates)
					{
						IsBreakingBlock = false;
						BlockBreakTimer.Reset();

						EventDispatcher.DispatchEventAsync(new BlockAbortBreakEvent(this, block)).Then(result =>
						{
							if (!result.IsCancelled)
							{
								SendBlockBreakEnd(block.Coordinates);
								return;
							}
						});
					}

					return true;
				}
				else if (action == PlayerAction.StopBreak || action == PlayerAction.PredictDestroyBlock)
				{
					var elapsed = BlockBreakTimer.ElapsedMilliseconds;
					var elapsedTicks = elapsed / 50d;

					if (IsBreakingBlock)
					{
						if (elapsedTicks >= BlockBreakTime || Math.Abs(elapsedTicks - BlockBreakTime) < 2.5
						) //Give a max time difference of 2.5 ticks.
						{
							StopBreak(BreakingBlockCoordinates);
						}
						else
						{
							// Broke faster than the tool allows. Put the block back so the client's
							// prediction does not stick.
							RevertBlockBreak(BreakingBlockCoordinates);
						}
					}
					else
					{
						IsBreakingBlock = false;
						BlockBreakTimer.Reset();
					}

					return true;
				}
			}

			return false;
	    }

	    private void SendBlockBreakEnd(BlockCoordinates coordinates)
	    {
	        McpeLevelEvent levelEvent = McpeLevelEvent.CreateObject();
	        levelEvent.position = coordinates;
	        levelEvent.eventId = 3601; //Block stop cracking
	        levelEvent.data = 0;
	        Level.RelayBroadcast(levelEvent);
        }

	    /// <summary>
	    ///		Shows the crack overlay progressing on a block that is already being broken.
	    /// </summary>
	    private void SendBlockCracking(BlockCoordinates coordinates, BlockFace face)
	    {
		    McpeLevelEvent levelEvent = McpeLevelEvent.CreateObject();
		    levelEvent.position = coordinates;
		    levelEvent.eventId = 2014; //Block cracking
		    // The face belongs in the high byte of data, but MiNET writes it as
		    // (byte) (face << 24), which is always 0. Kept off here rather than diverging from the
		    // bytes MiNET sends for the same event.
		    levelEvent.data = (int) Level.GetBlock(coordinates).GetRuntimeId();
		    Level.RelayBroadcast(levelEvent);
	    }

	    /// <summary>
	    ///		Re-sends a block the client predicted away. Needed because block breaking is server
	    ///		authoritative: the client removes the block locally before we agree to it, so a
	    ///		break we reject has to be undone client side or the two views drift apart.
	    /// </summary>
	    private void RevertBlockBreak(BlockCoordinates coordinates)
	    {
		    IsBreakingBlock = false;
		    BlockBreakTimer.Reset();

		    var block = Level.GetBlock(coordinates);

		    McpeUpdateBlock update = McpeUpdateBlock.CreateObject();
		    // Through BlockFactory, never a raw runtime id: see RevertBlockAction in OpenLevel.
		    update.blockRuntimeId = BlockFactory.GetNetworkId(block);
		    update.coordinates = coordinates;
		    update.blockPriority = 0xb;
		    SendPacket(update);

		    SendBlockBreakEnd(coordinates);
	    }

	    /// <summary>
	    ///		Breaks a block through OpenAPI's <see cref="OpenLevel.BreakBlock"/>, which is what
	    ///		raises <see cref="BlockBreakEvent"/>. MiNET's own Level.BreakBlock is not virtual
	    ///		and would skip the event pipeline entirely.
	    /// </summary>
	    private void BreakBlock(BlockCoordinates coordinates, BlockFace face)
	    {
		    var block = Level.GetBlock(coordinates);

		    if (!Level.BreakBlock(block, face, this, Inventory.GetItemInHand()))
			    SendBlockBreakEnd(coordinates);
	    }

	    private void StopBreak(BlockCoordinates coords, bool reset = true)
		{
			if (reset)
			{
				IsBreakingBlock = false;
				BlockBreakTimer.Reset();
			}

			BreakBlock(coords, BreakingFace);
		}

	    /*protected override bool CanBreakBlock(Block block, Item itemInHand)
	    {
	        if (GameMode == GameMode.Creative)
	        {
	            BlockBreakEvent e = new BlockBreakEvent(this, block);
	            EventDispatcher.DispatchEvent(e);
	            if (e.IsCancelled) return false;
                
	            block.BreakBlock(Level);

                e.OnComplete();
	            return true;
	        }

		    return false;
	    }*/

	    private readonly Dictionary<PlayerInput, PlayerInputState> _inputStates = new Dictionary<PlayerInput, PlayerInputState>()
	    {
	        {PlayerInput.W, PlayerInputState.Up},
	        {PlayerInput.A, PlayerInputState.Up},
	        {PlayerInput.S, PlayerInputState.Up},
	        {PlayerInput.D, PlayerInputState.Up},
	        {PlayerInput.Space, PlayerInputState.Up},
	    };

	    /// <summary>
	    ///		Wheter to capture player keyboard input, if true <see cref="HandleMcpePlayerAuthInput"/> will try to capture every keystroke.
	    /// </summary>
	    public bool CapturePlayerInputMode = false;

	    /// <summary>
	    ///		The last known state of one of the keys tracked while
	    ///		<see cref="CapturePlayerInputMode"/> is enabled.
	    /// </summary>
	    public PlayerInputState GetInputState(PlayerInput input)
	    {
		    lock (_inputStates)
		    {
			    return _inputStates.TryGetValue(input, out var state) ? state : PlayerInputState.Up;
		    }
	    }

	    /// <summary>
	    ///		The 1.26 client sends <see cref="McpePlayerAuthInput"/> every tick and it is the only
	    ///		movement packet it sends, so this is where movement, keyboard input and block
	    ///		breaking all arrive.
	    /// </summary>
	    /// <param name="message"></param>
	    public override void HandleMcpePlayerAuthInput(McpePlayerAuthInput message)
	    {
		    if (CapturePlayerInputMode)
			    CapturePlayerInput(message.InputFlags);

		    // Block actions OpenAPI owns are pulled out before the base call: MiNET would break the
		    // block through its own non-virtual Level.BreakBlock, which never raises BlockBreakEvent.
		    // The rest (the crack overlay, for one) is left for the base implementation.
		    List<McpePlayerAuthInput.PlayerBlockAction> blockActions = message.BlockActions;
		    List<McpePlayerAuthInput.PlayerBlockAction> ownedActions = null;

		    if (blockActions != null)
		    {
			    List<McpePlayerAuthInput.PlayerBlockAction> passthrough = null;

			    foreach (var blockAction in blockActions)
			    {
				    if (IsBreakAction((PlayerAction) blockAction.ActionType))
					    (ownedActions ??= new List<McpePlayerAuthInput.PlayerBlockAction>()).Add(blockAction);
				    else
					    (passthrough ??= new List<McpePlayerAuthInput.PlayerBlockAction>()).Add(blockAction);
			    }

			    message.BlockActions = passthrough;
		    }

		    bool moveRejected = false;

		    try
		    {
			    if (IsSpawned && !HealthManager.IsDead)
			    {
				    // Y arrives at eye height, the same as MovePlayer's did.
				    var to = new PlayerLocation(
					    message.Position.X, message.Position.Y - 1.62f, message.Position.Z,
					    message.HeadYaw, message.Yaw, message.Pitch);

				    if (HasMoved(KnownPosition, to) && !PlayerMoveEvent(KnownPosition, to))
				    {
					    // Rewritten to where the server already has the player so the base call
					    // applies the move as a no-op. Everything else on this packet (input flags,
					    // inventory actions) is unrelated to the move and still gets processed.
					    message.Position = new Vector3(KnownPosition.X, KnownPosition.Y + 1.62f, KnownPosition.Z);
					    message.Pitch = KnownPosition.Pitch;
					    message.Yaw = KnownPosition.Yaw;
					    message.HeadYaw = KnownPosition.HeadYaw;

					    moveRejected = true;
				    }
			    }

			    base.HandleMcpePlayerAuthInput(message);
		    }
		    finally
		    {
			    message.BlockActions = blockActions;
		    }

		    // Movement is client authoritative, so the client has already moved locally. Without
		    // correcting it, a rejected move leaves it walking away from where the server has it.
		    if (moveRejected)
			    base.Teleport(KnownPosition);

		    if (ownedActions == null)
			    return;

		    foreach (var blockAction in ownedActions)
		    {
			    HandleBlockAction(
				    (PlayerAction) blockAction.ActionType,
				    new BlockCoordinates(blockAction.X, blockAction.Y, blockAction.Z),
				    (BlockFace) blockAction.Face);
		    }
	    }

	    /// <summary>
	    ///		Whether the client reported a position or rotation that differs from what the server
	    ///		has. PlayerAuthInput arrives every tick whether or not the player moved, and
	    ///		<see cref="PlayerMoveEvent"/> is only meant to fire when they did.
	    /// </summary>
	    private static bool HasMoved(PlayerLocation from, PlayerLocation to)
	    {
		    const float threshold = 0.0001f;

		    return Math.Abs(from.X - to.X) > threshold
		           || Math.Abs(from.Y - to.Y) > threshold
		           || Math.Abs(from.Z - to.Z) > threshold
		           || Math.Abs(from.Yaw - to.Yaw) > threshold
		           || Math.Abs(from.HeadYaw - to.HeadYaw) > threshold
		           || Math.Abs(from.Pitch - to.Pitch) > threshold;
	    }

        /// <summary>
        ///		Translates the auth input flags into key states, so we can determine what buttons are
        ///		pressed by the client. The flags name the keys directly; before 1.26 this had to be
        ///		inferred from the motion vector on the retired PlayerInput packet.
        /// </summary>
        /// <param name="flags"></param>
        private void CapturePlayerInput(AuthInputFlags flags)
	    {
		    UpdateInputState(PlayerInput.W, (flags & AuthInputFlags.WalkForwards) != 0);
		    UpdateInputState(PlayerInput.S, (flags & AuthInputFlags.WalkBackwards) != 0);
		    UpdateInputState(PlayerInput.A, (flags & AuthInputFlags.StrafeLeft) != 0);
		    UpdateInputState(PlayerInput.D, (flags & AuthInputFlags.StrafeRight) != 0);
		    UpdateInputState(PlayerInput.Space, (flags & AuthInputFlags.JumpDown) != 0);
	    }

	    private void UpdateInputState(PlayerInput input, bool isDown)
	    {
		    var state = isDown ? PlayerInputState.Down : PlayerInputState.Up;

		    lock (_inputStates)
		    {
			    if (_inputStates.TryGetValue(input, out var previous) && previous == state)
				    return;

			    _inputStates[input] = state;
		    }

		    EventDispatcher.DispatchEvent(new PlayerInputEvent(this, input, state));
	    }

      /*  public override void HandleMcpeRiderJump(McpeRiderJump message)
        {
            if (CapturePlayerInputMode)
            {
                SendMessage("Jump input detected!");
                return;
            }
            base.HandleMcpeRiderJump(message);
        }*/
		
		/// <summary>
		///		Handles entity & world interactions.
		/// </summary>
		/// <param name="message"></param>
        public override void HandleMcpeInteract(McpeInteract message)
        {
            if (CapturePlayerInputMode && message.actionId == 3)
            {
                SendMessage("Leave vehicle detected!");
                return;
            }
            base.HandleMcpeInteract(message);
        }

        #region Resource Packs

        private bool _serverHaveResources = false;

        private uint _maxChunkSize = 1048576; //1MB
        public override void HandleMcpeResourcePackChunkRequest(McpeResourcePackChunkRequest message)
        {
	        var chunk = _plugin.ResourcePackProvider.GetResourcePackChunk(message.packageId, message.chunkIndex, _maxChunkSize);
	        
	        McpeResourcePackChunkData chunkData = McpeResourcePackChunkData.CreateObject();
	        chunkData.packageId = message.packageId;
	        chunkData.chunkIndex = message.chunkIndex;
	        chunkData.progress = (_maxChunkSize * message.chunkIndex);
	        //chunkData.length = (uint) chunk.Length;
	        chunkData.payload = chunk;
	        SendPacket(chunkData);
        }

        public override void HandleMcpeResourcePackClientResponse(McpeResourcePackClientResponse message)
        {
	        if (message.responseStatus == 2)
	        {
		        foreach (var a in message.resourcepackids)
		        {
			        string uuid = a.Split('_')[0];

			        var chunkCount = _plugin.ResourcePackProvider.GetChunkCount(uuid, _maxChunkSize, out var manifest,
				        out var size, out var hash);

			        McpeResourcePackDataInfo dataInfo = McpeResourcePackDataInfo.CreateObject();
			        dataInfo.maxChunkSize = _maxChunkSize;
			        dataInfo.chunkCount = chunkCount;
			        dataInfo.compressedPackageSize = size;
			        dataInfo.hash = hash;
			        dataInfo.packageId = manifest.Header.Uuid;

			        SendPacket(dataInfo);
		        }

		        return;
	        }
	        else if (message.responseStatus == 3)
	        {
		        SendResourcePackStack();
		        return;
	        }
	        else if (message.responseStatus == 4)
	        {
		        OpenServer.FastThreadPool.QueueUserWorkItem(() => { Start(null); });
		        return;
	        }
        }

        public override void SendResourcePacksInfo()
        {
	        McpeResourcePacksInfo info = McpeResourcePacksInfo.CreateObject();
	        info.worldTemplateId = (UUID) Guid.Empty;
	        info.worldTemplateVersion = "0.0.0"; // vanilla sends this, not an empty string
	        if (_serverHaveResources)
	        {
		        info.mustAccept = _plugin.ResourcePackProvider.MustAccept;
		        // 1.26 dropped the separate behaviour pack list from this packet; texturepacks is
		        // the only one left, which is where the resource packs belonged anyway.
		        info.texturepacks = new TexturePackInfos();
		        info.texturepacks.AddRange(_plugin.ResourcePackProvider.GetResourcePackInfos());
	        }
	        
	        SendPacket(info);
        }

        public override void SendResourcePackStack()
        {
	        var info = McpeResourcePackStack.CreateObject();
	        info.gameVersion = "*"; // vanilla sends this, not the concrete game version
	        if (_serverHaveResources)
	        {
		        info.mustAccept = _plugin.ResourcePackProvider.MustAccept;
		        info.resourcepackidversions = new ResourcePackIdVersions();
		        info.resourcepackidversions.AddRange(_plugin.ResourcePackProvider.GetResourcePackInfos().Select(x => new PackIdVersion()
		        {
			        Id = x.UUID,
			        Version = x.Version,
			        SubPackName = x.SubPackName
		        }));
	        }

	        SendPacket(info);
        }

        #endregion

        /// <summary>
        /// 	Set's the players gamemode to the specified gamemode
        /// </summary>
        /// <param name="gameMode">The gamemode to set for the player</param>
        public void SetGamemode(GameMode gameMode)
        {
	        GameMode = gameMode;
	        SendSetPlayerGameType();
        }

        private EntityDisguise _disguise = null;

        /// <summary>
        /// 	Can be used to Disguise a player into any Entity. See <see cref="EntityDisguise"/> and <seealso cref="Entity"/>
        /// 	Can be undone by setting the value to null.
        /// </summary>
        public EntityDisguise Disguise
        {
            get { return _disguise; }
            set
            {
                EntityDisguise newValue = value;
                if (newValue == _disguise) return;

                if (_disguise != null)
                {
                    _disguise.DespawnDisguise();
                }

                _disguise = newValue;

                if (newValue != null)
                {
                    this.IsInvisible = true;
                    newValue.SpawnDisguise();
                }
                else
                {
                    this.IsInvisible = false;
                }

                BroadcastSetEntityData();
            }
        }

        /// <summary>
        /// 	Whether the player is currently Disguised using the <see cref="Disguise"/> property
        /// </summary>
        public bool IsDisguised => Disguise != null;

        public override void SpawnToPlayers(MiNET.Player[] players)
        {
	        SpawnToPlayers(false, players.Cast<OpenPlayer>().ToArray());
        }

        public override void DespawnFromPlayers(MiNET.Player[] players)
        {
			DespawnFromPlayers(false, players.Cast<OpenPlayer>().ToArray());
        }

        /// <summary>
        ///		Despawns the player from other players
        /// </summary>
        /// <param name="forced">If true, force despawns even if disguised</param>
        /// <param name="players">The players to despawn from</param>
        public void DespawnFromPlayers(bool forced = true, params OpenPlayer[] players)
        {
	        if (!IsDisguised && !forced)
	        {
		        base.DespawnFromPlayers(players);
		        return;
	        }

	        Disguise.DespawnFromPlayers(players);
        }
        
        /// <summary>
        ///		Spawns the player from other players
        /// </summary>
        /// <param name="forced">If true, force spawns even if disguised</param>
        /// <param name="players">The players to spawn for</param>
        public void SpawnToPlayers(bool forced = true, params OpenPlayer[] players)
        {
	        if (!IsDisguised && !forced)
	        {
		        base.SpawnToPlayers(players);
		        return;
	        }

	        Disguise.SpawnToPlayers(players);
        }

        public void SendLocalizedTitle(string text, TitleType type = TitleType.Title, int fadeIn = 6, int fadeOut = 6,
            int stayTime = 20, MiNET.Player sender = null)
        {
            SendTitle(this.GetLocalizedMessage(LocaleManager.GetLocaleProvider(Assembly.GetCallingAssembly()), text), type, fadeIn, fadeOut, stayTime, sender);
        }

        public void SendLocalizedTitle(string text, object[] parameters = null, TitleType type = TitleType.Title, int fadeIn = 6, int fadeOut = 6,
            int stayTime = 20, MiNET.Player sender = null)
        {
            SendTitle(this.GetLocalizedMessage(LocaleManager.GetLocaleProvider(Assembly.GetCallingAssembly()), text, parameters), type, fadeIn, fadeOut, stayTime, sender);
        }

        public void SendLocalizedMessage(string text, MessageType type = MessageType.Chat, MiNET.Player sender = null)
        {
            base.SendMessage(this.GetLocalizedMessage(LocaleManager.GetLocaleProvider(Assembly.GetCallingAssembly()), text), type, sender);
        }

        public void SendLocalizedMessage(string text, object[] parameters = null, MessageType type = MessageType.Chat, MiNET.Player sender = null)
        {
            base.SendMessage(this.GetLocalizedMessage(LocaleManager.GetLocaleProvider(Assembly.GetCallingAssembly()), text, parameters), type, sender);
        }

        #region Player Attributes

        /// <summary>
        /// 	Allows you to retrieve the value for any PlayerAttributes set on a player <see cref="IOpenPlayerAttribute"/>
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type to retrieve its value for</typeparam>
        /// <returns>The value for the requested attribute, or null if no value was found.</returns>
        public TAttribute GetAttribute<TAttribute>() where TAttribute : class, IOpenPlayerAttribute
        {
	        if (_attributes.TryGetValue(typeof(TAttribute), out IOpenPlayerAttribute attribute))
            {
                return attribute as TAttribute;
            }

            return null;
        }

        
        /// <summary>
        /// 	Allows you to store extra data on a player using PlayerAttributes <see cref="IOpenPlayerAttribute"/>
        /// </summary>
        /// <param name="attribute">The value to set the attribute to</param>
        /// <typeparam name="TAttribute">The type of the attribute you wish to set.</typeparam>
        public void SetAttribute<TAttribute>(TAttribute attribute) where TAttribute : class, IOpenPlayerAttribute
        {
            _attributes.AddOrUpdate(typeof(TAttribute), attribute, (type, playerAttribute) => attribute);
        }

        /// <summary>
        /// 	Removes a PlayerAttribute from this player entirely.
        /// </summary>
        /// <remarks>
        /// 	Prefer this over <c>SetAttribute&lt;TAttribute&gt;(null)</c>. Setting null clears the
        /// 	value but keeps <c>typeof(TAttribute)</c> as a dictionary key, and the key alone is
        /// 	enough to keep the declaring assembly loaded — so a plugin that "cleaned up" that
        /// 	way could still never be unloaded.
        /// </remarks>
        /// <typeparam name="TAttribute">The type of the attribute to remove.</typeparam>
        /// <returns>Whether the attribute was set on this player.</returns>
        public bool RemoveAttribute<TAttribute>() where TAttribute : class, IOpenPlayerAttribute
        {
	        return _attributes.TryRemove(typeof(TAttribute), out _);
        }

        /// <summary>
        /// 	Removes every attribute belonging to <paramref name="assembly"/> from this player.
        /// </summary>
        /// <remarks>
        /// 	Players outlive plugin reloads, so an attribute left behind here pins the plugin
        /// 	assembly for as long as the player object survives. Both the key type and the
        /// 	stored value are checked.
        /// </remarks>
        internal int PurgeAssembly(Assembly assembly)
        {
	        int removed = 0;

	        foreach (var attribute in _attributes.ToArray())
	        {
		        bool belongsToAssembly =
			        attribute.Key.Assembly == assembly
			        || attribute.Value?.GetType().Assembly == assembly;

		        if (belongsToAssembly && _attributes.TryRemove(attribute.Key, out _))
			        removed++;
	        }

	        return removed;
        }

		#endregion
    }
}
