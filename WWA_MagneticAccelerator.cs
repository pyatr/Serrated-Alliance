using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_MagneticAccelerator : WWA_Attachment
    {
        public bool weaponHadEnergySocket;
        public int penetrationBonus = 3;
        public int energyUse = 70;

        public WWA_MagneticAccelerator()
        {
            displayName = "magnetic accelerator";
            weaponHadEnergySocket = false;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "WeaponMissileWeaponHit");            
            base.Register(Object);
        }

        public override bool OnInstall()
        {
            if (this.ParentObject.HasPart("EnergyCellSocket"))
            {
                EnergyCellSocket ecs = this.ParentObject.GetPart("EnergyCellSocket") as EnergyCellSocket;
                if (ecs.SlotType == "EnergyCell")
                {
                    weaponHadEnergySocket = true;
                }
            }
            else
            {
                EnergyCellSocket ecs = new EnergyCellSocket();
                ecs.SlotType = "EnergyCell";
                ecs.ChanceSlotted = 0;
                ecs.ChanceFullCell = 0;
                this.ParentObject.AddPart(ecs);
            }
            return base.OnInstall();
        }
        
        public override bool OnUninstall()
        {
            if (!weaponHadEnergySocket)
            {
                try
                {
                    InventoryActionEvent.Check(this.ParentObject, this.ParentObject.Equipped, this.ParentObject, "EmptyForDisassemble", false, false, false, 0, 0, (XRL.World.GameObject)null, (Cell)null, (Cell)null);
                }
                catch (Exception ex)
                {
                    MessageQueue.AddPlayerMessage("Could not remove cell. It was claimed by the void. " + ex.ToString());
                }
                this.ParentObject.RemovePart("EnergyCellSocket");
            }
            return base.OnUninstall();
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
                EnergyCellSocket ecs = this.ParentObject.GetPart("EnergyCellSocket") as EnergyCellSocket;
                EnergyCell cell = ecs.Cell.GetPart("EnergyCell") as EnergyCell;
                if (cell.Charge > energyUse)
                {
                    cell.UseCharge(energyUse);
                    int p = E.GetIntParameter("Penetrations");
                    int pc = E.GetIntParameter("PenetrationCap");
                    E.SetParameter("Penetrations", p + penetrationBonus);
                    E.SetParameter("PenetrationCap", pc + penetrationBonus);
                }
                else
                {
                    MessageQueue.AddPlayerMessage(this.ParentObject.ShortDisplayName + " does not have enough energy charge!");
                }
                return true;
            }
            return base.FireEvent(E);
        }
    }
}