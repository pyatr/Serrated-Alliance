﻿using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Messages;
using XRL.UI;
using XRL.World;
using XRL.World.Anatomy;
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
                return ParentObject.ID + "::" + ParentObject.ShortDisplayName + ":bayonet";
            }
        }

        public WWA_Bayonet()
        {
            displayName = "bayonet lug";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            base.Register(Object, Registrar);
        }

        public override void Write(GameObject basis,SerializationWriter Writer)
        {
            Writer.WriteGameObject(bayonetObject);
            base.Write(basis, Writer);
        }

        public override void Read(GameObject basis,SerializationReader Reader)
        {
            bayonetObject = Reader.ReadGameObject(null);
            base.Read(basis, Reader);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
                return ID == ZoneBuiltEvent.ID;
            return true;
        }

        public override bool HandleEvent(GetInventoryActionsEvent E)
        {
            if (ParentObject.Equipped != null)
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
            if (ParentObject.Equipped != null)
            {
                if (E.Command == "AddBayonet")
                    FireEvent(Event.New("AddBayonet", "Actor", E.Actor));
                if (E.Command == "RemoveBayonet")
                    FireEvent(Event.New("RemoveBayonet", "Actor", E.Actor));
            }
            return true;
        }

        public void InstallBayonet(GameObject bayonet)
        {
            if (!attachmentInstalled)
            {
                GameObject equipped = ParentObject.Equipped;
                if (equipped != null)
                {
                    BodyPart body = equipped.Body.GetBody();
                    if (body != null)
                    {
                        bayonetObject = bayonet;
                        BodyPart UBslot = null;
                        UBslot = body.AddPartAt("Bayonet", 0, null, null, null, null, ManagerID, new int?(), new int?(), new int?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), "Missile Weapon", new string[3]
                        {
                        "Hands",
                        "Feet",
                        "Thrown Weapon"
                        }, true);
                        bayonetObject.AddPart(new Cursed());
                        equipped.ForceEquipObject(bayonetObject, UBslot, true);
                        equipped.RegisterPartEvent(this, "CommandSwitchToBayonet");
                        attachmentInstalled = true;
                        ActivatedAbilities pAA = equipped.GetPart<ActivatedAbilities>();
                        if (pAA != null)
                        {
                            SwitchToBayonetAbilityID = pAA.AddAbility("Switch to " + bayonetObject.ShortDisplayName, "CommandSwitchToBayonet", "Tactics", "Switch to " + bayonetObject.ShortDisplayName + ".", "\a", null, false, true, false, false, false, true);
                            SwitchToBayonetAbility = pAA.AbilityByGuid[SwitchToBayonetAbilityID];
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
                ParentObject.Equipped.RemoveBodyPartsByManager(ManagerID);
                ParentObject.Equipped.UnregisterPartEvent(this, "CommandSwitchToBayonet");
                if (SwitchToBayonetAbilityID != Guid.Empty)
                {
                    ActivatedAbilities pAA = ParentObject.Equipped.GetPart<ActivatedAbilities>();
                    pAA.RemoveAbility(SwitchToBayonetAbilityID);
                    SwitchToBayonetAbilityID = Guid.Empty;
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
                    InstallBayonet(shortBlades.Values.ElementAt(Popup.PickOption("Choose short blade", null, "", null, shortBlades.Keys.ToArray())));
                else if (owner.IsPlayer())
                    MessageQueue.AddPlayerMessage("You don't have any short blades to attach on " + ParentObject.ShortDisplayName + " bayonet lug.");
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