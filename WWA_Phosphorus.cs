using System;
using System.Collections.Generic;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_Phosphorus : IPart
    {
        public string GasType = "Phosphorus";

        public override bool SameAs(IPart p)
        {
            if ((p as WWA_Phosphorus).GasType != this.GasType)
                return false;
            return base.SameAs(p);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && (ID != GeneralAmnestyEvent.ID && ID != GetNavigationWeightEvent.ID))
                return ID == ObjectEnteredCellEvent.ID;
            return true;
        }

        public override bool HandleEvent(EndTurnEvent E)
        {
            Cell currentCell = this.ParentObject.CurrentCell;
            if (currentCell != null)
            {
                foreach (GameObject GO in currentCell.GetObjectsWithPartReadonly("Physics"))
                    this.ApplyWP(GO);
            }
            return true;
        }

        public override bool HandleEvent(GetNavigationWeightEvent E)
        {
            if (CheckGasCanAffectEvent.Check(E.Actor, this.ParentObject, (Gas)null))
            {
                if (E.Smart)
                {
                    if (E.Actor == null || E.Actor.PhaseMatches(this.ParentObject))
                    {
                        int num1 = this.GasDensityStepped(5) / 2 + 15;
                        if (E.Actor != null)
                        {
                            int num2 = E.Actor.Stat("HeatResistance", 0);
                            if (num2 != 0)
                                num1 = num1 * (100 - num2) / 100;
                        }
                        E.MinWeight(num1, 65);
                    }
                }
                else
                    E.MinWeight(5);
            }
            return true;
        }

        public override bool HandleEvent(ObjectEnteredCellEvent E)
        {
            this.ApplyWP(E.Object);
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "DensityChange");
            Object.RegisterPartEvent((IPart)this, "EndTurn");
            base.Register(Object);
        }

        public void ApplyWP(GameObject GO)
        {
            if (GO == this.ParentObject)
                return;
            Gas part = this.ParentObject.GetPart("Gas") as Gas;
            if (!CheckGasCanAffectEvent.Check(GO, this.ParentObject, part) || !GO.PhaseAndFlightMatches(this.ParentObject)/* || GO.GetIntProperty("Inorganic", 0) != 0 || !GO.HasTag("Creature") && !GO.HasPart("Food")*/)
                return;
            Damage damage = new Damage((int)Math.Max(Math.Ceiling(0.220000007152557 * (double)part.Density), 1.0));
            damage.AddAttribute("Heat");
            damage.AddAttribute("Acid");
            damage.AddAttribute("NoBurn");
            Event E = Event.New("TakeDamage", 0, 0, 0);
            E.SetParameter("Damage", (object)damage);
            E.SetParameter("Owner", (object)part.Creator);
            E.SetParameter("Attacker", (object)part.Creator);
            E.SetParameter("Message", "from %o white phosphorus!");
            GO.FireEvent(E);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "DensityChange" && this.StepValue(E.GetIntParameter("OldValue", 0), 5) != this.StepValue(E.GetIntParameter("NewValue", 0), 5))
                this.FlushNavigationCaches();
            return base.FireEvent(E);
        }
    }
}