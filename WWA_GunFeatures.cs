using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using XRL.UI;
using XRL.Messages;
using XRL.World;
using XRL.World.Effects;
using ConsoleLib.Console;

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

        public bool FireMode; //semi-automatic, automatic
        public bool AutomaticOnly;
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

        public void ModFireRate(int mod)
        {
            FireRate += mod;
            if (FireMode)
            {
                MissileWeapon mw = this.ParentObject.GetPart<MissileWeapon>() as MissileWeapon;
                if (mw != null)
                {
                    mw.ShotsPerAction = DefaultFireRate + FireRate;
                    mw.AmmoPerAction = DefaultAmmoPerShot + FireRate;
                }
            }
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "BeginTakeAction");
            Object.RegisterPartEvent((IPart)this, "ObjectCreated");
            Object.RegisterPartEvent((IPart)this, "WeaponMissleWeaponFiring");
            base.Register(Object);
        }

        public override bool SameAs(IPart p)
        {
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
            if (!FireMode)
            {
                if (DefaultFireRate > 1)
                    E.Postfix.AppendRules($"Fire mode: semi-automatic, +{SemiAutoAccuracyBonus} to player accuracy\n");
                else
                    E.Postfix.AppendRules("Fire mode: semi-automatic\n");
            }
            else
            {
                if (this.ParentObject.GetTag("AutomaticOnly") == "false")
                    E.Postfix.AppendRules("Fire mode: automatic\n");
                else
                    E.Postfix.AppendRules("Fire mode: automatic only\n");
            }
            if (AttachmentSlots.Count == SlotNames.Count && AttachmentSlots.Count > 0)
            {
                string description = "Possible attachments: \n";
                foreach (KeyValuePair<string, string[]> attachmentSlot in AttachmentSlots)
                {
                    string name = "", displayName = "";
                    name = attachmentSlot.Key;
                    displayName = SlotNames[name];
                    description += "\t " + displayName + ": ";
                    string[] attachments = attachmentSlot.Value;
                    for (int i = 0; i < attachments.Length; i++)
                    {
                        description += attachments[i];
                        if (i < attachments.Length - 1)
                            description += ", ";
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
                E.AddAction("Attachments", "attachments", "ViewAttachments", null, 'a', false);
            return true;
        }

        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (E.Command == "ViewAttachments")
                this.FireEvent(Event.New("ViewAttachments", "Viewer", (object)E.Actor));
            return true;
        }

        public bool IsGunAutomatic() => DefaultFireRate > 1;

        public void SwitchAutomatic()
        {
            if (IsGunAutomatic())
            {
                if (!AutomaticOnly)
                {
                    MissileWeapon mw = this.ParentObject.GetPart<MissileWeapon>() as MissileWeapon;
                    if (mw != null)
                    {
                        this.FireMode = !this.FireMode;
                        if (!this.FireMode)
                        {
                            mw.ShotsPerAction = 1;
                            mw.AmmoPerAction = 1;
                            this.ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", SemiAutoAccuracyBonus, true);
                            MessageQueue.AddPlayerMessage($"Switched {this.ParentObject.ShortDisplayName} to semi-automatic mode.");
                        }
                        else
                        {
                            if (mw.ShotsPerAction != DefaultFireRate + FireRate && mw.ShotsPerAction > 1)
                                //New fire rate modifier if fire rate was changed by changing ShotsPerAction instead of FireRate
                                FireRate = mw.ShotsPerAction - DefaultFireRate;
                            mw.ShotsPerAction = DefaultFireRate + FireRate;
                            mw.AmmoPerAction = DefaultAmmoPerShot + FireRate;
                            this.ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -SemiAutoAccuracyBonus, true);
                            MessageQueue.AddPlayerMessage($"Switched {this.ParentObject.ShortDisplayName} to automatic mode.");
                        }
                    }
                }
                else
                {
                    MessageQueue.AddPlayerMessage(this.ParentObject.ShortDisplayName + " only has automatic mode.");
                }
            }
        }

        public void InstallAttachment(GameObject selectedAttachment, bool noEnergyUse = false, bool Silent = false)
        {
            if (selectedAttachment != null)
            {
                List<IPart> parts = selectedAttachment.PartsList;
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
                    this.ParentObject.AddPart(partToCopy);
                    WWA_Attachment part = this.ParentObject.GetPart(partToCopy.Name) as WWA_Attachment;
                    part.OnInstall();
                    if (!noEnergyUse)
                        inventoryViewer.UseEnergy(1000, "Physical");
                    selectedAttachment.Destroy();
                    if (inventoryViewer.IsPlayer() && !Silent)
                        MessageQueue.AddPlayerMessage($"{part.displayName} installed on {this.ParentObject.ShortDisplayName}.");
                    if (this.GetCharacterAbilities().chosenWeapon == null || this.ParentObject == null)
                        return;
                    if (this.GetCharacterAbilities().chosenWeapon == this.ParentObject)
                    {
                        part.OnSelect(inventoryViewer);
                        part.OnEquip(inventoryViewer);
                    }
                }
            }
        }

        public void UninstallAttachment(WWA_Attachment attachment, bool noEnergyUse = false, bool Silent = false)
        {
            if (!attachment.integral)
            {
                string name = attachment.Name;
                string blueprintName = attachment.AttachmentBlueprintName;
                if (this.ParentObject.Equipped != null)
                    attachment.OnUnequip(inventoryViewer);
                attachment.OnDeselect();
                attachment.OnUninstall();
                this.ParentObject.RemovePart(name);
                GameObject uninstalled = GameObject.create(blueprintName);
                if (inventoryViewer.IsPlayer())
                    MessageQueue.AddPlayerMessage($"Removed {uninstalled.ShortDisplayName} from {this.ParentObject.ShortDisplayName}.");
                if (uninstalled != null)
                    inventoryViewer.Inventory.AddObject(uninstalled, false, false, false);
                inventoryViewer.UseEnergy(1000, "Physical");
            }
            else
            {
                if (inventoryViewer.IsPlayer())
                    MessageQueue.AddPlayerMessage("You can't remove integral attachments.");
            }

        }

        public void UninstallAttachmentFromSlot(string slot)
        {
            List<IPart> parts = this.ParentObject.PartsList;
            foreach (IPart part in parts)
            {
                if (PartIsAttachment(part))
                {
                    WWA_Attachment attachment = part as WWA_Attachment;
                    if (AttachmentFitsInSlot(attachment.displayName, slot))
                    {
                        UninstallAttachment(attachment);
                        break;
                    }
                }
            }
        }

        public void UninstallAttachmentByName(string attachmentName)
        {
            List<IPart> parts = this.ParentObject.PartsList;
            foreach (IPart part in parts)
            {
                if (PartIsAttachment(part))
                {
                    WWA_Attachment attachment = part as WWA_Attachment;
                    if (attachment.displayName == attachmentName)
                    {
                        UninstallAttachment(attachment);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Uninstal all non-integral attachments from weapon
        /// </summary>
        public void UninstallAllAttachments()
        {
            string s;
            while (WeaponHasAttachment(out s, true))
            {
                if (s != "none")
                    UninstallAttachmentFromSlot(s);
            }
            MessageQueue.AddPlayerMessage($"Uninstalled all attachments from {this.ParentObject.ShortDisplayName}.");
        }

        /// <summary>
        /// Does weapon have an attachment in slot?
        /// </summary>
        /// <param name="s">Slot name</param>
        /// <param name="removeableOnly">Can attachment be removed from weapon or is it integral?</param>
        /// <returns></returns>
        public bool WeaponHasAttachment(out string s, bool removeableOnly = false)
        {
            s = "none";
            List<IPart> parts = this.ParentObject.PartsList;
            foreach (IPart part in parts)
            {
                if (PartIsAttachment(part))
                {
                    WWA_Attachment attachment = part as WWA_Attachment;
                    foreach (KeyValuePair<string, string[]> kvp in AttachmentSlots)
                        if (kvp.Value.Contains(attachment.displayName))
                            s = kvp.Key;
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

        public bool PartIsAttachment(IPart part) => part.GetType().BaseType.Name == "WWA_Attachment";        

        public Dictionary<string, GameObject> FindAttachmentsForSlot(string slot, List<string> possibleAttachments)
        {
            Dictionary<string, GameObject> attachments = new Dictionary<string, GameObject>();
            List<GameObject> inventory = inventoryViewer.GetInventory();
            foreach (GameObject GO in inventory)
            {
                if (GO.pPhysics.Category == "Weapon Attachments")
                {
                    List<IPart> parts = GO.PartsList;
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
            if (this.ParentObject.Equipped != null)
                if (this.ParentObject.Equipped.HasPart("WWA_TacticalAbilities"))
                    return this.ParentObject.Equipped.GetPart("WWA_TacticalAbilities") as WWA_TacticalAbilities;
                else if (this.inventoryViewer != null)
                    if (this.inventoryViewer.HasPart("WWA_TacticalAbilities"))
                        return this.inventoryViewer.GetPart("WWA_TacticalAbilities") as WWA_TacticalAbilities;

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
                if (!FireMode)
                    this.ParentObject.Equipped.pPhysics.PlayWorldSound(SingleFireSound, 0.5f, 0.0f, true, (Cell)null);
                else if (FireRate < 1)
                    this.ParentObject.Equipped.pPhysics.PlayWorldSound(FireBurstSound, 0.5f, 0.0f, true, (Cell)null);
                else
                    this.ParentObject.Equipped.pPhysics.PlayWorldSound(FireBurstHighRateSound, 0.5f, 0.0f, true, (Cell)null);

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
                if (!FireMode && DefaultFireRate == 1)
                {
                    //If in semi-automode fire rate was increased the increase will go to fire rate instead
                    MissileWeapon mw = this.ParentObject.GetPart<MissileWeapon>() as MissileWeapon;
                    if (mw.ShotsPerAction > 1)
                        FireRate = mw.ShotsPerAction - 1;
                }
                return true;
            }
            if (E.ID == "ViewAttachments")
            {
                if (AttachmentSlots.Count == SlotNames.Count && AttachmentSlots.Count > 0)
                {
                    GameObject weapon = this.ParentObject;
                    //if (weapon.HasPart("Stacker"))
                    //{
                    //    Stacker stackerino = weapon.GetPart("Stacker") as Stacker;
                    //    //MessageQueue.AddPlayerMessage(stackerino.StackCount.ToString());
                    //    weapon = stackerino.RemoveOne();
                    //}
                    inventoryViewer = E.GetParameter("Viewer") as GameObject;
                    List<string> slotsAndAttachmentsMenu = new List<string>();
                    Dictionary<string, bool> isSlotOccupied = new Dictionary<string, bool>();
                    foreach (string slot in AttachmentSlots.Keys)
                    {
                        List<IPart> weaponParts = weapon.PartsList;
                        string slotDisplayName = slot;
                        bool occupied = false;
                        slotDisplayName = SlotNames[slot];
                        slotsAndAttachmentsMenu.Add($"{slotDisplayName}: &knone");
                        foreach (IPart part in weaponParts)
                        {
                            if (PartIsAttachment(part))
                            {
                                WWA_Attachment possibleInstalledAttachment = part as WWA_Attachment;
                                if (AttachmentFitsInSlot(possibleInstalledAttachment.displayName, slot))
                                {
                                    string color = "";
                                    if (possibleInstalledAttachment.integral)
                                        color = "&Y";
                                    slotsAndAttachmentsMenu[slotsAndAttachmentsMenu.Count - 1] = slotDisplayName + ": " + color + possibleInstalledAttachment.displayName;
                                    occupied = true;
                                    break;
                                }
                            }
                        }
                        isSlotOccupied.Add(slot, occupied);
                    }
                    if (isSlotOccupied.Values.Contains(true))
                        slotsAndAttachmentsMenu.Add("Remove all attachments");
                    slotsAndAttachmentsMenu.Add("Cancel");
                    int n = -1;
                    do
                    {
                        n = Popup.ShowOptionList("Attachments", slotsAndAttachmentsMenu.ToArray());
                        switch (slotsAndAttachmentsMenu[n])
                        {
                            case "Remove all attachments": UninstallAllAttachments(); break;
                            case "Cancel": break;
                            default:
                                {
                                    string selectedSlot = AttachmentSlots.Keys.ElementAt(n);
                                    string fullSlotName = SlotNames[selectedSlot];
                                    List<string> possibleAttachments = AttachmentSlots[selectedSlot].ToList();
                                    if (selectedSlot != "Cancel")
                                    {
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
                                            n = Popup.ShowOptionList("Choose attachment", names);
                                            //MessageQueue.AddPlayerMessage("You can't remove integral attachments.");
                                            switch (names[n])
                                            {
                                                case "Remove attachment": UninstallAttachmentFromSlot(selectedSlot); break;
                                                case "Cancel": n = -1; break;
                                                default:
                                                    {
                                                        UninstallAttachmentFromSlot(selectedSlot);
                                                        selectedAttachment = attachments.Values.ElementAt(n);
                                                        InstallAttachment(selectedAttachment);
                                                    }
                                                    break;
                                            }
                                        }
                                        else
                                            MessageQueue.AddPlayerMessage($"You don't have any attachments to install on {weapon.ShortDisplayName} {fullSlotName}.");
                                    }
                                }
                                break;
                        }
                    }
                    while (n == -1);
                    //if (this.ParentObject.InInventory != null)
                    //    this.ParentObject.InInventory.Inventory.AddObjectNoStack(weapon);
                    //else
                    //    this.ParentObject.CurrentCell.AddObject(weapon);
                }
                return true;
            }
            if (!(E.ID == "ObjectCreated"))
                return base.FireEvent(E);

            FireRate = 0;
            GameObjectBlueprint blueprint = this.ParentObject.GetBlueprint();
            Dictionary<string, string> blueprintTags = blueprint.Tags;
            AttachmentSlots = new Dictionary<string, string[]>();
            SlotNames = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kvp in blueprintTags)
            {
                if (kvp.Key.Contains("AttachmentSlot") && kvp.Value != "")
                {
                    string[] newSlot = kvp.Key.Split(':');
                    if (newSlot.Length == 3)
                    {
                        AttachmentSlots.Add(newSlot[1], kvp.Value.Split(','));
                        SlotNames.Add(newSlot[1], newSlot[2]);
                    }
                }
            }
            AutomaticOnly = false;
            if (this.ParentObject.GetTag("AutomaticOnly") == "true")
                AutomaticOnly = true;
            if (this.ParentObject.GetTag("MissileFireSound") == "none")
            {
                SingleFireSound = this.ParentObject.GetTag("FireSoundSingle");
                FireBurstSound = this.ParentObject.GetTag("FireBurstSound");
                FireBurstHighRateSound = this.ParentObject.GetTag("FireBurstHighRateSound");
                //MessageQueue.AddPlayerMessage(SingleFireSound + "|" + FireBurstSound + "|" + FireBurstHighRateSound);
            }
            MagazineAmmoLoader mal = this.ParentObject.GetPart("MagazineAmmoLoader") as MagazineAmmoLoader;
            if (mal != null)
            {
                if (this.ParentObject.HasIntProperty("ExtendedMagCapacity"))
                {
                    HighCapacityMagSize = this.ParentObject.GetIntProperty("ExtendedMagCapacity");
                    //if (HighCapacityMagSize == -1)
                    //    HighCapacityMagSize = (int)(mal.MaxAmmo * 1.5f);
                    //MessageQueue.AddPlayerMessage(ParentObject.ShortDisplayName + ": " + HighCapacityMagSize.ToString());
                }
                if (this.ParentObject.HasIntProperty("DrumMagCapacity"))
                {
                    DrumMagCapacity = this.ParentObject.GetIntProperty("DrumMagCapacity");
                    //if (DrumMagCapacity == -1)
                    //    DrumMagCapacity = (int)(mal.MaxAmmo * 3.0f);
                    //MessageQueue.AddPlayerMessage(ParentObject.ShortDisplayName + ": " + DrumMagCapacity.ToString());
                }
            }
            MissileWeapon mw2 = this.ParentObject.GetPart<MissileWeapon>() as MissileWeapon;
            if (mw2 != null)
            {
                DefaultFireRate = mw2.ShotsPerAction;
                DefaultAmmoPerShot = mw2.AmmoPerAction;
                FireMode = DefaultFireRate != 1;
            }
            return true;
        }
    }
}