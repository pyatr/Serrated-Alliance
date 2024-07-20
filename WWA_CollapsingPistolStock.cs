using System;
using XRL.UI;
using XRL.Messages;
using XRL.World;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_CollapsingPistolStock : WWA_Attachment
    {
        public int aimingBonus = 2;
        public int energyUse = 200;//1/5 of a turn
        public bool retracted = false;

        public WWA_CollapsingPistolStock()
        {
            displayName = "collapsing pistol stock";
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
                return ID == ZoneBuiltEvent.ID;
            return true;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            base.Register(Object, Registrar);
        }

        public override bool OnUninstall()
        {
            Collapse();
            return base.OnUninstall();
        }

        public override bool HandleEvent(GetInventoryActionsEvent E)
        {
            if (retracted)
                E.AddAction("Collapse stock", "collapse stock", "CollapseStock", null, 'c', false);
            else
                E.AddAction("Retract stock", "retract stock", "RetractStock", null, 'r', false);
            return true;
        }

        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (E.Command == "CollapseStock")
                FireEvent(Event.New("CollapseStock", "Actor", E.Actor));
            if (E.Command == "RetractStock")
                FireEvent(Event.New("RetractStock", "Actor", E.Actor));
            return true;
        }

        public void Retract(GameObject actor = null)
        {
            if (!retracted)
            {
                retracted = true;
                ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", aimingBonus, true);
                ParentObject.Physics.UsesTwoSlots = true;
                MissileWeapon mw = ParentObject.GetPart<MissileWeapon>();
                if (mw != null)
                {
                    mw.Skill = "Rifle";
                }
                if (actor != null)
                {
                    actor.UseEnergy(energyUse, "Physical");
                    if (ParentObject.Equipped != null)//If the weapon is equipped and not somewhere else
                        actor.ForceEquipObject(ParentObject, ParentObject.EquippedOn(), true);
                }
            }
        }

        public void Collapse(GameObject actor = null)
        {
            if (retracted)
            {
                retracted = false;
                ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -aimingBonus, true);
                ParentObject.Physics.UsesTwoSlots = false;
                MissileWeapon mw = ParentObject.GetPart<MissileWeapon>();
                if (mw != null)
                {
                    mw.Skill = "Pistol";
                }
                if (actor != null)
                {
                    actor.UseEnergy(energyUse, "Physical");
                    if (ParentObject.Equipped != null)
                        actor.ForceEquipObject(ParentObject, ParentObject.EquippedOn(), true);
                }
            }
        }

        public override string GetDescription()
        {
            string s = "Collapsing pistol stock: this pistol has a stock which when retracted increases player accuracy by " + aimingBonus.ToString() + " but uses both hands. It can retract and collapse at little energy cost. Weapon will use rifle skill.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CollapseStock")
            {
                Collapse(E.GetGameObjectParameter("Actor"));
                return true;
            }
            if (E.ID == "RetractStock")
            {
                Retract(E.GetGameObjectParameter("Actor"));
                return true;
            }
            return base.FireEvent(E);
        }
    }
}