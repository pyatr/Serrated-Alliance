using System;
using System.Collections.Generic;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_Phosphorus : IGasBehavior
    {
        public string GasType = "Phosphorus";

        public override bool SameAs(IPart p)
        {
            if ((p as WWA_Phosphorus).GasType != GasType)
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
            Cell currentCell = ParentObject.CurrentCell;
            if (currentCell != null)
            {
                foreach (GameObject GO in currentCell.GetObjectsWithPartReadonly("Physics"))
                    ApplyWP(GO);
            }
            return true;
        }

        public override bool HandleEvent(GetNavigationWeightEvent E)
        {
            if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject, null))
            {
                if (E.Smart)
                {
                    if (E.Actor == null || E.Actor.PhaseMatches(ParentObject))
                    {
                        int num1 = GasDensityStepped(5) / 2 + 15;
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
                {
                    E.MinWeight(5);
                }
            }
            return true;
        }

        public override bool HandleEvent(ObjectEnteredCellEvent E)
        {
            ApplyWP(E.Object);
            return true;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("DensityChange");
            Registrar.Register("EndTurn");
            base.Register(Object, Registrar);
        }

        public void ApplyWP(GameObject GO)
        {
            if (GO == ParentObject)
                return;
            Gas part = ParentObject.GetPart("Gas") as Gas;
            if (!CheckGasCanAffectEvent.Check(GO, ParentObject, part) || !GO.PhaseAndFlightMatches(ParentObject)/* || GO.GetIntProperty("Inorganic", 0) != 0 || !GO.HasTag("Creature") && !GO.HasPart("Food")*/)
                return;
            Damage damage = new Damage((int)Math.Max(Math.Ceiling(0.220000007152557 * part.Density), 1.0));
            damage.AddAttribute("Heat");
            damage.AddAttribute("Acid");
            damage.AddAttribute("NoBurn");
            Event E = Event.New("TakeDamage", 0, 0, 0);
            E.SetParameter("Damage", damage);
            E.SetParameter("Owner", part.Creator);
            E.SetParameter("Attacker", part.Creator);
            E.SetParameter("Message", "from %o white phosphorus!");
            GO.FireEvent(E);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "DensityChange" && StepValue(E.GetIntParameter("OldValue", 0), 5) != StepValue(E.GetIntParameter("NewValue", 0), 5))
                FlushNavigationCaches();
            return base.FireEvent(E);
        }
    }
}