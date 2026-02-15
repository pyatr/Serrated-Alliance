using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_Attachment : IPart
    {
        public string displayName;
        public string AttachmentBlueprintName;
        public bool integral = false;
        public bool installed = false;
        public bool worksOnSelect = false;

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("Equipped");
            Registrar.Register("Unequipped");
            Registrar.Register("ObjectCreated");
            base.Register(Object, Registrar);
        }

        public WWA_GunFeatures GetGunFeatures()
        {
            WWA_GunFeatures gf = ParentObject.GetPart<WWA_GunFeatures>();
            return gf;
        }

        public override bool HandleEvent(GetDisplayNameEvent E)
        {
            return true;
        }

        public virtual bool OnSelect(GameObject selector)
        {
            return true;
        }

        public virtual bool OnDeselect()
        {
            return true;
        }

        public virtual bool OnEquip(GameObject equipper)
        {
            //MessageQueue.AddPlayerMessage(this.Name + " equippd.");
            return true;
        }

        public virtual bool OnUnequip(GameObject unequipper)
        {
            //MessageQueue.AddPlayerMessage(this.Name + " unequiped.");
            return true;
        }

        public virtual bool OnInstall()
        {
            //MessageQueue.AddPlayerMessage(this.Name + " installed.");
            installed = true;
            return true;
        }

        public virtual bool OnUninstall()
        {
            //MessageQueue.AddPlayerMessage(this.Name + " uninstalled.");
            installed = false;
            return true;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade))
                return ID == GetShortDescriptionEvent.ID;
            return true;
        }

        public override bool HandleEvent(GetShortDescriptionEvent E)
        {
            E.Postfix.AppendRules(GetDescription());
            return true;
        }

        public virtual string GetDescription()
        {
            string s = "\n";
            if (integral && !worksOnSelect)
                s += "This attachment is integral and cannot be removed.";
            if (worksOnSelect && !integral)
                s += "This attachment functions only when its weapon is selected.";
            if (worksOnSelect && integral)
                s += "This attachment is integral and cannot be removed. It only functions when its weapon is selected.";
            return s;
        }

        protected bool CheckParentObject()
        {
            if (ParentObject == null)
            {
                MessageQueue.AddPlayerMessage("No parent object");

                return false;
            }

            if (ParentObject.Equipped == null)
            {
                MessageQueue.AddPlayerMessage("No equipped on parent object");

                return false;
            }

            MessageQueue.AddPlayerMessage("Got both parent object and equipped");

            return true;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "Equipped")
            {
                OnEquip(E.GetParameter<GameObject>("EquippingObject"));
                return true;
            }
            if (E.ID == "Unequipped")
            {
                OnUnequip(E.GetParameter<GameObject>("UnequippingObject"));
                return true;
            }
            if (!(E.ID == "ObjectCreated"))
                return true;
            if (ParentObject.HasPart("MissileWeapon"))
                OnInstall();
            return base.FireEvent(E);
        }
    }
}