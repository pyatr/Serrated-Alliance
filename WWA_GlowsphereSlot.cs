using System;
using System.Linq;
using System.Collections.Generic;
using XRL.Messages;
using XRL.UI;
using XRL.World.Anatomy;

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
                return ParentObject.ID + "::glowsphere";
            }
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            base.Register(Object, Registrar);
        }

        public override void Write(GameObject basis,SerializationWriter Writer)
        {
            Writer.WriteGameObject(glowsphereObject);
            base.Write(basis, Writer);
        }

        public override void Read(GameObject basis,SerializationReader Reader)
        {
            glowsphereObject = Reader.ReadGameObject(null);
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
                    E.AddAction("Attach glowsphere", "attach glowsphere", "AttachGlowsphere", null, 'n', false);
                else
                    E.AddAction("Remove glowsphere", "remove glowsphere", "RemoveGlowsphere", null, 'd', false);
            }
            return true;
        }

        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (ParentObject.Equipped != null)
            {
                if (E.Command == "AttachGlowsphere")
                    FireEvent(Event.New("AttachGlowsphere", "Actor", E.Actor));
                if (E.Command == "RemoveGlowsphere")
                    FireEvent(Event.New("RemoveGlowsphere", "Actor", E.Actor));
            }
            return true;
        }

        public void InstallGlowsphere(GameObject glowsphere)
        {
            if (!attachmentInstalled)
            {
                GameObject equipped = ParentObject.Equipped;
                if (equipped != null)
                {
                    BodyPart body = equipped.Body.GetBody();
                    if (body != null)
                    {
                        glowsphereObject = glowsphere;
                        BodyPart UBslot = body.AddPartAt("Glowsphere", 0, null, null, null, null, ManagerID, new int?(), new int?(), new int?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), new bool?(), "Missile Weapon", new string[3]
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
                ParentObject.Equipped.RemoveBodyPartsByManager(ManagerID);
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
                    GameObject sphere = glowSpheres.Values.ElementAt(Popup.PickOption("Choose glowsphere", null, "", null/* + ". Come on, they're all the same." */, options));
                    InstallGlowsphere(sphere);
                }
                else
                {
                    if (owner.IsPlayer())
                        MessageQueue.AddPlayerMessage("You don't have any glowspheres to attach on " + ParentObject.ShortDisplayName + ".");
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