using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_Dummy : WWA_Attachment
    {
        public WWA_Dummy()
        {
            displayName = "";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            base.Register(Object, Registrar);
        }

        public override bool OnInstall()
        {
            return base.OnInstall();
        }

        public override bool OnUninstall()
        {
            return base.OnUninstall();
        }

        public override bool OnSelect(GameObject selector)
        {
            return base.OnSelect(selector);
        }

        public override bool OnDeselect()
        {
            return base.OnDeselect();
        }

        public override bool OnEquip(GameObject equipper)
        {
            return base.OnEquip(equipper);
        }

        public override bool OnUnequip(GameObject unequipper)
        {
            return base.OnUnequip(unequipper);
        }

        public override string GetDescription()
        {
            string s = "";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }
    }
}