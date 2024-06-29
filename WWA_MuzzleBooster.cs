using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_MuzzleBooster : WWA_Attachment
    {
        public WWA_MuzzleBooster()
        {
            displayName = "muzzle booster";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent((IPart)this, "WeaponMissleWeaponFiring");
            Object.RegisterPartEvent((IPart)this, "ShotComplete");
            base.Register(Object, Registrar);
        }

        public override bool OnInstall()
        {
            WWA_GunFeatures gf = this.ParentObject.GetPart<WWA_GunFeatures>() as WWA_GunFeatures;
            if (gf != null)
            {
                gf.ModFireRate(1);
            }
            return base.OnInstall();
        }

        public override bool OnUninstall()
        {
            WWA_GunFeatures gf = this.ParentObject.GetPart<WWA_GunFeatures>() as WWA_GunFeatures;
            if (gf != null)
            {
                gf.ModFireRate(-1);
            }
            return base.OnUninstall();
        }

        public override string GetDescription()
        {
            string s = "Muzzle booster: +1 bullet per shot in automatic mode, -2 to weapon accuracy in automatic fire mode.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WeaponMissleWeaponFiring")
            {
                WWA_GunFeatures gf = this.ParentObject.GetPart<WWA_GunFeatures>() as WWA_GunFeatures;
                if (gf != null)
                {
                    if (gf.FireMode)
                    {
                        this.ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -2, true);
                    }
                }

                return true;
            }
            if (E.ID == "ShotComplete")
            {
                WWA_GunFeatures gf = this.ParentObject.GetPart<WWA_GunFeatures>() as WWA_GunFeatures;
                if (gf != null)
                {
                    if (gf.FireMode)
                    {
                        this.ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", 2, true);
                    }
                }
                return true;
            }
            return base.FireEvent(E);
        }
    }
}