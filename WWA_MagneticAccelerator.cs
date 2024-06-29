using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_MagneticAccelerator : WWA_Attachment
    {
        public int penetrationBonus = 3;

        public WWA_MagneticAccelerator()
        {
            displayName = "magnetic accelerator";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent((IPart)this, "WeaponMissileWeaponHit");            
            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            string s = "Magnetic accelerator: weapon penetration increased by " + penetrationBonus.ToString() + ".";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WeaponMissileWeaponHit")
            {
				if (!this.ParentObject.HasEffect("ElectromagneticPulsed"))
				{
					int p = E.GetIntParameter("Penetrations");
					int pc = E.GetIntParameter("PenetrationCap");
					E.SetParameter("Penetrations", p + penetrationBonus);
					E.SetParameter("PenetrationCap", pc + penetrationBonus);
				}
                return true;
            }
            return base.FireEvent(E);
        }
    }
}