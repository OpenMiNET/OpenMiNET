using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
	        EventDispatcher.DispatchEventAsync(
		        new PlayerGamemodeChangeEvent(
			        this, GameMode, (GameMode) message.gamemode,
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

        /// <inheritdoc />
        public override void HandleMcpeAdventureSettings(McpeAdventureSettings message)
        {
	        if (message.entityUniqueId != EntityId)
	        {
		        //We are trying to change another players adventuresettings.
		        return;
	        }
	        
	        base.HandleMcpeAdventureSettings(message);
        }

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

		    base.HandleItemUseTransaction(transaction);
	    }

	    /// <inheritdoc />
	    protected override void HandleTransactionRecords(List<TransactionRecord> records)
	    {
		    if (records.Count == 0)
			    return;

		    foreach (TransactionRecord record in records.ToArray())
		    {
			    switch (record)
			    {
				    case WorldInteractionTransactionRecord _:
				    {
					    records.Remove(record);
					    
					    var sourceItem = Inventory.GetItemInHand();
					    byte count = record.NewItem.Count;
					    
					    Item dropItem;
					    bool clearSlot = false;
					    if (sourceItem.Count == count)
					    {
						    dropItem = sourceItem;
						    clearSlot = true;
					    }
					    else
					    {
						    dropItem = (Item) sourceItem.Clone();
						    dropItem.Count = count;
						    dropItem.UniqueId = Environment.TickCount;
					    }
					    
					    if (DropItem(dropItem, clearSlot ? new ItemAir() : sourceItem))
					    {
						    DropItem(dropItem);
						    
						    if (clearSlot)
						    {
							    Inventory.ClearInventorySlot((byte) Inventory.InHandSlot);
						    }
						    else
						    {
							    sourceItem.Count -= count;
						    }
					    }
					    break;
				    }
			    }
		    }
		    
		    base.HandleTransactionRecords(records);
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

	    private bool DropItem(Item droppedItem, Item newInventoryItem)
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
		    var action = (PlayerAction)message.actionId;

			lock (_breakSync)
			{
				if (GameMode == GameMode.Creative)
				{
					return;
				}

				Block block;
				if (action == PlayerAction.StartBreak)
				{
					block = Level.GetBlock(message.coordinates);
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
					message1.position = message.coordinates;
					message1.data = (int) ((double) ushort.MaxValue / (breakTime / multiplier));
					
					Level.RelayBroadcast(message1);
					
					BlockFace face = (BlockFace) message.face;

					IsBreakingBlock = true;
					BlockBreakTimer.Restart();
					BreakingBlockCoordinates = block.Coordinates;
					BlockBreakTime = breakTime / multiplier;
					BreakingFace = face;

			//		Log.Info(
			//			$"Start Breaking block. Hardness: {hardness} | ToolTypeFactor; {tooltypeFactor} | BreakTime: {breakTime} | Multiplier: {multiplier} | BLockBreakTime: {breakTime / multiplier} | IsBreaking: {IsBreakingBlock}");
					
					var blockStartBreak = new BlockStartBreakEvent(this, block);
					EventDispatcher.DispatchEventAsync(blockStartBreak).Then(result =>
					{
						if (result.IsCancelled)
						{
							SendBlockBreakEnd(block.Coordinates);
							return;
						}
					});
					
					return;
				}
				else if (action == PlayerAction.AbortBreak)
				{
					var elapsed = BlockBreakTimer.ElapsedMilliseconds;
					var elapsedTicks = elapsed / 50d;
					
				//	Log.Info($"!! Abort Break !!! Ticks elapsed: {elapsedTicks} | Required: {BlockBreakTime} | IsBreaking: {IsBreakingBlock}");
					
					block = Level.GetBlock(message.coordinates);
					if (IsBreakingBlock && BreakingBlockCoordinates == block.Coordinates)
					{
						IsBreakingBlock = false;
						BlockBreakTimer.Reset();

						EventDispatcher.DispatchEventAsync(new BlockAbortBreakEvent(this, block));
					}
					
					return;
				}
				else if (action == PlayerAction.StopBreak)
				{
					var elapsed = BlockBreakTimer.ElapsedMilliseconds;
					var elapsedTicks = elapsed / 50d;
					
					//Log.Info($"## !! Stop Break !!! Ticks elapsed: {elapsedTicks} | Required: {BlockBreakTime} | IsBreaking: {IsBreakingBlock}");
					
					if (IsBreakingBlock)
					{
						//BlockFace face = (BlockFace) message.face;
						if (elapsedTicks >= BlockBreakTime || Math.Abs(elapsedTicks - BlockBreakTime) < 2.5
						) //Give a max time difference of 2.5 ticks.
						{
							StopBreak(BreakingBlockCoordinates);
						}
						else
						{
							
						}
					}
					else
					{
						IsBreakingBlock = false;
						BlockBreakTimer.Reset();
					}

					return;
				}
			}

			base.HandleMcpePlayerAction(message);
	    }

	    private void SendBlockBreakEnd(BlockCoordinates coordinates)
	    {
	        McpeLevelEvent levelEvent = McpeLevelEvent.CreateObject();
	        levelEvent.position = coordinates;
	        levelEvent.eventId = 3601; //Block stop cracking
	        levelEvent.data = 0;
	        Level.RelayBroadcast(levelEvent);
        }

	    private void StopBreak(BlockCoordinates coords, bool reset = true)
		{
			if (reset)
			{
				IsBreakingBlock = false;
				BlockBreakTimer.Reset();
			}

			var b = Level.GetBlock(coords);
			
			Item inHand = Inventory.GetItemInHand();
			Level.BreakBlock(b, BreakingFace, this, inHand);

           // e.OnComplete();
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

	    private Dictionary<PlayerInput, PlayerInputState> _inputStates = new Dictionary<PlayerInput, PlayerInputState>()
	    {
	        {PlayerInput.W, PlayerInputState.Up},
	        {PlayerInput.A, PlayerInputState.Up},
	        {PlayerInput.S, PlayerInputState.Up},
	        {PlayerInput.D, PlayerInputState.Up},
	        {PlayerInput.Space, PlayerInputState.Up},
	    };

	    /// <summary>
	    ///		Wheter to capture player keyboard input, if true <see cref="HandleMcpePlayerAction"/> will try to capture every keystroke.
	    /// </summary>
        public bool CapturePlayerInputMode = false;
        /// <summary>
        ///		Handles player input, so we can determine what buttons are pressed by the client.
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMcpePlayerInput(McpePlayerInput message)
	    {
	       // SendMessage($"MX: {message.motionX} MZ: {message.motionZ} Flag1: {message.flag1} Flag2: {message.flag2}");
	        if (CapturePlayerInputMode)
	        {
	            if (message.motionX > 0)
	            {
	                _inputStates[PlayerInput.A] = PlayerInputState.Down;
	                _inputStates[PlayerInput.D] = PlayerInputState.Up;
                }
                else if (message.motionX < 0)
	            {
	                _inputStates[PlayerInput.A] = PlayerInputState.Up;
                    _inputStates[PlayerInput.D] = PlayerInputState.Down;
                }
	            else
	            {
	                _inputStates[PlayerInput.A] = PlayerInputState.Up;
	                _inputStates[PlayerInput.D] = PlayerInputState.Up;
                }

	           if(message.motionZ > 0)
	            {
	                _inputStates[PlayerInput.W] = PlayerInputState.Down;
	                _inputStates[PlayerInput.S] = PlayerInputState.Up;
                }
	            else if (message.motionZ < 0)
	            {
	                _inputStates[PlayerInput.W] = PlayerInputState.Up;
	                _inputStates[PlayerInput.S] = PlayerInputState.Down;
                }
	           else
	           {
	               _inputStates[PlayerInput.W] = PlayerInputState.Up;
	               _inputStates[PlayerInput.S] = PlayerInputState.Up;
                }
            }
            base.HandleMcpePlayerInput(message);
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
	        if (_serverHaveResources)
	        {
		        info.mustAccept = _plugin.ResourcePackProvider.MustAccept;
		        info.behahaviorpackinfos = new ResourcePackInfos();
		        info.behahaviorpackinfos.AddRange(_plugin.ResourcePackProvider.GetResourcePackInfos());
	        }
	        
	        SendPacket(info);
        }

        public override void SendResourcePackStack()
        {
	        var info = McpeResourcePackStack.CreateObject();
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

		#endregion
    }
}
