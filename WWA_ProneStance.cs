using System;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Effects
{
    [Serializable]
    internal class WWA_ProneStance : Effect
    {
        public int AccuracyBonus;
        public int DVPenalty = 15;
        public int IncomingProjectileToHitPenalty = 10;
        public int MinProneDVDistance = 5;
        public int MovementSpeedPenalty = 50;

        public bool ProneBonusApplied = false;

        public WWA_ProneStance()
        {
            base.DisplayName = "&cProne stance";
            Duration = 1;
            AccuracyBonus = 1;
        }

        public WWA_ProneStance(int accuracyBonus)
        {
            base.DisplayName = "&cProne stance";
            Duration = 1;
            AccuracyBonus = accuracyBonus;
        }

        public override bool Apply(GameObject Object)
        {
            Object.Statistics["DV"].Penalty += DVPenalty;
            StatShifter.SetStatShift("MoveSpeed", MovementSpeedPenalty);
            return true;
        }

        public override void Remove(GameObject Object)
        {
            Object.Statistics["DV"].Penalty -= DVPenalty;
            StatShifter.RemoveStatShifts();
            base.Remove(Object);
        }

        public override string GetDetails()
        {
            return "Rifles and lead-based heavy weapons have accuracy bonus as if your agility was " + (AccuracyBonus * 2).ToString() + " points higher.\n"
                + $"{DVPenalty} DV penalty in melee combat, {IncomingProjectileToHitPenalty} DV bonus in ranged combat if distance between you and attacker is larger than {MinProneDVDistance}.\n"
                + $"-{MovementSpeedPenalty} to movement speed.";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterEffectEvent(this, "BeginMove");
            Object.RegisterEffectEvent(this, "BeginTakeAction");
            Object.RegisterEffectEvent(this, "WeaponGetDefenderDV");
            Object.RegisterEffectEvent(this, "FiringMissile");
            Object.RegisterEffectEvent(this, "FiredMissileWeapon");
            base.Register(Object, Registrar);
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
                //Make AI get up after some time has passed
                //Object can be null after effect is removed, lets hope it won't screw up the game
                if (Duration > 0 && Object != null && !Object.IsPlayer())
                {
                    Duration--;
                }

                return true;
            }

            if (E.ID == "BeginTakeAction")
            {
                // After effect removal it still seems to exist without object
                if (Object == null)
                {
                    return true;
                }

                if (Object.HasEffect(typeof(Flying)) || Object.HasEffect(typeof(Running)))
                {
                    Object.RemoveEffect(typeof(WWA_ProneStance));
                    Object.UseEnergy(1000, "Physical");

                    if (Object.IsPlayer())
                    {
                        MessageQueue.AddPlayerMessage("You get up.");
                    }
                }

                return true;
            }

            if (E.ID == "FiringMissile")
            {
                if (Object == null)
                {
                    return true;
                }

                WWA_TacticalAbilities tactics = Object.GetPart<WWA_TacticalAbilities>();
                GameObject chosenWeapon = tactics.chosenWeapon;

                if (!ProneBonusApplied && chosenWeapon != null)
                {
                    MissileWeapon mw = tactics.GetMissileWeaponPart(chosenWeapon);
                    bool weaponIsPistol = false;

                    if (mw != null && mw.Skill == "Pistol")
                    {
                        weaponIsPistol = true;
                    }

                    if (chosenWeapon.HasPart("MagazineAmmoLoader") && !weaponIsPistol)
                    {
                        // MessageQueue.AddPlayerMessage("Prone accuracy bonus applied to " + chosenWeapon.ShortDisplayName + ".");
                        Object.ModIntProperty("MissileWeaponAccuracyBonus", AccuracyBonus, true);
                        ProneBonusApplied = true;
                    }
                }

                return true;
            }

            if (E.ID == "FiredMissileWeapon")
            {
                if (Object == null)
                {
                    return true;
                }

                if (ProneBonusApplied)
                {
                    // WWA_TacticalAbilities tactics = Object.GetPart<WWA_TacticalAbilities>();
                    // GameObject chosenWeapon = tactics.chosenWeapon;
                    // MessageQueue.AddPlayerMessage("Prone accuracy bonus unapplied to " + chosenWeapon.ShortDisplayName + ".");
                    Object.ModIntProperty("MissileWeaponAccuracyBonus", -AccuracyBonus, true);
                    ProneBonusApplied = false;
                }

                return true;
            }

            if (E.ID == "WeaponGetDefenderDV")
            {
                if (Object == null)
                {
                    return true;
                }

                GameObject attackerWeapon = E.GetParameter("Weapon") as GameObject;
                GameObject attacker = attackerWeapon.Equipped;

                int dif = attacker.CurrentCell.DistanceTo(Object);

                if (dif > MinProneDVDistance)
                {
                    // MessageQueue.AddPlayerMessage(Object.ShortDisplayName + " is far enough from " + attacker.ShortDisplayName + " to gain DV bonus. " + dif.ToString() + "/" + MinProneDVDistance.ToString());
                    E.SetParameter("Amount", (DVPenalty + IncomingProjectileToHitPenalty) * -1);
                }
                else
                {
                    // MessageQueue.AddPlayerMessage(Object.ShortDisplayName + " is too close to " + attacker.ShortDisplayName + " to gain DV bonus." + dif.ToString() + "/" + MinProneDVDistance.ToString());
                }

                return true;
            }

            return base.FireEvent(E);
        }
    }
}