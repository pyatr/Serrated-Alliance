using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_Foregrip : WWA_Attachment
    {
        public int EnergyCostMod = -100;

        public WWA_Foregrip()
        {
            displayName = "foregrip";
            worksOnSelect = true;
        }

        public override bool OnSelect(GameObject selector)
        {
            MissileWeapon mw = this.ParentObject.GetPart<MissileWeapon>() as MissileWeapon;
            if (mw != null)
            {
                mw.EnergyCost += EnergyCostMod;
            }
            return base.OnSelect(selector);
        }

        public override bool OnDeselect()
        {
            MissileWeapon mw = this.ParentObject.GetPart<MissileWeapon>() as MissileWeapon;
            if (mw != null)
            {
                mw.EnergyCost -= EnergyCostMod;
            }
            return base.OnDeselect();
        }

        public override string GetDescription()
        {
            string s = "Foregrip: " + EnergyCostMod.ToString() + " to firing energy cost.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }
    }
}