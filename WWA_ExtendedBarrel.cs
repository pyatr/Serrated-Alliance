using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_ExtendedBarrel : WWA_Attachment
    {
        public WWA_ExtendedBarrel()
        {
            displayName = "extended barrel";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent((IPart)this, "WeaponMissleWeaponFiring");
            Object.RegisterPartEvent((IPart)this, "ShotComplete");
            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            string s = "Extended barrel: +1 to weapon accuracy in semi-automatic mode.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WeaponMissleWeaponFiring")
            {
                WWA_GunFeatures gf = this.ParentObject.GetPart<WWA_GunFeatures>() as WWA_GunFeatures;
                MissileWeapon mw = this.ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                if (gf != null && mw != null)
                {
                    if (!gf.FireMode)
                        mw.WeaponAccuracy += 1;

                }
                return true;
            }
            if (E.ID == "ShotComplete")
            {
                WWA_GunFeatures gf = this.ParentObject.GetPart<WWA_GunFeatures>() as WWA_GunFeatures;
                MissileWeapon mw = this.ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                if (gf != null && mw != null)
                {
                    if (!gf.FireMode)
                        mw.WeaponAccuracy -= 1;

                }
                return true;
            }
            return base.FireEvent(E);
        }
    }
}