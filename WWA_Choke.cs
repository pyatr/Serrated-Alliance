using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_Choke : WWA_Attachment
    {
        public int accuracyBonus = 28;

        public WWA_Choke()
        {
            displayName = "choke";
        }

        public override void Register(GameObject Object)
        {
            base.Register(Object);
        }

        public override bool OnInstall()
        {
            MissileWeapon mw = this.ParentObject.GetPart("MissileWeapon") as MissileWeapon;
            if (mw != null)
                mw.WeaponAccuracy += accuracyBonus;
            return base.OnInstall();
        }

        public override bool OnUninstall()
        {
            MissileWeapon mw = this.ParentObject.GetPart("MissileWeapon") as MissileWeapon;
            if (mw != null)
                mw.WeaponAccuracy -= accuracyBonus;
            return base.OnUninstall();
        }

        public override string GetDescription()
        {
            string s = "Choke: significantly decreased pellet spread.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }
    }
}