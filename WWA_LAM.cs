using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_LAM : WWA_Attachment
    {
        public int aimBonus = 1;

        public WWA_LAM()
        {
            displayName = "laser aiming module";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent((IPart)this, "ModifyAimVariance");
            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            string s = "Laser aiming module: +" + aimBonus.ToString() + " to player accuracy.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "ModifyAimVariance")
            {
				if (!this.ParentObject.HasEffect("ElectromagneticPulsed"))
				{
					int amount = E.GetIntParameter("Amount");
					E.SetParameter("Amount", amount + aimBonus);
				}
                return true;
            }
            return base.FireEvent(E);
        }
    }
}