using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_ExtendedHeavyBarrel : WWA_Attachment
    {
        public WWA_ExtendedHeavyBarrel()
        {
            displayName = "extended heavy barrel";
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "WeaponMissleWeaponFiring");
            Object.RegisterPartEvent((IPart)this, "ShotComplete");
            base.Register(Object);
        }

        public override string GetDescription()
        {
            string s = "Extended heavy barrel: +1 to weapon accuracy in both fire modes.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WeaponMissleWeaponFiring")
            {
                MissileWeapon mw = this.ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                if (mw != null)                
                    mw.WeaponAccuracy += 1;                
                return true;
            }
            if (E.ID == "ShotComplete")
            {
                MissileWeapon mw = this.ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                if (mw != null)                
                    mw.WeaponAccuracy -= 1;                
                return true;
            }
            return base.FireEvent(E);
        }
    }
}