using System;
using XRL.Messages;
using XRL.World.Effects;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_Bipod : WWA_Attachment
    {
        public WWA_Bipod()
        {
            displayName = "bipod";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("WeaponMissleWeaponFiring");
            Registrar.Register("ShotComplete");
            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            string s = "Bipod: +1 to single shot accuracy in prone stance, +2 to automatic accuracy in prone stance.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WeaponMissleWeaponFiring")
            {
                if (ParentObject.Equipped.HasEffect(typeof(WWA_ProneStance)))
                {
                    WWA_GunFeatures gf = ParentObject.GetPart<WWA_GunFeatures>();

                    if (gf != null)
                    {
                        if (gf.AutomaticFireMode)
                        {
                            ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", 2, true);
                            // MessageQueue.AddPlayerMessage("Bipod accuracy bonus (2) applied to " + this.ParentObject.ShortDisplayName + ".");
                        }
                        else
                        {
                            ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", 1, true);
                            // MessageQueue.AddPlayerMessage("Bipod accuracy bonus (1) applied to " + this.ParentObject.ShortDisplayName + ".");
                        }
                    }
                }
                return true;
            }
            if (E.ID == "ShotComplete")
            {
                if (ParentObject.Equipped.HasEffect(typeof(WWA_ProneStance)))
                {
                    WWA_GunFeatures gf = ParentObject.GetPart<WWA_GunFeatures>();

                    if (gf != null)
                    {
                        if (gf.AutomaticFireMode)
                        {
                            ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -2, true);
                            // MessageQueue.AddPlayerMessage("Bipod accuracy bonus (2) unapplied to " + this.ParentObject.ShortDisplayName + ".");
                        }
                        else
                        {
                            ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -1, true);
                            // MessageQueue.AddPlayerMessage("Bipod accuracy bonus (1) unapplied to " + this.ParentObject.ShortDisplayName + ".");
                        }
                    }
                }
                return true;
            }
            return base.FireEvent(E);
        }
    }
}