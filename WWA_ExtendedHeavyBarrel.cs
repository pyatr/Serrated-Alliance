﻿using System;
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

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("WeaponMissleWeaponFiring");
            Registrar.Register("ShotComplete");
            base.Register(Object, Registrar);
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
                MissileWeapon mw = ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                if (mw != null)                
                    mw.WeaponAccuracy += 1;                
                return true;
            }
            if (E.ID == "ShotComplete")
            {
                MissileWeapon mw = ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                if (mw != null)                
                    mw.WeaponAccuracy -= 1;                
                return true;
            }
            return base.FireEvent(E);
        }
    }
}