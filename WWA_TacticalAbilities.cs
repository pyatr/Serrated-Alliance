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
        public int ProneDVPenalty = 15;
        public int IncomingProjectileToHitPenalty = 10;
        public int MinProneDVDistance = 5;

        public bool ProneBonusApplied = false;

        public WWA_TacticalAbilities()
        {
            this.activeWeapons = new List<GameObject>();
        }

        public override void Write(GameObject basis, SerializationWriter Writer)
        {
            Writer.WriteGameObject(this.chosenWeapon);
            Writer.WriteGameObjectList(this.activeWeapons);
            base.Write(basis, Writer);
        }

        public override void Read(GameObject basis, SerializationReader Reader)
        {
            this.chosenWeapon = Reader.ReadGameObject();
            this.activeWeapons = new List<GameObject>();
            Reader.ReadGameObjectList(this.activeWeapons);
            base.Read(basis, Reader);
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent(this, "CommandSelectWeapon");
            Object.RegisterPartEvent(this, "CommandDeselectWeapon");
            Object.RegisterPartEvent(this, "CommandSwitchFireMode");
            Object.RegisterPartEvent(this, "CommandGoProne");
            Object.RegisterPartEvent(this, "BeginTakeAction");
            Object.RegisterPartEvent(this, "ObjectCreated");
            Object.RegisterPartEvent(this, "BeginEquip");
            Object.RegisterPartEvent(this, "BeginUnequip");
            Object.RegisterPartEvent(this, "WeaponGetDefenderDV");
            Object.RegisterPartEvent(this, "FiringMissile");
            Object.RegisterPartEvent(this, "FiredMissileWeapon");
            base.Register(Object, Registrar);
        }

        public void AddAbilities()
        {
            ActivatedAbilities pAA = this.ParentObject.GetPart<ActivatedAbilities>();
            if (pAA != null)
            {
                this.SelectPrimaryWeaponID = pAA.AddAbility(
                    "Select primary missile weapon",
                    "CommandSelectWeapon",
                    "Tactics",
                    "Select a specific missile weapon."
                );
                this.SelectPrimaryWeapon = pAA.AbilityByGuid[this.SelectPrimaryWeaponID];
                this.DeselectPrimaryWeaponID = pAA.AddAbility(
                    "Deselect missile weapon",
                    "CommandDeselectWeapon",
                    "Tactics"
                );
                this.DeselectPrimaryWeapon = pAA.AbilityByGuid[this.DeselectPrimaryWeaponID];
                this.DeselectPrimaryWeapon.Enabled = false;
                this.SwitchFireModeID = pAA.AddAbility(
                    "Switch fire mode",
                    "CommandSwitchFireMode",
                    "Tactics",
                    "Choose between automatic and semi-automatic fire mode. Some weapons only have automatic fire mode."
                );
                this.SwitchFireMode = pAA.AbilityByGuid[this.SwitchFireModeID];
                this.GoProneID = pAA.AddAbility(
                    "Go prone",
                    "CommandGoProne",
                    "Tactics",
                    "Go prone. If your current missile weapon is a firearm or energy rifle or a firearm heavy weapon, you shoot as if your agility was "
                        + (ProneAimBonus * 2).ToString()
                        + " points higher."
                );
                this.GoProne = pAA.AbilityByGuid[this.GoProneID];
            }
        }

        public void SwitchAutomatic()
        {
            if (this.chosenWeapon != null)
            {
                WWA_GunFeatures gf =
                    this.chosenWeapon.GetPart<WWA_GunFeatures>() as WWA_GunFeatures;
                gf.SwitchAutomatic();
            }
        }

        void SelectWeapon(GameObject GO)
        {
            DeselectWeapon();
            if (GO != null && SelectPrimaryWeapon != null) //May be called before object creation event
            {
                this.chosenWeapon = GO;
                this.SelectPrimaryWeapon.DisplayName =
                    "Selected - " + this.chosenWeapon.ShortDisplayName;
                //MessageQueue.AddPlayerMessage(this.chosenWeapon.ShortDisplayName + " selected as primary missile weapon.");
                foreach (GameObject GO2 in this.activeWeapons)
                {
                    if (GO2 != this.chosenWeapon)
                        SetFiresManually(false, GO2);
                }
                PartRack parts = this.chosenWeapon.PartsList;
                foreach (IPart part in parts)
                {
                    if (part.GetType().BaseType.Name == "WWA_Attachment")
                    {
                        WWA_Attachment attachment = part as WWA_Attachment;
                        attachment.OnSelect(this.ParentObject);
                    }
                }
                if (this.activeWeapons.Count > 1)
                    this.DeselectPrimaryWeapon.Enabled = true;
            }
        }

        void DeselectWeapon()
        {
            if (this.chosenWeapon != null)
            {
                PartRack parts = this.chosenWeapon.PartsList;
                foreach (IPart part in parts)
                {
                    if (part.GetType().BaseType.Name == "WWA_Attachment")
                    {
                        WWA_Attachment attachment = part as WWA_Attachment;
                        attachment.OnDeselect();
                    }
                }
                this.chosenWeapon = null;
                this.SelectPrimaryWeapon.DisplayName = "Select primary missile weapon";
                foreach (GameObject GO2 in this.activeWeapons)
                    SetFiresManually(true, GO2);
                this.DeselectPrimaryWeapon.Enabled = false;
            }
        }

        public MissileWeapon GetMissileWeaponPart(GameObject GO)
        {
            return GO.GetPart<MissileWeapon>() as MissileWeapon;
        }

        public bool WeaponFiresManually(GameObject GO)
        {
            if (GO != null)
            {
                GameObject item = GameObject.Create(GO.Blueprint);
                MissileWeapon mw = GetMissileWeaponPart(item);
                if (mw != null)
                    if (mw.FiresManually == false)
                        return false;
                item.Destroy(null, true);
            }
            return true;
        }

        //Won't change weapons that don't fire manually by default like point-defense laser
        public void SetFiresManually(bool b, GameObject GO)
        {
            if (GO != null)
                if (WeaponFiresManually(GO))
                    if (GetMissileWeaponPart(GO) != null)
                        GetMissileWeaponPart(GO).FiresManually = b;
        }

        public bool IsFirearm(GameObject GO)
        {
            if (GO != null)
                if (GO.HasTag("IsFirearm"))
                    return true;
            return false;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeginTakeAction")
            {
                if (
                    (
                        this.ParentObject.HasEffect("Flying")
                        || this.ParentObject.HasEffect("Sprinting")
                    ) && this.ParentObject.HasEffect("WWA_ProneStance")
                )
                {
                    this.ParentObject.RemoveEffect(typeof(WWA_ProneStance));
                    this.ParentObject.UseEnergy(1000, "Physical");
                    if (this.ParentObject.IsPlayer())
                        MessageQueue.AddPlayerMessage("You get up.");
                }
                return true;
            }
            if (E.ID == "BeginEquip")
            {
                GameObject equipped = E.GetParameter("Object") as GameObject;
                if (!equipped.HasTag("IsUnderbarrelWeapon"))
                {
                    if (equipped != null && IsFirearm(equipped))
                    {
                        this.activeWeapons.Add(equipped);
                        if (this.activeWeapons.Count == 1)
                            SelectWeapon(equipped);
                        if (chosenWeapon != null && chosenWeapon != equipped)
                            SetFiresManually(false, equipped);
                    }
                }
                //In case BeginEquip happens before object is fully generated
                ActivatedAbilities pAA = this.ParentObject.GetPart<ActivatedAbilities>();
                if (this.DeselectPrimaryWeapon == null && pAA != null)
                {
                    this.DeselectPrimaryWeaponID = pAA.AddAbility(
                        "Deselect missile weapon",
                        "CommandDeselectWeapon",
                        "Tactics"
                    );
                    this.DeselectPrimaryWeapon = pAA.AbilityByGuid[this.DeselectPrimaryWeaponID];
                }
                this.DeselectPrimaryWeapon.Enabled = this.activeWeapons.Count > 1;
                return true;
            }
            if (E.ID == "BeginUnequip")
            {
                BodyPart bodyPart = E.GetParameter("BodyPart") as BodyPart;
                GameObject equipped = bodyPart.Equipped;
                if (!equipped.HasTag("IsUnderbarrelWeapon"))
                {
                    if (equipped != null && IsFirearm(equipped))
                    {
                        SetFiresManually(true, equipped);
                        if (equipped == this.chosenWeapon)
                            DeselectWeapon();
                        this.activeWeapons.Remove(equipped);
                        this.activeWeapons.TrimExcess();
                        if (this.activeWeapons.Count == 1) //Selecting the only remaining weapon
                            SelectWeapon(this.activeWeapons[0]);
                    }
                }
                return true;
            }
            if (E.ID == "CommandSelectWeapon")
            {
                if (this.activeWeapons.Count == 0)
                {
                    MessageQueue.AddPlayerMessage("You don't have any missile weapons equipped.");
                    return true;
                }
                else if (this.activeWeapons.Count == 1)
                {
                    if (this.chosenWeapon == null)
                        SelectWeapon(this.activeWeapons[0]);
                    else
                        MessageQueue.AddPlayerMessage("You only have one missile weapon equipped.");
                    return true;
                }
                else
                {
                    Dictionary<GameObject, string> names = new Dictionary<GameObject, string>();

                    foreach (GameObject GO in this.activeWeapons)
                    {
                        if (
                            GetMissileWeaponPart(GO) != null /* && GO != this.chosenWeapon*/
                        )
                        {
                            names.Add(GO, GO.DisplayName);
                        }
                    }
                    string[] _names = names.Values.ToArray();
                    if (_names.Length == 1)
                    {
                        SelectWeapon(this.activeWeapons[0]);
                        return true;
                    }
                    else if (_names.Length > 1)
                        SelectWeapon(
                            names.Keys.ElementAt(
                                Popup.PickOption("Choose your weapon", null, "", null, _names)
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
                if (!this.ParentObject.OnWorldMap())
                {
                    if (!this.ParentObject.HasEffect("WWA_ProneStance"))
                    {
                        this.ParentObject.ApplyEffect(new WWA_ProneStance(1));
                        this.ParentObject.UseEnergy(1000, "Physical");
                        if (this.ParentObject.IsPlayer())
                            MessageQueue.AddPlayerMessage("You lie down.");
                    }
                    else
                    {
                        this.ParentObject.RemoveEffect(typeof(WWA_ProneStance));
                        this.ParentObject.UseEnergy(1000, "Physical");
                        if (this.ParentObject.IsPlayer())
                            MessageQueue.AddPlayerMessage("You get up.");
                    }
                }
                return true;
            }
            if (E.ID == "FiringMissile")
            {
                if (!ProneBonusApplied && this.chosenWeapon != null)
                {
                    MissileWeapon mw = GetMissileWeaponPart(chosenWeapon);
                    bool weaponIsPistol = false;
                    if (mw != null)
                        if (mw.Skill == "Pistol")
                            weaponIsPistol = true;
                    if (
                        this.ParentObject.HasEffect("WWA_ProneStance")
                        && this.chosenWeapon.HasPart("MagazineAmmoLoader")
                        && !weaponIsPistol
                    )
                    {
                        //MessageQueue.AddPlayerMessage("Prone accuracy bonus applied to " + this.chosenWeapon.ShortDisplayName + ".");
                        this.ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", 2, true);
                        ProneBonusApplied = true;
                    }
                }
                return true;
            }
            if (E.ID == "FiredMissileWeapon")
            {
                if (ProneBonusApplied)
                {
                    //MessageQueue.AddPlayerMessage("Prone accuracy bonus unapplied to " + this.chosenWeapon.ShortDisplayName + ".");
                    this.ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -2, true);
                    ProneBonusApplied = false;
                }
                return true;
            }
            if (E.ID == "WeaponGetDefenderDV")
            {
                if (this.ParentObject.HasEffect("WWA_ProneStance"))
                {
                    GameObject attackerWeapon = E.GetParameter("Weapon") as GameObject;
                    GameObject attacker = attackerWeapon.Equipped;
                    int dif = attacker.CurrentCell.DistanceTo(this.ParentObject);
                    if (dif > MinProneDVDistance)
                    {
                        //MessageQueue.AddPlayerMessage(this.ParentObject.ShortDisplayName + " is far enough from " + attacker.ShortDisplayName + " to gain DV bonus. " + dif.ToString() + "/" + minProneDVDistance.ToString());
                        E.SetParameter(
                            "Amount",
                            (ProneDVPenalty + IncomingProjectileToHitPenalty) * -1
                        );
                    }
                    else
                    {
                        //MessageQueue.AddPlayerMessage(this.ParentObject.ShortDisplayName + " is too close to " + attacker.ShortDisplayName + " to gain DV bonus." + dif.ToString() + "/" + minProneDVDistance.ToString());
                    }
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
