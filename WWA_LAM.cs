using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_LAM : WWA_Attachment
    {
        public bool weaponHadEnergySocket;
        public int aimBonus = 1;
        public int energyUse = 20;

        public WWA_LAM()
        {
            displayName = "laser aiming module";
            weaponHadEnergySocket = false;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "ModifyAimVariance");
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
            string s = "Laser aiming module: +" + aimBonus.ToString() + " to player accuracy.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "ModifyAimVariance")
            {
                EnergyCellSocket ecs = this.ParentObject.GetPart("EnergyCellSocket") as EnergyCellSocket;
                EnergyCell cell = ecs.Cell.GetPart("EnergyCell") as EnergyCell;
                if (cell.Charge > energyUse)
                {
                    cell.UseCharge(energyUse);
                    int amount = E.GetIntParameter("Amount");
                    E.SetParameter("Amount", amount + aimBonus);
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