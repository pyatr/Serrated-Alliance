using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.World.Anatomy;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_UBWeapon : WWA_Attachment
    {
        public Guid FireUBWeaponAbilityID = Guid.Empty;
        public ActivatedAbilityEntry FireUBWeaponAbility;

        [NonSerialized]
        public GameObject weaponObject = null;
        public string WeaponBlueprintName;
        public string Description;

        //OnEquip does not work properly here, do not change
        public bool AddOnEquip = false;

        public override void SaveData(SerializationWriter Writer)
        {
            Writer.WriteGameObject(this.weaponObject);
            base.SaveData(Writer);
        }

        public override void LoadData(SerializationReader Reader)
        {
            this.weaponObject = Reader.ReadGameObject((string)null);
            base.LoadData(Reader);
        }

        public string ManagerID
        {
            get { return this.ParentObject.ID + "::" + this.ParentObject.ShortDisplayName; }
        }

        public WWA_UBWeapon()
        {
            worksOnSelect = true;
        }

        public override void Register(GameObject Object)
        {
            base.Register(Object);
        }

        public override string GetDescription()
        {
            string s = "";
            if (installed)
                s = displayName + ": " + Description;
            return s + base.GetDescription();
        }

        public override bool OnEquip(GameObject equipper)
        {
            if (AddOnEquip)
            {
                if (equipper != null)
                {
                    BodyPart body = equipper.Body.GetBody();
                    ActivatedAbilities pAA = equipper.GetPart<ActivatedAbilities>();
                    if (body != null && pAA != null)
                    {
                        weaponObject = GameObject.Create(WeaponBlueprintName);
                        weaponObject.AddPart(new Cursed());
                        MissileWeapon UBWmw =
                            weaponObject.GetPart("MissileWeapon") as MissileWeapon;
                        UBWmw.FiresManually = true;
                        BodyPart UBslot = null;
                        string type = "Underbarrel Weapon";
                        //This is atrocious
                        UBslot = body.AddPartAt(
                            type,
                            0,
                            (string)null,
                            (string)null,
                            (string)null,
                            (string)null,
                            this.ManagerID,
                            new int?(),
                            new int?(),
                            new int?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            "Missile Weapon",
                            new string[3] { "Hands", "Feet", "Thrown Weapon" },
                            true
                        );

                        equipper.ForceEquipObject(weaponObject, UBslot, true);
                        WWA_TacticalAbilities ta = equipper.GetPart<WWA_TacticalAbilities>();
                        ta.activeWeapons.Add(weaponObject);
                    }
                }
            }
            return base.OnEquip(equipper);
        }

        public override bool OnUnequip(GameObject unequipper)
        {
            if (AddOnEquip)
            {
                if (unequipper != null)
                {
                    MagazineAmmoLoader mal =
                        weaponObject.GetPart("MagazineAmmoLoader") as MagazineAmmoLoader;
                    if (mal != null)
                    {
                        unequipper.TakeObject(
                            mal.Ammo,
                            false,
                            true,
                            new int?(0),
                            (string)null,
                            (List<GameObject>)null
                        );
                        mal.SetAmmo((GameObject)null);
                    }
                    weaponObject.Destroy(null, true);
                    unequipper.RemoveBodyPartsByManager(this.ManagerID);
                }
            }
            return base.OnUnequip(unequipper);
        }

        public override bool OnSelect(GameObject selector)
        {
            if (!AddOnEquip)
            {
                if (selector != null)
                {
                    BodyPart body = selector.Body.GetBody();
                    ActivatedAbilities pAA = selector.GetPart<ActivatedAbilities>();
                    if (body != null && pAA != null)
                    {
                        weaponObject = GameObject.Create(WeaponBlueprintName);
                        weaponObject.AddPart(new Cursed());
                        BodyPart UBslot = null;
                        string type = "Underbarrel Weapon";
                        UBslot = body.AddPartAt(
                            type,
                            0,
                            (string)null,
                            (string)null,
                            (string)null,
                            (string)null,
                            this.ManagerID,
                            new int?(),
                            new int?(),
                            new int?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            new bool?(),
                            "Missile Weapon",
                            new string[3] { "Hands", "Feet", "Thrown Weapon" },
                            true
                        );
                        selector.ForceEquipObject(weaponObject, UBslot, true);
                        selector.RegisterPartEvent((IPart)this, "CommandFireUBWeapon");
                        this.FireUBWeaponAbilityID = pAA.AddAbility(
                            "Fire " + weaponObject.ShortDisplayName,
                            "CommandFireUBWeapon",
                            "Tactics",
                            "Fire "
                                + weaponObject.ShortDisplayName
                                + " of "
                                + this.ParentObject.ShortDisplayName
                                + ".",
                            "\a",
                            null,
                            false,
                            true,
                            false,
                            false,
                            false,
                            true
                        );
                        this.FireUBWeaponAbility = pAA.AbilityByGuid[this.FireUBWeaponAbilityID];
                    }
                }
            }
            return base.OnSelect(selector);
        }

        public override bool OnDeselect()
        {
            if (!AddOnEquip)
            {
                MagazineAmmoLoader mal =
                    weaponObject.GetPart("MagazineAmmoLoader") as MagazineAmmoLoader;
                if (mal != null)
                {
                    this.ParentObject.Equipped.TakeObject(
                        mal.Ammo,
                        false,
                        true,
                        new int?(0),
                        (string)null,
                        (List<GameObject>)null
                    );
                    mal.SetAmmo((GameObject)null);
                }
                weaponObject.Destroy(null, true);
                this.ParentObject.Equipped.RemoveBodyPartsByManager(this.ManagerID);
                this.ParentObject.Equipped.UnregisterPartEvent((IPart)this, "CommandFireUBWeapon");
                if (this.FireUBWeaponAbilityID != Guid.Empty)
                {
                    ActivatedAbilities pAA =
                        this.ParentObject.Equipped.GetPart<ActivatedAbilities>();
                    pAA.RemoveAbility(this.FireUBWeaponAbilityID);
                    this.FireUBWeaponAbilityID = Guid.Empty;
                }
            }
            return base.OnDeselect();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandFireUBWeapon")
            {
                MagazineAmmoLoader mal =
                    weaponObject.GetPart("MagazineAmmoLoader") as MagazineAmmoLoader;
                if (mal.Ammo != null)
                {
                    Combat combat = this.ParentObject.Equipped.GetPart("Combat") as Combat;
                    MissileWeapon mw = this.ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                    MissileWeapon UBWmw = weaponObject.GetPart("MissileWeapon") as MissileWeapon;
                    combat.LastFired = mw;
                    UBWmw.FiresManually = true;
                    this.ParentObject.Equipped.FireEvent("CommandFireMissileWeapon");
                    UBWmw.FiresManually = false;
                }
                else
                {
                    MessageQueue.AddPlayerMessage(weaponObject.ShortDisplayName + " has no ammo!");
                }
                return true;
            }
            return base.FireEvent(E);
        }
    }
}
