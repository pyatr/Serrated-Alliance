using System;
using System.Linq;
using System.Collections.Generic;
using XRL.Messages;
using XRL.UI;
using XRL.World;
using XRL.World.Anatomy;
using ConsoleLib.Console;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_GlowsphereSlot : WWA_Attachment
    {
        public WWA_GlowsphereSlot()
        {
            displayName = "glowsphere slot";
        }

        public bool attachmentInstalled = false;
        [NonSerialized]
        public GameObject glowsphereObject = null;

        public string ManagerID
        {
            get
            {
                return this.ParentObject.ID + "::" + this.ParentObject.ShortDisplayName + ":glowsphere";
            }
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            base.Register(Object, Registrar);
        }

        public override void SaveData(SerializationWriter Writer)
        {
            Writer.WriteGameObject(this.glowsphereObject);
            base.SaveData(Writer);
        }

        public override void LoadData(SerializationReader Reader)
        {
            this.glowsphereObject = Reader.ReadGameObject((string)null);
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
                    E.AddAction("Attach glowsphere", "attach glowsphere", "AttachGlowsphere", null, 'n', false);
                else
                    E.AddAction("Remove glowsphere", "remove glowsphere", "RemoveGlowsphere", null, 'd', false);
            }
            return true;
        }

        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (this.ParentObject.Equipped != null)
            {
                if (E.Command == "AttachGlowsphere")
                    this.FireEvent(Event.New("AttachGlowsphere", "Actor", (object)E.Actor));
                if (E.Command == "RemoveGlowsphere")
                    this.FireEvent(Event.New("RemoveGlowsphere", "Actor", (object)E.Actor));
            }
            return true;
        }

        public void InstallGlowsphere(GameObject glowsphere)
        {
            if (!attachmentInstalled)
            {
                GameObject equipped = this.ParentObject.Equipped;
                if (equipped != null)
                {
                    BodyPart body = equipped.Body.GetBody();
                    if (body != null)
                    {
                        glowsphereObject = glowsphere;
                        BodyPart UBslot = null;
                        UBslot = body.AddPartAt("Glowsphere", 0, (string)null, (string)null, (string)null, (string)null, this.ManagerID, new int?(), new int?(), new int?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), "Missile Weapon", new string[3]
                        {
                        "Hands",
                        "Feet",
                        "Thrown Weapon"
                        }, true);
                        glowsphereObject.AddPart(new Cursed());
                        equipped.ForceEquipObject(glowsphereObject, UBslot, true);
                        attachmentInstalled = true;
                    }
                }
            }
        }

        public void RemoveGlowsphere()
        {
            if (attachmentInstalled)
            {
                glowsphereObject.RemovePart("Cursed");
                glowsphereObject.ForceUnequip(true);
                attachmentInstalled = false;
                this.ParentObject.Equipped.RemoveBodyPartsByManager(this.ManagerID);
            }
        }

        public override bool OnUnequip(GameObject unequipper)
        {
            RemoveGlowsphere();
            return base.OnUnequip(unequipper);
        }

        public override bool OnDeselect()
        {
            RemoveGlowsphere();
            return base.OnDeselect();
        }

        public override string GetDescription()
        {
            string s = "";
            if (installed)
                s = displayName + ": you can attach glowsphere to this weapon when it is equipped.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "AttachGlowsphere")
            {
                GameObject owner = E.GetGameObjectParameter("Actor");
                List<GameObject> inventory = owner.GetPart<Inventory>().Objects;
                Dictionary<string, GameObject> glowSpheres = new Dictionary<string, GameObject>();
                foreach (GameObject GO in inventory)
                {
                    if (GO.Blueprint == "Glowsphere"/* || GO.Blueprint == "Floating Glowsphere"*/)//Floating glowspheres for some reason can be unequipped
                        glowSpheres.Add(GO.DisplayName, GO);
                }
                if (glowSpheres.Count > 0)
                {
                    string[] options = glowSpheres.Keys.ToArray();
                    GameObject sphere = glowSpheres.Values.ElementAt(Popup.ShowOptionList("Choose glowsphere"/* + ". Come on, they're all the same." */, options));
                    InstallGlowsphere(sphere);
                }
                else
                {
                    if (owner.IsPlayer())
                        MessageQueue.AddPlayerMessage("You don't have any glowspheres to attach on " + this.ParentObject.ShortDisplayName + ".");
                }
                return true;
            }
            if (E.ID == "RemoveGlowsphere")
            {
                RemoveGlowsphere();
                return true;
            }
            return base.FireEvent(E);
        }
    }
}