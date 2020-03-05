using System.Threading.Tasks;
using log4net;
using MiNET.Items;
using OpenAPI.Events;
using OpenAPI.Events.Block;
using OpenAPI.Events.Player;
using OpenAPI.Plugins;
using OpenAPI.Utils;

namespace OpenAPI.TestPlugin
{
    [OpenPluginInfo(Name = "RandomBreak")]
    public class RandomBreakBlocker : OpenPlugin, IEventHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RandomBreakBlocker));
        
        private FastRandom Random { get; }
        public RandomBreakBlocker()
        {
            Random = new FastRandom();
        }
        
        public override void Enabled(OpenApi api)
        {
            api.EventDispatcher.RegisterEvents(this);
        }

        public override void Disabled(OpenApi api)
        {
            api.EventDispatcher.UnregisterEvents(this);
        }

        [EventHandler]
        public Task OnBlockBreak(BlockBreakEvent e)
        {
            return Task.Run(() =>
            {
                if (Random.NextBool())
                {
                    Log.Info($"Cancelled block breaking.");
                    e.SetCancelled(true);
                }
                else
                {
                    Log.Info("Did not cancel breaking");

                    if (Random.Next(0, 10) == 5)
                    {
                        Log.Info($"Golden apple yay.");
                        e.Drops.Add(new ItemGoldenApple());

                        e.Source.Level.BroadcastMessage($"A goldenapple was found! Lucky!");
                    }
                }
            });
        }

       /* [EventHandler(EventPriority.Highest)]
        public void OnPlayerMove(PlayerMoveEvent e)
        {
            e.SetCancelled(true);
        }*/
    }
}