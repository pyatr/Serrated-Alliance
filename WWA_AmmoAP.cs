using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_AmmoAP : WWA_AmmoPartOnLoad
    {
        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            base.Register(Object, Registrar);
        }

        public override bool HandleEvent(GetDisplayNameEvent E)
        {
            return true;
        }

        public override bool OnLoad(GameObject loader)
        {
            return true;
        }

        public override bool OnUnload(GameObject unloader)
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
            E.Postfix.AppendRules(GetDescription());
            return true;
        }

        public override string GetDescription()
        {
            string s = "";
            return base.GetDescription() + s;
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }
    }
}
