using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_MuzzleBrake : WWA_Attachment
    {
        public WWA_MuzzleBrake()
        {
            displayName = "recoil compensator";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("WeaponMissleWeaponFiring");
            Registrar.Register("ShotComplete");
            base.Register(Object, Registrar);
        }
        
        public override string GetDescription()
        {
            string s = "Recoil compensator: +1 player accuracy in automatic mode.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WeaponMissleWeaponFiring")
            {
                WWA_GunFeatures gf = ParentObject.GetPart<WWA_GunFeatures>();
                if (gf != null)
                {
                    if (gf.AutomaticFireMode)
                    {
                        ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", 1, true);
                    }
                }
                return true;
            }
            if (E.ID == "ShotComplete")
            {
                WWA_GunFeatures gf = ParentObject.GetPart<WWA_GunFeatures>();
                if (gf != null)
                {
                    if (gf.AutomaticFireMode)
                    {
                        ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -1, true);
                    }
                }
                return true;
            }
            return base.FireEvent(E);
        }
    }
}