using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Messages;
using XRL.UI;
using XRL.World;
using ConsoleLib.Console;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_Bayonet : WWA_Attachment
    {
        public Guid SwitchToBayonetAbilityID = Guid.Empty;
        public ActivatedAbilityEntry SwitchToBayonetAbility;

        public bool attachmentInstalled = false;
        [NonSerialized]
        public GameObject bayonetObject = null;

        public string ManagerID
        {
            get
            {
                return this.ParentObject.id + "::" + this.ParentObject.ShortDisplayName + ":bayonet";
            }
        }

        public WWA_Bayonet()
        {
            displayName = "bayonet lug";
        }

        public override void Register(GameObject Object)
        {
            base.Register(Object);
        }

        public override void SaveData(SerializationWriter Writer)
        {
            Writer.WriteGameObject(this.bayonetObject);
            base.SaveData(Writer);
        }

        public override void LoadData(SerializationReader Reader)
        {
            this.bayonetObject = Reader.ReadGameObject((string)null);
            base.LoadData(Reader);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
                return ID == ZoneBuiltEvent.ID;
            return true;
        }

        public override bool HandleEvent(GetInventoryActionsEvent E)
        {
            if (this.ParentObject.Equipped != null)
            {
                if (!attachmentInstalled)
                    E.AddAction("Add bayonet", "add bayonet", "AddBayonet", null, 'm', false);
                else
                    E.AddAction("Remove bayonet", "remove bayonet", "RemoveBayonet", null, 'y', false);
            }
            return true;
        }

        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (this.ParentObject.Equipped != null)
            {
                if (E.Command == "AddBayonet")
                    this.FireEvent(Event.New("AddBayonet", "Actor", (object)E.Actor));
                if (E.Command == "RemoveBayonet")
                    this.FireEvent(Event.New("RemoveBayonet", "Actor", (object)E.Actor));
            }
            return true;
        }

        public void InstallBayonet(GameObject bayonet)
        {
            if (!attachmentInstalled)
            {
                GameObject equipped = this.ParentObject.Equipped;
                if (equipped != null)
                {
                    BodyPart body = equipped.Body.GetBody();
                    if (body != null)
                    {
                        bayonetObject = bayonet;
                        BodyPart UBslot = null;                        
                        UBslot = body.AddPartAt("Bayonet", 0, (string)null, (string)null, (string)null, (string)null, this.ManagerID, new int?(), new int?(), new int?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), "Missile Weapon", new string[3]
                        {
                        "Hands",
                        "Feet",
                        "Thrown Weapon"
                        }, true);
                        bayonetObject.AddPart(new Cursed());
                        equipped.ForceEquipObject(bayonetObject, UBslot, true);
                        equipped.RegisterPartEvent((IPart)this, "CommandSwitchToBayonet");
                        attachmentInstalled = true;
                        ActivatedAbilities pAA = equipped.GetPart<ActivatedAbilities>();
                        if (pAA != null)
                        {
                            this.SwitchToBayonetAbilityID = pAA.AddAbility("Switch to " + bayonetObject.ShortDisplayName, "CommandSwitchToBayonet", "Tactics", "Switch to " + bayonetObject.ShortDisplayName + ".", "\a", null, false, true, false, false, false, true);
                            this.SwitchToBayonetAbility = pAA.AbilityByGuid[this.SwitchToBayonetAbilityID];
                        }
                    }
                }
            }
        }

        public void RemoveBayonet()
        {
            if (attachmentInstalled)
            {
                bayonetObject.RemovePart("Cursed");
                bayonetObject.ForceUnequip(true);
                attachmentInstalled = false;
                this.ParentObject.Equipped.UnregisterPartEvent((IPart)this, "CommandSwitchToBayonet");
                this.ParentObject.Equipped.RemoveBodyPartsByManager(this.ManagerID);
                if (this.SwitchToBayonetAbilityID != Guid.Empty)
                {
                    ActivatedAbilities pAA = this.ParentObject.Equipped.GetPart<ActivatedAbilities>();
                    pAA.RemoveAbility(this.SwitchToBayonetAbilityID);
                    this.SwitchToBayonetAbilityID = Guid.Empty;
                }
            }
        }

        public override bool OnUnequip(GameObject unequipper)
        {
            RemoveBayonet();
            return base.OnUnequip(unequipper);
        }

        public override bool OnDeselect()
        {
            RemoveBayonet();
            return base.OnDeselect();
        }

        public override string GetDescription()
        {
            string s = "";
            if (installed)
                s = displayName + ": you can attach short blades to this weapon when it is equipped.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandSwitchToBayonet")
            {
                BodyPart lug = bayonetObject.EquippedOn();
                lug.SetAsPreferredDefault(true);
                return true;
            }
            if (E.ID == "AddBayonet")
            {
                GameObject owner = E.GetGameObjectParameter("Actor");
                List<GameObject> inventory = owner.GetPart<Inventory>().Objects;
                Dictionary<string, GameObject> shortBlades = new Dictionary<string, GameObject>();
                foreach (GameObject GO in inventory)
                {
                    MeleeWeapon mw = GO.GetPart<MeleeWeapon>();
                    if (mw != null)
                        if (mw.Skill == "ShortBlades")
                            shortBlades.Add(GO.DisplayName, GO);
                }
                if (shortBlades.Count > 0)
                    InstallBayonet(shortBlades.Values.ElementAt(Popup.ShowOptionList("Choose short blade", shortBlades.Keys.ToArray())));
                else if (owner.IsPlayer())
                    MessageQueue.AddPlayerMessage("You don't have any short blades to attach on " + this.ParentObject.ShortDisplayName + " bayonet lug.");
                return true;
            }
            if (E.ID == "RemoveBayonet")
            {
                RemoveBayonet();
                return true;
            }
            return base.FireEvent(E);
        }
    }
}