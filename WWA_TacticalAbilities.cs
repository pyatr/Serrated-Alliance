using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using XRL.Messages;
using XRL.UI;
using XRL.World.Effects;
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
        public ActivatedAbilityEntry SelectPrimaryWeapon;
        public Guid DeselectPrimaryWeaponID = Guid.Empty;
        public ActivatedAbilityEntry DeselectPrimaryWeapon;
        public Guid SwitchFireModeID = Guid.Empty;
        public ActivatedAbilityEntry SwitchFireMode;
        public Guid GoProneID = Guid.Empty;
        public ActivatedAbilityEntry GoProne;

        public int proneAimBonus = 1;
        public int DVPenalty = 15;
        public int IncomingProjectileToHitPenalty = 10;
        public int minProneDVDistance = 5;

        public bool ProneBonusApplied = false;

        public WWA_TacticalAbilities()
        {
            this.activeWeapons = new List<GameObject>();
        }

        public override bool SameAs(IPart p)
        {
            return true;
        }

        public override void SaveData(SerializationWriter Writer)
        {
            Writer.WriteGameObject(this.chosenWeapon);
            Writer.WriteGameObjectList(this.activeWeapons);
            base.SaveData(Writer);
        }

        public override void LoadData(SerializationReader Reader)
        {
            this.chosenWeapon = Reader.ReadGameObject((string)null);
            this.activeWeapons = new List<GameObject>();
            Reader.ReadGameObjectList(this.activeWeapons, null);
            base.LoadData(Reader);
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "CommandSelectWeapon");
            Object.RegisterPartEvent((IPart)this, "CommandDeselectWeapon");
            Object.RegisterPartEvent((IPart)this, "CommandSwitchFireMode");
            Object.RegisterPartEvent((IPart)this, "CommandGoProne");
            Object.RegisterPartEvent((IPart)this, "BeginTakeAction");
            Object.RegisterPartEvent((IPart)this, "ObjectCreated");
            Object.RegisterPartEvent((IPart)this, "BeginEquip");
            Object.RegisterPartEvent((IPart)this, "BeginUnequip");
            Object.RegisterPartEvent((IPart)this, "WeaponGetDefenderDV");
            Object.RegisterPartEvent((IPart)this, "FiringMissile");
            Object.RegisterPartEvent((IPart)this, "FiredMissileWeapon");
            base.Register(Object);
        }

        public void AddAbilities()
        {
            ActivatedAbilities pAA = this.ParentObject.GetPart<ActivatedAbilities>();
            if (pAA != null)
            {
                this.SelectPrimaryWeaponID = pAA.AddAbility("Select primary missile weapon", "CommandSelectWeapon", "Tactics", "Select a specific missile weapon.");
                this.SelectPrimaryWeapon = pAA.AbilityByGuid[this.SelectPrimaryWeaponID];
                this.DeselectPrimaryWeaponID = pAA.AddAbility("Deselect missile weapon", "CommandDeselectWeapon", "Tactics");
                this.DeselectPrimaryWeapon = pAA.AbilityByGuid[this.DeselectPrimaryWeaponID];
                this.DeselectPrimaryWeapon.Enabled = false;
                this.SwitchFireModeID = pAA.AddAbility("Switch fire mode", "CommandSwitchFireMode", "Tactics", "Choose between automatic and semi-automatic fire mode. Some weapons only have automatic fire mode.");
                this.SwitchFireMode = pAA.AbilityByGuid[this.SwitchFireModeID];
                this.GoProneID = pAA.AddAbility("Go prone", "CommandGoProne", "Tactics", "Go prone. If your current missile weapon is a firearm or energy rifle or a firearm heavy weapon, you shoot as if your agility was " + (proneAimBonus * 2).ToString() + " points higher.");
                this.GoProne = pAA.AbilityByGuid[this.GoProneID];
            }
        }

        public void SwitchAutomatic()
        {
            if (this.chosenWeapon != null)
            {
                WWA_GunFeatures gf = this.chosenWeapon.GetPart<WWA_GunFeatures>() as WWA_GunFeatures;
                gf.SwitchAutomatic();
            }
        }

        void SelectWeapon(GameObject GO)
        {
            DeselectWeapon();
            if (GO != null && SelectPrimaryWeapon != null)//May be called before object creation event
            {
                this.chosenWeapon = GO;
                this.SelectPrimaryWeapon.DisplayName = "Selected - " + this.chosenWeapon.ShortDisplayName;
                //MessageQueue.AddPlayerMessage(this.chosenWeapon.ShortDisplayName + " selected as primary missile weapon.");
                foreach (GameObject GO2 in this.activeWeapons)
                {
                    if (GO2 != this.chosenWeapon)
                        SetFiresManually(false, GO2);
                }
                List<IPart> parts = this.chosenWeapon.PartsList;
                foreach (IPart part in parts)
                {
                    if (part.GetType().BaseType.Name == "WWA_Attachment")
                    {
                        WWA_Attachment attachment = part as WWA_Attachment;
                        attachment.OnSelect(this.ParentObject);
                    }
                }
                this.DeselectPrimaryWeapon.Enabled = true;
            }
        }

        void DeselectWeapon()
        {
            if (this.chosenWeapon != null)
            {
                List<IPart> parts = this.chosenWeapon.PartsList;
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
                GameObject item = GameObject.create(GO.Blueprint);
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
                if ((this.ParentObject.HasEffect("Flying") || this.ParentObject.HasEffect("Sprinting")) && this.ParentObject.HasEffect("WWA_ProneStance"))
                {
                    this.ParentObject.RemoveEffect("WWA_ProneStance");
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
                        if (this.activeWeapons.Count == 1)//Selecting the only remaining weapon                        
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
                        if (GetMissileWeaponPart(GO) != null/* && GO != this.chosenWeapon*/)
                            names.Add(GO, GO.DisplayName);
                    string[] _names = names.Values.ToArray();
                    if (_names.Length == 1)
                    {
                        SelectWeapon(this.activeWeapons[0]);
                        return true;
                    }
                    else if (_names.Length > 1)
                        SelectWeapon(names.Keys.ElementAt(Popup.ShowOptionList("Choose your weapon", _names)));
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
                        this.ParentObject.RemoveEffect("WWA_ProneStance");
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
                    if (this.ParentObject.HasEffect("WWA_ProneStance") && this.chosenWeapon.HasPart("MagazineAmmoLoader") && !weaponIsPistol)
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
                    if (dif > minProneDVDistance)
                    {
                        //MessageQueue.AddPlayerMessage(this.ParentObject.ShortDisplayName + " is far enough from " + attacker.ShortDisplayName + " to gain DV bonus. " + dif.ToString() + "/" + minProneDVDistance.ToString());
                        E.SetParameter("Amount", (DVPenalty + IncomingProjectileToHitPenalty) * -1);
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