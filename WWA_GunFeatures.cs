using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using XRL.UI;
using XRL.Messages;
using XRL.World;
using XRL.World.Effects;
using ConsoleLib.Console;
using UnityEngine;
using System.Reflection;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_GunFeatures : IPart
    {
        /*
        scope (+2 to player accuracy, +100 to energy use)
        reflex sight (-100 to energy use)

        bipod (+1 to player accuracy while prone)
        foregrip (-100 to energy use)
        underbarrel grenade launcher
        underbarrel shotgun (4 shells)
        bayonet (short blades)

        muzzle brake (+1 player automatic accuracy) 
        muzzle booster (+1 bullet per attack in automatic mode)
        shotgun choke (+28 to shotgun accuracy)

        extended barrel (+1 weapon accuracy in semi-automatic mode)
        heavy barrel (+1 weapon in automatic mode)
        extended heavy barrel (+1 weapon accuracy in both fire modes)

        laser aiming module (+1 to player accuracy, requires batteries)
        magnetic accelerator (+3 to weapon penetration, requires batteries)
        glowsphere slot

        //Perhaps later
        Ammo types
        standart
        hollow point (+3 to damage, -2 to penetration)
        armor piercing (+2 to penetration, -2 to damage)
        tracer (+2 to player accuracy in automatic mode)
        match (+1 to weapon accuracy)
        subsonic (-1 penetration, for use with silencers)
        enhanced (+1 to penetration, +1 to damage)
        duplex (-4 to damage, double bullet amount)

        Shotguns
        slug
        stun
        fire stream
        flechette (+4 to penetration)
        */

        public bool AutomaticFireMode; //semi-automatic, automatic
        public bool AutomaticOnly;
        public bool DefaultFiresManually = false;
        public Dictionary<string, string[]> AttachmentSlots;
        public Dictionary<string, string> SlotNames;
        public string SingleFireSound, FireBurstSound, FireBurstHighRateSound;
        public int FireRate;
        public int DefaultFireRate;
        public int DefaultAmmoPerShot;
        public int SemiAutoAccuracyBonus = 2;
        public int HighCapacityMagSize = -1;
        public int DrumMagCapacity = -1;

        [NonSerialized]
        private GameObject inventoryViewer;

        public GameObject WeaponFromStack
        {
            get
            {
                GameObject weapon = ParentObject;

                if (weapon.HasPart("Stacker"))
                {
                    weapon = weapon.Stacker.SplitStack(1);
                    weapon ??= ParentObject;
                }

                return weapon;
            }
        }

        public bool PartIsAttachment(IPart part) => part.GetType().BaseType.Name == "WWA_Attachment";
        public bool IsGunAutomatic() => DefaultFireRate > 1;

        public void ModFireRate(int mod)
        {
            FireRate += mod;

            if (AutomaticFireMode)
            {
                MissileWeapon mw = ParentObject.GetPart<MissileWeapon>();

                if (mw != null)
                {
                    mw.ShotsPerAction = DefaultFireRate + FireRate;
                    mw.AmmoPerAction = DefaultAmmoPerShot + FireRate;
                }
            }
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("BeginTakeAction");
            Registrar.Register("ObjectCreated");
            Registrar.Register("WeaponMissleWeaponFiring");
            base.Register(Object, Registrar);
        }

        //Not sure if that's necessary, there's always GamePoolError still has 3 part registrations after unregistering
        public override void ApplyUnregistrar(GameObject Object, bool Active = false)
        {
            Object.UnregisterPartEvent(this, "BeginTakeAction");
            Object.UnregisterPartEvent(this, "ObjectCreated");
            Object.UnregisterPartEvent(this, "WeaponMissleWeaponFiring");
            base.ApplyUnregistrar(Object, Active);
        }

        private List<WWA_Attachment> GetAttachments(GameObject forWeapon)
        {
            PartRack weaponParts = forWeapon.PartsList;
            List<WWA_Attachment> attachments = new List<WWA_Attachment>();
            foreach (IPart part in weaponParts)
            {
                if (PartIsAttachment(part))
                {
                    attachments.Add((WWA_Attachment)part);
                }
            }
            return attachments;
        }

        private List<string> AttachmentListToString(List<WWA_Attachment> alist)
        {
            List<string> atStrList = new List<string>();
            foreach (WWA_Attachment satt in alist)
                atStrList.Add(satt.Name);
            return atStrList;
        }

        public override bool SameAs(IPart p)
        {
            List<WWA_Attachment> currentWeaponAttachments = GetAttachments(ParentObject);
            List<WWA_Attachment> comparedWeaponAttachments = GetAttachments(p.ParentObject);

            //If different attachment count it's not the same and we do not stack those weapons
            if (currentWeaponAttachments.Count != comparedWeaponAttachments.Count)
            {
                return false;
            }
            else
            {
                List<string> cwasl = AttachmentListToString(currentWeaponAttachments);
                List<string> comwasl = AttachmentListToString(comparedWeaponAttachments);

                if (cwasl.All(comwasl.Contains))
                {
                    return true;
                }
            }

            return base.SameAs(p);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != GetShortDescriptionEvent.ID)
                return ID == ZoneBuiltEvent.ID;//I have no idea what that means
            return true;
        }
        public override bool HandleEvent(GetShortDescriptionEvent E)
        {
            if (DefaultAmmoPerShot == 1)
            {
                E.Postfix.AppendRules("Fire mode: semi-automatic\n");
            }
            else if (!AutomaticFireMode)
            {
                E.Postfix.AppendRules($"Fire mode: semi-automatic, +{SemiAutoAccuracyBonus} to player accuracy\n");
            }
            else if (ParentObject.GetTag("AutomaticOnly") == "true")
            {
                E.Postfix.AppendRules("Fire mode: automatic only\n");
            }
            else
            {
                E.Postfix.AppendRules("Fire mode: automatic\n");
            }

            if (AttachmentSlots.Count == SlotNames.Count && AttachmentSlots.Count > 0)
            {
                string description = "Possible attachments: \n";

                foreach (KeyValuePair<string, string[]> attachmentSlot in AttachmentSlots)
                {
                    string displayName = SlotNames[attachmentSlot.Key];
                    description += "\t " + displayName + ": ";
                    string[] attachments = attachmentSlot.Value;

                    for (int i = 0; i < attachments.Length; i++)
                    {
                        description += attachments[i];

                        if (i < attachments.Length - 1)
                        {
                            description += ", ";
                        }
                    }
                    description += '\n';
                }

                E.Postfix.AppendRules(description);
            }
            return true;
        }

        public override bool HandleEvent(GetInventoryActionsEvent E)
        {
            if (AttachmentSlots.Count > 0)
            {
                E.AddAction("Attachments", "attachments", "ViewAttachments", null, 'a', false);
            }

            return true;
        }

        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (E.Command == "ViewAttachments")
                FireEvent(Event.New("ViewAttachments", "Viewer", E.Actor));
            return true;
        }

        public void SwitchAutomatic()
        {
            if (IsGunAutomatic())
            {
                if (!AutomaticOnly)
                {
                    MissileWeapon mw = ParentObject.GetPart<MissileWeapon>();
                    if (mw != null)
                    {
                        AutomaticFireMode = !AutomaticFireMode;
                        if (!AutomaticFireMode)
                        {
                            //TODO: Fix for autoshotgun and DBMG
                            mw.ShotsPerAction = 1;
                            mw.AmmoPerAction = 1;
                            ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", SemiAutoAccuracyBonus, true);
                            MessageQueue.AddPlayerMessage($"Switched {ParentObject.ShortDisplayName} to semi-automatic mode.");
                        }
                        else
                        {
                            if (mw.ShotsPerAction != DefaultFireRate + FireRate && mw.ShotsPerAction > 1)
                                //New fire rate modifier if fire rate was changed by changing ShotsPerAction instead of FireRate
                                FireRate = mw.ShotsPerAction - DefaultFireRate;
                            mw.ShotsPerAction = DefaultFireRate + FireRate;
                            mw.AmmoPerAction = DefaultAmmoPerShot + FireRate;
                            ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -SemiAutoAccuracyBonus, true);
                            MessageQueue.AddPlayerMessage($"Switched {ParentObject.ShortDisplayName} to automatic mode.");
                        }
                    }
                }
                else
                {
                    MessageQueue.AddPlayerMessage(ParentObject.ShortDisplayName + " only has automatic mode.");
                }
            }
        }

        public void InstallAttachment(GameObject weapon, GameObject selectedAttachment, bool noEnergyUse = false, bool Silent = false)
        {
            //Weapon should be retrieved from SplitStack so that part is not added to entire stack

            if (selectedAttachment != null)
            {
                PartRack parts = selectedAttachment.PartsList;
                IPart partToCopy = null;

                foreach (IPart part in parts)
                {
                    if (PartIsAttachment(part))
                    {
                        partToCopy = part;
                        break;
                    }
                }

                if (partToCopy != null)
                {
                    selectedAttachment.RemovePart(partToCopy);
                    WWA_Attachment part = weapon.AddPart(partToCopy) as WWA_Attachment;
                    part.OnInstall();

                    if (!noEnergyUse)
                    {
                        inventoryViewer.UseEnergy(1000, "Physical");
                    }

                    selectedAttachment.Destroy();

                    if (inventoryViewer.IsPlayer() && !Silent)
                    {
                        MessageQueue.AddPlayerMessage($"{part.displayName} attached to {weapon.ShortDisplayName}.");
                    }

                    WWA_TacticalAbilities abilities = GetCharacterAbilities();

                    if (abilities == null || abilities.chosenWeapon == null || weapon == null)
                    {
                        return;
                    }
                    else if (abilities.chosenWeapon == weapon)
                    {
                        part.OnSelect(inventoryViewer);
                        part.OnEquip(inventoryViewer);
                    }
                }
            }
        }

        public void UninstallAttachment(GameObject weapon, WWA_Attachment attachment, bool useEnergy = true, bool Silent = false)
        {
            if (!attachment.integral)
            {
                string name = attachment.Name;
                string blueprintName = attachment.AttachmentBlueprintName;

                if (weapon.Equipped != null)
                {
                    attachment.OnUnequip(inventoryViewer);
                }

                attachment.OnDeselect();
                attachment.OnUninstall();
                weapon.RemovePart(attachment);

                GameObject uninstalled = GameObject.Create(blueprintName);

                if (inventoryViewer.IsPlayer() && !Silent)
                {
                    MessageQueue.AddPlayerMessage($"Detached {uninstalled.ShortDisplayName} from {weapon.ShortDisplayName}.");
                }

                //Can't remember why FlushTransient is null
                if (uninstalled != null)
                {
                    inventoryViewer.Inventory.AddObject(uninstalled, null, false, false, false);
                }

                if (useEnergy)
                {
                    inventoryViewer.UseEnergy(1000, "Physical");
                }
            }
            else if (inventoryViewer.IsPlayer() && !Silent)
            {
                MessageQueue.AddPlayerMessage("You can't remove integral attachments.");
            }
        }

        public void UninstallAttachmentFromSlot(GameObject weapon, string slot)
        {
            PartRack parts = weapon.PartsList;
            foreach (IPart part in parts)
            {
                if (PartIsAttachment(part))
                {
                    WWA_Attachment attachment = part as WWA_Attachment;

                    if (AttachmentFitsInSlot(attachment.displayName, slot))
                    {
                        UninstallAttachment(weapon, attachment);
                        break;
                    }
                }
            }
        }

        public void UninstallAttachmentByName(GameObject weapon, string attachmentName)
        {
            PartRack parts = weapon.PartsList;
            foreach (IPart part in parts)
            {
                if (PartIsAttachment(part))
                {
                    WWA_Attachment attachment = part as WWA_Attachment;
                    if (attachment.displayName == attachmentName)
                    {
                        UninstallAttachment(weapon, attachment);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Uninstall all non-integral attachments from weapon
        /// </summary>
        public void UninstallAllAttachments(GameObject weapon)
        {
            int uninstalledAttachmentCount = 0;
            string slot;

            while (WeaponHasAttachment(out slot, weapon, true))
            {
                if (slot != "none")
                {
                    UninstallAttachmentFromSlot(weapon, slot);
                    uninstalledAttachmentCount++;
                }
            }

            MessageQueue.AddPlayerMessage($"Uninstalled ${uninstalledAttachmentCount} attachments from {weapon.ShortDisplayName}.");
        }

        /// <summary>
        /// Does weapon have an attachment in slot?
        /// </summary>
        /// <param name="slot">Slot name</param>
        /// <param name="removeableOnly">Can attachment be removed from weapon or is it integral?</param>
        /// <returns></returns>
        public bool WeaponHasAttachment(out string slot, GameObject weapon, bool removeableOnly = false)
        {
            slot = "none";
            PartRack parts = weapon.PartsList;
            foreach (IPart part in parts)
            {
                if (PartIsAttachment(part))
                {
                    WWA_Attachment attachment = part as WWA_Attachment;
                    foreach (KeyValuePair<string, string[]> kvp in AttachmentSlots)
                        if (kvp.Value.Contains(attachment.displayName))
                            slot = kvp.Key;
                    if (removeableOnly)
                    {
                        if (attachment.integral)
                            continue;
                        else
                            return true;
                    }
                    else
                        return true;
                }
            }
            return false;
        }

        public Dictionary<string, GameObject> FindAttachmentsForSlot(string slot, List<string> possibleAttachments)
        {
            Dictionary<string, GameObject> attachments = new Dictionary<string, GameObject>();
            List<GameObject> inventory = inventoryViewer.GetInventory();
            foreach (GameObject GO in inventory)
            {
                if (GO.Physics.Category == "Weapon Attachments")
                {
                    PartRack parts = GO.PartsList;
                    foreach (IPart part in parts)
                    {
                        if (PartIsAttachment(part))
                        {
                            WWA_Attachment possibleAttachment = part as WWA_Attachment;
                            if (AttachmentFitsInSlot(possibleAttachment.displayName, slot) && possibleAttachments.Contains(possibleAttachment.displayName))
                                attachments.Add(possibleAttachment.displayName, GO);
                        }
                    }
                }
            }
            return attachments;
        }

        public WWA_TacticalAbilities GetCharacterAbilities()
        {
            if (ParentObject.Equipped != null)
                if (ParentObject.Equipped.HasPart("WWA_TacticalAbilities"))
                    return ParentObject.Equipped.GetPart<WWA_TacticalAbilities>();
                else if (inventoryViewer != null)
                    if (inventoryViewer.HasPart("WWA_TacticalAbilities"))
                        return inventoryViewer.GetPart<WWA_TacticalAbilities>();

            return null;
        }

        public bool AttachmentFitsInSlot(string attachment, string slot)
        {
            foreach (KeyValuePair<string, string[]> kvp in AttachmentSlots)
                foreach (string s in kvp.Value)
                    if (slot == kvp.Key && s == attachment)
                        return true;
            return false;
        }

        public void PlayAttackSound()
        {
            //Single shot weapons use default MissileFireSound sound
            if (DefaultFireRate > 1)
            {
                if (!AutomaticFireMode)
                    ParentObject.Equipped.Physics.PlayWorldSound(SingleFireSound, 0.5f, 0.0f, true, null);
                else if (FireRate < 1)
                    ParentObject.Equipped.Physics.PlayWorldSound(FireBurstSound, 0.5f, 0.0f, true, null);
                else
                    ParentObject.Equipped.Physics.PlayWorldSound(FireBurstHighRateSound, 0.5f, 0.0f, true, null);

            }
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WeaponMissleWeaponFiring")
            {
                PlayAttackSound();
                return true;
            }
            if (E.ID == "BeginTakeAction")
            {
                if (!AutomaticFireMode && DefaultFireRate == 1)
                {
                    //If in semi-automode fire rate was increased the increase will go to fire rate instead
                    MissileWeapon mw = ParentObject.GetPart<MissileWeapon>();
                    if (mw.ShotsPerAction > 1)
                        FireRate = mw.ShotsPerAction - 1;
                }
                return true;
            }
            if (E.ID == "ViewAttachments")
            {
                if (AttachmentSlots.Count != SlotNames.Count || AttachmentSlots.Count == 0)
                {
                    return true;
                }

                GameObject weapon = WeaponFromStack;

                inventoryViewer = E.GetParameter("Viewer") as GameObject;
                List<string> slotsAndAttachmentsMenu = new List<string>();
                Dictionary<string, bool> isSlotOccupied = new Dictionary<string, bool>();

                foreach (string slot in AttachmentSlots.Keys)
                {
                    PartRack weaponParts = weapon.PartsList;
                    string slotDisplayName = SlotNames[slot];
                    bool occupied = false;
                    slotsAndAttachmentsMenu.Add($"{slotDisplayName}: &knone");

                    foreach (IPart part in weaponParts)
                    {
                        if (!PartIsAttachment(part))
                        {
                            continue;
                        }

                        WWA_Attachment possibleInstalledAttachment = part as WWA_Attachment;
                        string color = "";

                        if (!AttachmentFitsInSlot(possibleInstalledAttachment.displayName, slot))
                        {
                            continue;
                        }

                        if (possibleInstalledAttachment.integral)
                        {
                            color = "&Y";
                        }

                        slotsAndAttachmentsMenu[slotsAndAttachmentsMenu.Count - 1] = slotDisplayName + ": " + color + possibleInstalledAttachment.displayName;
                        occupied = true;
                        break;
                    }

                    isSlotOccupied.Add(slot, occupied);
                }

                if (isSlotOccupied.ContainsValue(true))
                {
                    slotsAndAttachmentsMenu.Add("Remove all attachments");
                }

                slotsAndAttachmentsMenu.Add("Cancel");
                int selectedSlotNumber = -1;

                do
                {
                    selectedSlotNumber = Popup.PickOption("Attachments", null, "", null, slotsAndAttachmentsMenu.ToArray());

                    switch (slotsAndAttachmentsMenu[selectedSlotNumber])
                    {
                        case "Remove all attachments":
                            UninstallAllAttachments(weapon);
                            break;
                        case "Cancel":
                            break;
                        default:
                            string selectedSlot = AttachmentSlots.Keys.ElementAt(selectedSlotNumber);
                            string fullSlotName = SlotNames[selectedSlot];
                            List<string> possibleAttachments = AttachmentSlots[selectedSlot].ToList();

                            if (selectedSlot == "Cancel")
                            {
                                break;
                            }

                            Dictionary<string, GameObject> attachments = FindAttachmentsForSlot(selectedSlot, possibleAttachments);
                            GameObject selectedAttachment = null;
                            List<string> options = new List<string>();
                            options.AddRange(attachments.Keys.ToList());
                            if (isSlotOccupied[selectedSlot])
                                options.Add("Remove attachment");
                            options.Add("Cancel");
                            string[] names = options.ToArray();
                            if (attachments.Count > 0 || isSlotOccupied[selectedSlot])
                            {
                                selectedSlotNumber = Popup.PickOption("Choose attachment", null, "", null, names);

                                switch (names[selectedSlotNumber])
                                {
                                    case "Remove attachment": UninstallAttachmentFromSlot(weapon, selectedSlot); break;
                                    case "Cancel": selectedSlotNumber = -1; break;
                                    default:
                                        {
                                            UninstallAttachmentFromSlot(weapon, selectedSlot);
                                            selectedAttachment = attachments.Values.ElementAt(selectedSlotNumber);
                                            InstallAttachment(weapon, selectedAttachment);
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                MessageQueue.AddPlayerMessage($"You don't have any attachments to install on {weapon.ShortDisplayName} {fullSlotName}.");
                            }

                            break;
                    }
                }
                while (selectedSlotNumber == -1);

                return true;
            }
            if (!(E.ID == "ObjectCreated"))
                return base.FireEvent(E);

            FireRate = 0;
            GameObjectBlueprint blueprint = ParentObject.GetBlueprint();
            Dictionary<string, string> blueprintTags = blueprint.Tags;
            AttachmentSlots = new Dictionary<string, string[]>();
            SlotNames = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kvp in blueprintTags)
            {
                if (!kvp.Key.StartsWith("AttachmentSlot"))
                {
                    continue;
                }

                string[] newSlot = kvp.Key.Split(':');
                string slotType = newSlot[1];
                string slotDisplayName = newSlot[2];

                if (kvp.Value != "")
                {
                    AttachmentSlots.Add(slotType, kvp.Value.Split(','));
                    SlotNames.Add(slotType, slotDisplayName);
                }
                else
                {
                    AttachmentSlots.Remove(slotType);
                    SlotNames.Remove(slotType);
                }
            }

            AutomaticOnly = false;
            if (ParentObject.GetTag("AutomaticOnly") == "true")
                AutomaticOnly = true;
            if (ParentObject.GetTag("MissileFireSound") == "none")
            {
                //TODO: Use groups of various fire sounds for all shots
                SingleFireSound = ParentObject.GetTag("FireSoundSingle");
                FireBurstSound = ParentObject.GetTag("FireBurstSound");
                FireBurstHighRateSound = ParentObject.GetTag("FireBurstHighRateSound");
            }
            MagazineAmmoLoader mal = ParentObject.GetPart<MagazineAmmoLoader>();
            if (mal != null)
            {
                if (ParentObject.HasIntProperty("ExtendedMagCapacity"))
                {
                    HighCapacityMagSize = ParentObject.GetIntProperty("ExtendedMagCapacity");
                    //if (HighCapacityMagSize == -1)
                    //    HighCapacityMagSize = (int)(mal.MaxAmmo * 1.5f);
                    //MessageQueue.AddPlayerMessage(ParentObject.ShortDisplayName + ": " + HighCapacityMagSize.ToString());
                }
                if (ParentObject.HasIntProperty("DrumMagCapacity"))
                {
                    DrumMagCapacity = ParentObject.GetIntProperty("DrumMagCapacity");
                    //if (DrumMagCapacity == -1)
                    //    DrumMagCapacity = (int)(mal.MaxAmmo * 3.0f);
                    //MessageQueue.AddPlayerMessage(ParentObject.ShortDisplayName + ": " + DrumMagCapacity.ToString());
                }
            }
            MissileWeapon mw2 = ParentObject.GetPart<MissileWeapon>();
            if (mw2 != null)
            {
                DefaultFireRate = mw2.ShotsPerAction;
                DefaultAmmoPerShot = mw2.AmmoPerAction;
                DefaultFiresManually = mw2.FiresManually;
                AutomaticFireMode = DefaultFireRate != 1;
            }
            return true;
        }
    }
}