using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using XRL.Messages;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Anatomy;
using ConsoleLib.Console;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_TacticalAbilities : IPart
    {
        [NonSerialized]
        public GameObject chosenWeapon = null;

        [NonSerialized]
        public List<GameObject> activeWeapons = new List<GameObject>();

        public Guid SelectPrimaryWeaponID = Guid.Empty;
        public ActivatedAbilityEntry SelectPrimaryWeapon = null;
        public Guid DeselectPrimaryWeaponID = Guid.Empty;
        public ActivatedAbilityEntry DeselectPrimaryWeapon = null;
        public Guid SwitchFireModeID = Guid.Empty;
        public ActivatedAbilityEntry SwitchFireMode = null;
        public Guid GoProneID = Guid.Empty;
        public ActivatedAbilityEntry GoProne = null;

        public int ProneAimBonus = 1;

        public bool IsMissileWeapon(GameObject GO) => GO != null && GO.HasTag("MissileWeapon");
        public MissileWeapon GetMissileWeaponPart(GameObject GO) => GO.GetPart<MissileWeapon>();
        public WWA_GunFeatures GetGunFeaturesPart(GameObject GO) => GO.GetPart<WWA_GunFeatures>();

        public WWA_TacticalAbilities()
        {
            activeWeapons = new List<GameObject>();
        }

        public override void Write(GameObject basis, SerializationWriter Writer)
        {
            Writer.WriteGameObject(chosenWeapon);
            Writer.WriteGameObjectList(activeWeapons);
            base.Write(basis, Writer);
        }

        public override void Read(GameObject basis, SerializationReader Reader)
        {
            chosenWeapon = Reader.ReadGameObject();
            activeWeapons = new List<GameObject>();
            Reader.ReadGameObjectList(activeWeapons);
            base.Read(basis, Reader);
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("CommandSelectWeapon");
            Registrar.Register("CommandDeselectWeapon");
            Registrar.Register("CommandSwitchFireMode");
            Registrar.Register("CommandGoProne");
            Registrar.Register("ObjectCreated");
            Registrar.Register("BeginEquip");
            Registrar.Register("BeginUnequip");
            base.Register(Object, Registrar);
        }

        public override bool HandleEvent(AIGetDefensiveAbilityListEvent E)
        {
            // If AI
            // has weapon
            // is a humanoid
            // smart enough to go prone
            // it's far enough
            // and it has an accurate enough weapon to hit at current distance it can go prone
            if (chosenWeapon != null &&
                ParentObject.GetPart<Body>().Anatomy == "Humanoid" &&
                ParentObject.GetStat("Intelligence").Value >= 16 &&
                E.Distance > 5 &&
                E.Distance > chosenWeapon.GetPart<MissileWeapon>().WeaponAccuracy)
            {
                E.Add("CommandGoProne");
            }

            return base.HandleEvent(E);
        }

        public void AddAbilities()
        {
            ActivatedAbilities pAA = ParentObject.GetPart<ActivatedAbilities>();
            if (pAA != null)
            {
                SelectPrimaryWeaponID = pAA.AddAbility(
                    "Select primary missile weapon",
                    "CommandSelectWeapon",
                    "Tactics",
                    "Select a specific missile weapon."
                );
                SelectPrimaryWeapon = pAA.AbilityByGuid[SelectPrimaryWeaponID];
                DeselectPrimaryWeaponID = pAA.AddAbility(
                    "Deselect missile weapon",
                    "CommandDeselectWeapon",
                    "Tactics"
                );
                DeselectPrimaryWeapon = pAA.AbilityByGuid[DeselectPrimaryWeaponID];
                DeselectPrimaryWeapon.Enabled = false;
                SwitchFireModeID = pAA.AddAbility(
                    "Switch fire mode",
                    "CommandSwitchFireMode",
                    "Tactics",
                    "Choose between automatic and semi-automatic fire mode. Some weapons only have automatic fire mode."
                );
                SwitchFireMode = pAA.AbilityByGuid[SwitchFireModeID];
                GoProneID = pAA.AddAbility(
                    "Go prone",
                    "CommandGoProne",
                    "Tactics",
                    "Go prone. If your current missile weapon is a firearm or energy rifle or a firearm heavy weapon, you shoot as if your agility was "
                        + (ProneAimBonus * 2).ToString()
                        + " points higher."
                );
                GoProne = pAA.AbilityByGuid[GoProneID];
            }
        }

        public void SwitchAutomatic()
        {
            if (chosenWeapon == null)
            {
                return;
            }

            WWA_GunFeatures gf = chosenWeapon.GetPart<WWA_GunFeatures>();

            if (gf == null)
            {
                return;
            }

            gf.SwitchAutomatic();
        }

        void SelectWeapon(GameObject GO)
        {
            DeselectWeapon();
            if (GO != null && SelectPrimaryWeapon != null) //May be called before object creation event
            {
                chosenWeapon = GO;
                SelectPrimaryWeapon.DisplayName = "Selected - " + chosenWeapon.ShortDisplayName;
                //MessageQueue.AddPlayerMessage(this.chosenWeapon.ShortDisplayName + " selected as primary missile weapon.");
                foreach (GameObject GO2 in activeWeapons)
                {
                    if (GO2 != chosenWeapon)
                        SetFiresManually(false, GO2);
                }
                PartRack parts = chosenWeapon.PartsList;
                foreach (IPart part in parts)
                {
                    if (part.GetType().BaseType.Name == "WWA_Attachment")
                    {
                        WWA_Attachment attachment = part as WWA_Attachment;
                        attachment.OnSelect(ParentObject);
                    }
                }
                if (activeWeapons.Count > 1)
                    DeselectPrimaryWeapon.Enabled = true;
            }
        }

        void DeselectWeapon()
        {
            if (chosenWeapon != null)
            {
                PartRack parts = chosenWeapon.PartsList;
                foreach (IPart part in parts)
                {
                    if (part.GetType().BaseType.Name == "WWA_Attachment")
                    {
                        WWA_Attachment attachment = part as WWA_Attachment;
                        attachment.OnDeselect();
                    }
                }
                chosenWeapon = null;
                SelectPrimaryWeapon.DisplayName = "Select primary missile weapon";
                foreach (GameObject GO2 in activeWeapons)
                    SetFiresManually(true, GO2);
                DeselectPrimaryWeapon.Enabled = false;
            }
        }

        //Won't change weapons that don't fire manually by default like point-defense laser
        public void SetFiresManually(bool b, GameObject GO)
        {
            if (GO != null)
            {
                WWA_GunFeatures gf = GetGunFeaturesPart(GO);
                MissileWeapon mw = GetMissileWeaponPart(GO);

                if (gf != null && mw != null && gf.DefaultFiresManually)
                {
                    mw.FiresManually = b;
                }
            }
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeginEquip")
            {
                GameObject equipped = E.GetParameter("Object") as GameObject;

                if (
                    equipped != null
                    && !equipped.HasTag("IsUnderbarrelWeapon")
                    && IsMissileWeapon(equipped)
                    && !equipped.IsBroken()
                    && equipped.Understood()
                )
                {
                    activeWeapons.Add(equipped);

                    if (activeWeapons.Count == 1)
                        SelectWeapon(equipped);

                    if (chosenWeapon != null && chosenWeapon != equipped)
                        SetFiresManually(false, equipped);
                }
                //In case BeginEquip happens before object is fully generated
                ActivatedAbilities pAA = ParentObject.GetPart<ActivatedAbilities>();
                if (DeselectPrimaryWeapon == null && pAA != null)
                {
                    DeselectPrimaryWeaponID = pAA.AddAbility(
                        "Deselect missile weapon",
                        "CommandDeselectWeapon",
                        "Tactics"
                    );
                    DeselectPrimaryWeapon = pAA.AbilityByGuid[DeselectPrimaryWeaponID];
                }
                DeselectPrimaryWeapon.Enabled = activeWeapons.Count > 1;
                return true;
            }
            if (E.ID == "BeginUnequip")
            {
                BodyPart bodyPart = E.GetParameter("BodyPart") as BodyPart;
                GameObject equipped = bodyPart.Equipped;

                if (
                    equipped != null
                    && !equipped.HasTag("IsUnderbarrelWeapon")
                    && IsMissileWeapon(equipped)
                )
                {
                    SetFiresManually(true, equipped);

                    if (equipped == chosenWeapon)
                        DeselectWeapon();

                    activeWeapons.Remove(equipped);
                    activeWeapons.TrimExcess();

                    if (activeWeapons.Count == 1) //Selecting the only remaining weapon
                        SelectWeapon(activeWeapons[0]);
                }
                return true;
            }
            if (E.ID == "CommandSelectWeapon")
            {
                List<GameObject> activeWeaponsForValidation = activeWeapons;

                foreach (GameObject activeWeapon in activeWeaponsForValidation)
                {
                    if (!GameObject.Validate(activeWeapon))
                    {
                        activeWeapons.Remove(activeWeapon);
                        activeWeapons.TrimExcess();
                    }
                }

                if (activeWeapons.Count == 0)
                {
                    MessageQueue.AddPlayerMessage("You don't have any missile weapons equipped.");
                    return true;
                }
                else if (activeWeapons.Count == 1)
                {
                    if (chosenWeapon == null)
                        SelectWeapon(activeWeapons[0]);
                    else
                        MessageQueue.AddPlayerMessage("You only have one missile weapon equipped.");
                    return true;
                }
                else
                {
                    Dictionary<GameObject, string> names = new Dictionary<GameObject, string>();

                    foreach (GameObject GO in activeWeapons)
                    {
                        if (GetMissileWeaponPart(GO) != null)
                        {
                            names.Add(GO, GO.DisplayName);
                        }
                    }

                    string[] nameValues = names.Values.ToArray();

                    if (nameValues.Length == 1)
                    {
                        SelectWeapon(activeWeapons[0]);

                        return true;
                    }
                    else if (nameValues.Length > 1)
                        SelectWeapon(
                            names.Keys.ElementAt(
                                Popup.PickOption("Choose your weapon", null, "", null, nameValues)
                            )
                        );
                }
                return true;
            }
            if (E.ID == "CommandDeselectWeapon")
            {
                DeselectWeapon();
                return true;
            }
            if (E.ID == "CommandSwitchFireMode")
            {
                SwitchAutomatic();
                return true;
            }
            if (E.ID == "CommandGoProne")
            {
                if (!ParentObject.HasEffect(typeof(WWA_ProneStance)))
                {
                    WWA_ProneStance prone = new WWA_ProneStance(2);

                    if (!ParentObject.IsPlayer())
                    {
                        prone.Duration = 30;
                    }

                    ParentObject.ApplyEffect(prone);
                    ParentObject.UseEnergy(1000, "Physical");

                    if (ParentObject.IsPlayer())
                    {
                        MessageQueue.AddPlayerMessage("You lie down.");
                    }

                    ParentObject.Physics.PlayWorldSound("prone.wav");
                }
                else
                {
                    ParentObject.RemoveEffect(typeof(WWA_ProneStance));
                    ParentObject.UseEnergy(1000, "Physical");

                    if (ParentObject.IsPlayer())
                    {
                        MessageQueue.AddPlayerMessage("You get up.");
                    }
                    ParentObject.Physics.PlayWorldSound("stand.wav");
                }

                return true;
            }

            if (!(E.ID == "ObjectCreated"))
                return true;
            AddAbilities();
            return true;
        }
    }
}
