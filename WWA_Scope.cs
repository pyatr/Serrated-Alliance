using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_Scope : WWA_Attachment
    {
        public int aimingBonus = 2;
        public int energyPenalty = 100;

        public WWA_Scope()
        {
            displayName = "scope";
            worksOnSelect = true;
        }

        public override bool OnSelect(GameObject selector)
        {
            this.ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", aimingBonus, true);
            MissileWeapon mw = this.ParentObject.GetPart<MissileWeapon>();
            if (mw != null)
            {
                mw.EnergyCost += energyPenalty;
            }
            return base.OnSelect(selector);
        }

        public override bool OnDeselect()
        {
            this.ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -aimingBonus, true);
            MissileWeapon mw = this.ParentObject.GetPart<MissileWeapon>();
            if (mw != null)
            {
                mw.EnergyCost -= energyPenalty;
            }
            return base.OnDeselect();
        }

        public override string GetDescription()
        {
            string s = "Scope: Increases weapon accuracy by " + this.aimingBonus + ", +" + energyPenalty.ToString() + " energy cost.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }
    }
}