using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Resources;
using System.Threading;
using log4net;
using MiNET;
using MiNET.BlockEntities;
using MiNET.Blocks;
using MiNET.Entities;
using MiNET.Items;
using MiNET.Net;
using MiNET.Plugins;
using MiNET.Utils;
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
	public class OpenPlayer : MiNET.Player, ILocaleReceiver
    {
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenPlayer));

		private readonly ConcurrentDictionary<Type, IOpenPlayerAttribute> _attributes =
			new ConcurrentDictionary<Type, IOpenPlayerAttribute>();

		public EventDispatcher EventDispatcher => Level.EventDispatcher ?? _plugin.EventDispatcher;

        private OpenAPI _plugin;
		public bool IsXbox => !string.IsNullOrWhiteSpace(CertificateData.ExtraData.Xuid);
        public CultureInfo Culture { get; private set; } = CultureInfo.CurrentCulture;
        public PermissionManager Permissions { get; }

        internal CommandSet Commands { get; set; } = null;
        public OpenPlayer(MiNetServer server, IPEndPoint endPoint, OpenAPI api) : base(server, endPoint)
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

        public new OpenLevel Level => (OpenLevel)base.Level;

        public override void InitializePlayer()
        {
	        PlayerLoginCompleteEvent e = new PlayerLoginCompleteEvent(this, DateTime.UtcNow);
	        EventDispatcher.DispatchEvent(e);
	        if (e.IsCancelled)
	        {
		        Disconnect("Error #357. Please report this error.");
	        }

			base.InitializePlayer();

            Culture = CultureInfo.CreateSpecificCulture(PlayerInfo.LanguageCode.Replace('_', '-'));

            HungerManager = new OpenHungerManager(this);
	        HealthManager = new OpenHealthManager(this);
		   // HealthManager.PlayerTakeHit += HealthManagerOnPlayerTakeHit;
        }

        public override void HandleMcpeCommandRequest(McpeCommandRequest message)
        {
	        var result = _plugin.CommandManager.HandleCommand(this, message.command);
	        if (result is string)
	        {
		        string sRes = result as string;
		        SendMessage(sRes);
	        }
        }

        protected override void SendAvailableCommands()
        {
	        McpeAvailableCommands commands = McpeAvailableCommands.CreateObject();
	        commands.CommandSet = Commands;
	        
	        SendPacket(commands);
        }

        protected override void SendSetCommandsEnabled()
        {
	        base.SendSetCommandsEnabled();
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

        protected override void OnTicked(PlayerEventArgs e)
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
                EventDispatcher.DispatchEvent(ev);
            }
            else if (!isSpawned && _previousIsSpawned)
            {
                PlayerDespawnedEvent ev = new PlayerDespawnedEvent(this);
                EventDispatcher.DispatchEvent(ev);
            }

            _previousIsSpawned = isSpawned;

            _disguise?.Tick();
        }

		private bool _hasJoinedServer = false;

        protected override void OnPlayerJoin(PlayerEventArgs e)
        {
	        if (_hasJoinedServer) return; //Make sure this is only called once when we join the server for the first time.
	        _hasJoinedServer = true;

			EventDispatcher.DispatchEvent(new PlayerJoinEvent(this));
        }

        protected override void OnPlayerLeave(PlayerEventArgs e)
        {
            EventDispatcher.DispatchEvent(new PlayerQuitEvent(this));
        }

        private bool PlayerMoveEvent(PlayerLocation from, PlayerLocation to, bool teleport = false)
        {
            PlayerMoveEvent playerMoveEvent = new PlayerMoveEvent(this, from, to, teleport);
            EventDispatcher.DispatchEvent(playerMoveEvent);
            return !playerMoveEvent.IsCancelled;
        }

        protected override bool AcceptPlayerMove(McpeMovePlayer message, bool isOnGround, bool isFlyingHorizontally)
        {
            if (!PlayerMoveEvent(KnownPosition, new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch)))
            {
                return false;
            }

            return base.AcceptPlayerMove(message, isOnGround, isFlyingHorizontally);
        }

        public override void Teleport(PlayerLocation newPosition)
        {
            if (!PlayerMoveEvent(KnownPosition, newPosition, true))
            {
                return;
            }
            base.Teleport(newPosition);
        }

		public void TransferToServer(IPEndPoint endpoint)
		{
			if (endpoint.AddressFamily == AddressFamily.InterNetworkV6)
				throw new NotSupportedException("IPV6 is currently not supported!");

			McpeTransfer transfer = McpeTransfer.CreateObject();
			transfer.port = (ushort) endpoint.Port;
			transfer.serverAddress = endpoint.Address.ToString();
			SendPacket(transfer);
		}

        public override void HandleMcpeText(McpeText message)
        {
            string text = message.message;

            if (string.IsNullOrEmpty(text)) return;
            PlayerChatEvent chatEvent = new PlayerChatEvent(this, text);
	        EventDispatcher.DispatchEvent(chatEvent);

	        if (chatEvent.IsCancelled) return;

	        Level.BroadcastMessage(chatEvent.Message, sender: this);
		}

	    protected override void HandleItemUseOnEntityTransactions(Transaction transaction)
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
			EventDispatcher.DispatchEvent(interactEvent);
			if (interactEvent.IsCancelled) return;

            base.HandleItemUseOnEntityTransactions(transaction);
        }

	    protected override void HandleItemReleaseTransactions(Transaction transaction)
	    {
		    Item itemInHand = Inventory.GetItemInHand();
			switch ((McpeInventoryTransaction.ItemReleaseAction) transaction.ActionType)
		    {
			    case McpeInventoryTransaction.ItemReleaseAction.Release:
			    {
				    if (!DropItem(itemInHand, new ItemAir()))
				    {
					    HandleNormalTransactions(transaction);
					    return;
				    }

				    break;
			    }
			    case McpeInventoryTransaction.ItemReleaseAction.Use:
			    {
				    if (!UseItem(itemInHand))
				    {
					    HandleNormalTransactions(transaction);
					    return;
				    }

				    break;
			    }
		    }

		    base.HandleItemReleaseTransactions(transaction);
	    }

	    public override void DropItem(Item item)
	    {
		    base.DropItem(item);
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
					var drops = block.GetDrops(Inventory.GetItemInHand());
					float tooltypeFactor = drops == null || drops.Length == 0 ? 5f : 1.5f; // 1.5 if proper tool
					double breakTime = Math.Ceiling(block.Hardness * tooltypeFactor * 20);

					var blockStartBreak = new BlockStartBreakEvent(this, block);
					EventDispatcher.DispatchEvent(blockStartBreak);

					if (blockStartBreak.IsCancelled)
					{
						SendBlockBreakEnd(block.Coordinates);
						return;
					}

					IsBreakingBlock = true;
					BlockBreakTimer.Restart();
					BreakingBlockCoordinates = block.Coordinates;
					BlockBreakTime = breakTime;
				}
				else if (action == PlayerAction.AbortBreak)
				{
					block = Level.GetBlock(message.coordinates);
					if (IsBreakingBlock && BreakingBlockCoordinates == block.Coordinates)
					{
						IsBreakingBlock = false;
						BlockBreakTimer.Reset();

						EventDispatcher.DispatchEvent(new BlockAbortBreakEvent(this, block));
					}
				}
				else if (action == PlayerAction.StopBreak)
				{
					if (IsBreakingBlock)
					{
						var elapsed = BlockBreakTimer.Elapsed.TotalMilliseconds;
						var elapsedTicks = elapsed / 50;
						if (elapsedTicks > BlockBreakTime || Math.Abs(elapsedTicks - BlockBreakTime) < 2.5
						) //Give a max time difference of 2.5 ticks.
						{
							StopBreak(BreakingBlockCoordinates);
						}
					}
					else
					{
						IsBreakingBlock = false;
						BlockBreakTimer.Reset();
					}
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

	    private void StopBreak(BlockCoordinates coords)
		{
			IsBreakingBlock = false;
			BlockBreakTimer.Reset();

			var b = Level.GetBlock(coords);
			BlockBreakEvent e = new BlockBreakEvent(this, b);
			EventDispatcher.DispatchEvent(e);
			if (e.IsCancelled)
			{
                return;
			}

			Item inHand = Inventory.GetItemInHand();
			Level.BreakBlock(b, this, inHand);

            e.OnComplete();
		}

	    protected override void HandleItemUseTransactions(Transaction transaction)
	    {
		    var itemInHand = Inventory.GetItemInHand();

		    switch ((McpeInventoryTransaction.ItemUseAction) transaction.ActionType)
		    {
			    case McpeInventoryTransaction.ItemUseAction.Destroy:
			    {
				    break;
			    }
			    case McpeInventoryTransaction.ItemUseAction.Use:
			    {
				    if (!UseItem(itemInHand))
				    {
					    HandleNormalTransactions(transaction);
					    return;
				    }

				    break;
				}
				case McpeInventoryTransaction.ItemUseAction.Place:
				{
					break;
				}
		    }

		    base.HandleItemUseTransactions(transaction);
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

        public bool CapturePlayerInputMode = false;
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
	        chunkData.length = (uint) chunk.Length;
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
		        info.resourcepackinfos = new ResourcePackInfos();
		        info.resourcepackinfos.AddRange(_plugin.ResourcePackProvider.GetResourcePackInfos());
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
		        info.resourcepackidversions.AddRange(_plugin.ResourcePackProvider.GetResourcePackInfos().Select(x => x.PackIdVersion));
	        }

	        SendPacket(info);
        }

        #endregion

        private EntityDisguise _disguise = null;

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

        public bool IsDisguised => Disguise != null;

        public override void SpawnToPlayers(MiNET.Player[] players)
        {
            if (!IsDisguised)
            {
                base.SpawnToPlayers(players);
                return;
            }

            Disguise.SpawnToPlayers(players);
        }

        public override void DespawnFromPlayers(MiNET.Player[] players)
        {
            if (!IsDisguised)
            {
                base.DespawnFromPlayers(players);
                return;
            }

            Disguise.DespawnFromPlayers(players);
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

        public TAttribute GetAttribute<TAttribute>() where TAttribute : class, IOpenPlayerAttribute
        {
	        if (_attributes.TryGetValue(typeof(TAttribute), out IOpenPlayerAttribute attribute))
            {
                return attribute as TAttribute;
            }

            return null;
        }

        public void SetAttribute<TAttribute>(TAttribute attribute) where TAttribute : class, IOpenPlayerAttribute
        {
            _attributes.AddOrUpdate(typeof(TAttribute), attribute, (type, playerAttribute) => attribute);
        }

		#endregion
    }
}
