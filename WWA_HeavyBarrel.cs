using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_HeavyBarrel : WWA_Attachment
    {
        public WWA_HeavyBarrel()
        {
            displayName = "heavy barrel";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("WeaponMissleWeaponFiring");
            Registrar.Register("ShotComplete");
            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            string s = "Heavy barrel: +1 to weapon accuracy in automatic mode.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WeaponMissleWeaponFiring")
            {
                WWA_GunFeatures gf = ParentObject.GetPart<WWA_GunFeatures>();
                MissileWeapon mw = ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                if (gf != null && mw != null)
                {
                    if (gf.FireMode)
                        mw.WeaponAccuracy += 1;
                }
                return true;
            }
            if (E.ID == "ShotComplete")
            {
                WWA_GunFeatures gf = ParentObject.GetPart<WWA_GunFeatures>();
                MissileWeapon mw = ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                if (gf != null && mw != null)
                {
                    if (gf.FireMode)
                        mw.WeaponAccuracy -= 1;
                }
                return true;
            }
            return base.FireEvent(E);
        }
    }
}