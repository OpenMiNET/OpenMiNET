using MiNET;
using MiNET.Items;
using MiNET.Worlds;
using OpenAPI.Events.Player;
using OpenAPI.Player;

namespace OpenAPI.Entities
{
    public class OpenHungerManager : HungerManager
    {
        public OpenHungerManager(OpenPlayer player) : base(player)
        {

        }

        public override void IncreaseExhaustion(float amount)
        {
            if (Player is OpenPlayer open)
            {
                var e = new FoodLevelChangeEvent(open, Hunger, Hunger, Exhaustion, Exhaustion + amount, Saturation,
                    Saturation);
                open.EventDispatcher.DispatchEvent(e);

                if (e.IsCancelled) return;
            }

            Exhaustion += amount;
            ProcessHunger();
        }

        public override void IncreaseFoodAndSaturation(Item item, int foodPoints, double saturationRestore)
        {
            if (Player is OpenPlayer open){
                var e = new FoodLevelChangeEvent(open, Hunger, Hunger + foodPoints, Exhaustion, Exhaustion, Saturation,
                    Saturation + saturationRestore);
                open.EventDispatcher.DispatchEvent(e);

                if (e.IsCancelled) return;
            }

            Hunger += foodPoints;
            Saturation += saturationRestore;

            ProcessHunger(true);
        }

        public override void ProcessHunger(bool forceSend = false)
        {
            int oldHunger = Hunger;
            double oldExhaustion = Exhaustion;
            double oldSaturation = Saturation;

            bool send = forceSend;

            if (Hunger > MaxHunger)
            {
                Hunger = MaxHunger;
                send = true;
            }

            if (Saturation > Hunger)
            {
                Saturation = Hunger;
                send = true;
            }

            while (Exhaustion >= 4)
            {
                Exhaustion -= 4;

                if (Saturation > 0)
                {
                    Saturation -= 1;
                    if (Saturation < 0) send = true;
                }
                else
                {
                    Hunger -= 1;
                    Saturation = 0;

                    if (Hunger < 0) Hunger = 0; // Damage!
                    send = true;
                }
            }

            if (Player is OpenPlayer open)
            {
                var e = new FoodLevelChangeEvent(open, oldHunger, Hunger, oldExhaustion, Exhaustion, oldSaturation, Saturation);
                open.EventDispatcher.DispatchEvent(e);

                if (e.IsCancelled)
                {
                    Hunger = oldHunger;
                    Saturation = oldSaturation;
                    Exhaustion = oldExhaustion;
                    return;
                }
                else
                {
                    Hunger = e.NewLevel;
                    Saturation = e.NewSaturation;
                    Exhaustion = e.NewExhaustion;
                }
            }

            if (send) SendHungerAttributes();
        }

        private long _ticker;
        public override void OnTick()
        {
            if (Hunger <= 0)
            {
                _ticker++;

                if (_ticker % 80 == 0)
                {
                    Player.HealthManager.TakeHit(null, 1, DamageCause.Starving);
                }
            }
            else if (Hunger > 18 && Player.HealthManager.Hearts < 20)
            {
                _ticker++;

                if (Hunger >= 20 && Saturation > 0)
                {
                    if (_ticker % 10 == 0)
                    {
                        if (Player.Level.Difficulty != Difficulty.Hardcore)
                        {
                            IncreaseExhaustion(4);
                            Player.HealthManager.Regen(1);
                        }
                    }
                }
                else
                {
                    if (_ticker % 80 == 0)
                    {
                        if (Player.Level.Difficulty != Difficulty.Hardcore)
                        {
                            IncreaseExhaustion(4);
                            Player.HealthManager.Regen(1);
                        }
                    }
                }
            }
            else
            {
                _ticker = 0;
            }

        }
    }
}
