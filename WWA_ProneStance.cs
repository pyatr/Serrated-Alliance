using System;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;

namespace XRL.World.Effects
{
    [Serializable]
    internal class WWA_ProneStance : Effect
    {
        public int AccuracyBonus;
        public int DVPenalty = 15;
        public int IncomingProjectileToHitPenalty = 10;

        public WWA_ProneStance()
        {
            base.DisplayName = "&cPronce stance";
            this.Duration = 1;
            this.AccuracyBonus = 1;
        }

        public WWA_ProneStance(int accuracyBonus)
        {
            base.DisplayName = "&cPronce stance";
            this.Duration = 1;
            this.AccuracyBonus = accuracyBonus;
        }

        public override bool Apply(GameObject Object)
        {
            Object.Statistics["DV"].Penalty += DVPenalty;
            this.StatShifter.SetStatShift("MoveSpeed", 50);
            return true;
        }

        public override void Remove(GameObject Object)
        {
            Object.Statistics["DV"].Penalty -= DVPenalty;
            this.StatShifter.RemoveStatShifts();
            base.Remove(Object);
        }

        public override string GetDetails()
        {
            return "Rifles and lead-based heavy weapons fire as if your agility was " + (this.AccuracyBonus * 2).ToString() + " points higher.\n"
                + this.DVPenalty.ToString() + " DV penalty in melee combat, " + this.IncomingProjectileToHitPenalty + " DV bonus in ranged combat if distance between you and attacker is larger than 5.\n"
                + "-50 to movement speed.";
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterEffectEvent(this, "BeginMove");
            base.Register(Object);
        }

        public override void Unregister(GameObject Object)
        {
            Object.UnregisterEffectEvent(this, "BeginMove");
            base.Unregister(Object);
        }

        public override bool Render(RenderEvent E)
        {
            int num = XRLCore.CurrentFrame % 120;
            if (num > 35 && num < 45)
            {
                E.Tile = null;
                E.RenderString = "\x001F";
                E.ColorString = "&g";
            }
            return true;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeginMove")
            {
                //return false;
                return true;
            }
            return base.FireEvent(E);
        }
    }
}