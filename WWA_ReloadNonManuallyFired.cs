using System;
using System.Collections.Generic;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_ReloadNonManuallyFired : IPart
    {
        private MissileWeapon ParentWeapon
        {
            get
            {
                if (parentWeapon == null)
                    parentWeapon = this.ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                return parentWeapon;
            }
        }

        [NonSerialized]
        private MissileWeapon parentWeapon = null;

        public WWA_ReloadNonManuallyFired() { }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            //Doesn't matter what event this is, it's required for FireEvent to be called at all
            Object.RegisterPartEvent((IPart)this, "EndTurn");
            base.Register(Object, Registrar);
        }

        //Very important to for HandleEvent to be called
        public override bool WantEvent(int ID, int cascade)
        {
            return true;
        }

        public override bool HandleEvent(CommandReloadEvent E)
        {
            ParentWeapon.FiresManually = true;
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (E.Command == "LoadMagazineAmmo") { }
            return base.HandleEvent(E);
        }

        public override bool FireEvent(Event E)
        {
            ParentWeapon.FiresManually = false;
            return base.FireEvent(E);
        }
    }
}
