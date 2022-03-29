using log4net;
using MiNET.Blocks;
using MiNET.Items;
using OpenAPI.Events;
using OpenAPI.Events.Entity;
using OpenAPI.Events.Player;
using OpenAPI.Plugins;

namespace OpenAPI.TestPlugin
{
	public class TransactionTest : OpenPlugin, IEventHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(TransactionTest));
		
		/// <inheritdoc />
		public override void Enabled(OpenApi api)
		{
			Log.Info("Transaction test!");
			api.EventDispatcher.RegisterEvents(this);
		}

		/// <inheritdoc />
		public override void Disabled(OpenApi api)
		{
			
		}

		[EventHandler]
		public void OnPlayerSpawn(PlayerSpawnedEvent e)
		{
			e.Player.Inventory.SetInventorySlot(0, new ItemDiamond()
			{
				Count = 64
			}, true);
			
			e.Player.Inventory.SetInventorySlot(3, new ItemBlock(new GoldBlock())
			{
				Count = 48
			}, true);
		}

		[EventHandler]
		public void OnEntityInteract(EntityInteractEvent e)
		{
			Log.Info(
				$"Got entity interact event (Source={e.SourcePlayer.ToString()} Target={e.Entity.EntityId}) Action={e.Action.ToString()}");
		}

		[EventHandler]
		public void OnitemDrop(PlayerItemDropEvent e)
		{
			Log.Info($"Detected Item Drop.");
		}

		[EventHandler]
		public void OnItemUse(PlayerItemUseEvent e)
		{
			Log.Info($"Detected item use: {e.ItemUsed}");
		}

		[EventHandler]
		public void OnPlayerInteract(PlayerInteractEvent e)
		{
			Log.Info($"Player Interact triggered. (Player={e.Player} Action={e.InteractType} Coordinates={e.Coordinates})");
		}
	}
}