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

        public override void Write(GameObject basis, SerializationWriter Writer)
        {
            Writer.WriteGameObject(weaponObject);
            base.Write(basis, Writer);
        }

        public override void Read(GameObject basis, SerializationReader Reader)
        {
            weaponObject = Reader.ReadGameObject(null);
            base.Read(basis, Reader);
        }

        public string ManagerID
        {
            get
            {
                return ParentObject.ID + "::" + ParentObject.ShortDisplayName;
            }
        }

        public WWA_UBWeapon()
        {
            worksOnSelect = true;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            base.Register(Object, Registrar);
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
                            null,
                            null,
                            null,
                            null,
                            ManagerID,
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
                            null,
                            (List<GameObject>)null
                        );
                        mal.SetAmmo(null);
                    }
                    weaponObject.Destroy(null, true);
                    unequipper.RemoveBodyPartsByManager(ManagerID);
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
                            null,
                            null,
                            null,
                            null,
                            ManagerID,
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
                        selector.RegisterPartEvent(this, "CommandFireUBWeapon");
                        FireUBWeaponAbilityID = pAA.AddAbility(
                            "Fire " + weaponObject.ShortDisplayName,
                            "CommandFireUBWeapon",
                            "Tactics",
                            "Fire "
                                + weaponObject.ShortDisplayName
                                + " of "
                                + ParentObject.ShortDisplayName
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

                        FireUBWeaponAbility = pAA.AbilityByGuid[FireUBWeaponAbilityID];
                    }
                }
            }
            return base.OnSelect(selector);
        }

        public override bool OnDeselect()
        {
            if (!AddOnEquip && ParentObject.Equipped != null)
            {
                MagazineAmmoLoader mal = weaponObject.GetPart("MagazineAmmoLoader") as MagazineAmmoLoader;
                if (mal != null)
                {
                    ParentObject.Equipped.TakeObject(
                        mal.Ammo,
                        false,
                        true,
                        new int?(0),
                        null,
                        (List<GameObject>)null
                    );
                    mal.SetAmmo(null);
                }
                weaponObject.Destroy(null, true);
                ParentObject.Equipped.RemoveBodyPartsByManager(ManagerID);
                ParentObject.Equipped.UnregisterPartEvent(this, "CommandFireUBWeapon");

                if (FireUBWeaponAbilityID != Guid.Empty)
                {
                    ActivatedAbilities pAA = ParentObject.Equipped.GetPart<ActivatedAbilities>();
                    pAA.RemoveAbility(FireUBWeaponAbilityID);
                    FireUBWeaponAbilityID = Guid.Empty;
                }
            }
            return base.OnDeselect();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandFireUBWeapon")
            {
                MagazineAmmoLoader mal = weaponObject.GetPart("MagazineAmmoLoader") as MagazineAmmoLoader;
                if (mal.Ammo != null)
                {
                    Combat combat = ParentObject.Equipped.GetPart("Combat") as Combat;
                    MissileWeapon mw = ParentObject.GetPart("MissileWeapon") as MissileWeapon;
                    MissileWeapon UBWmw = weaponObject.GetPart("MissileWeapon") as MissileWeapon;
                    combat.LastFired = mw;
                    string cachedSkill = UBWmw.Skill;
                    UBWmw.FiresManually = true;
                    UBWmw.Skill = "Underbarrel";
                    Combat.FireMissileWeapon(The.Player, null, null, FireType.Normal, "Underbarrel");
                    UBWmw.FiresManually = false;
                    UBWmw.Skill = cachedSkill;
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
