using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_AmmoPartOnLoad : IPart
    {
        public int diceModifier = 0;
        public int diceSideModifier = 0;
        public int flatModifier = 0;

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent((IPart)this, "ObjectCreated");
            base.Register(Object, Registrar);
        }
        
        public override bool HandleEvent(GetDisplayNameEvent E)
        {
            return true;
        }

        public virtual bool OnLoad(GameObject loader)
        {
            return true;
        }

        public virtual bool OnUnload(GameObject unloader)
        {
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
            E.Postfix.AppendRules(this.GetDescription());
            return true;
        }

        public virtual string GetDescription()
        {
            string s = "";
            return s;
        }

        public override bool FireEvent(Event E)
        {
            if (!(E.ID == "ObjectCreated"))
                return true;
            return base.FireEvent(E);
        }
    }
}