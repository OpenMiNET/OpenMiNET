using OpenAPI.Player;

namespace OpenAPI.Events.Player
{
    public class FoodLevelChangeEvent : PlayerEvent
    {
        public int OldLevel { get; set; }
        public int NewLevel { get; set; }
        public double OldExhaustion { get; set; }
        public double NewExhaustion { get; set; }
        public double OldSaturation { get; set; }
        public double NewSaturation { get; set; }
        public FoodLevelChangeEvent(OpenPlayer player, int oldLevel, int newLevel, double oldExhaustion, double newExhaustion, double oldSaturation, double newSaturation) : base(player)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
            OldSaturation = oldSaturation;
            NewSaturation = newSaturation;
            OldExhaustion = oldExhaustion;
            NewExhaustion = newExhaustion;
        }
    }
}
