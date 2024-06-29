using System;
using XRL.UI;
using XRL.Messages;
using XRL.World;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_PistolStock : WWA_Attachment
    {
        public int aimingBonus = 2;

        public WWA_PistolStock()
        {
            displayName = "pistol stock";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            base.Register(Object, Registrar);
        }

        public override bool OnInstall()
        {
            this.ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", aimingBonus, true);
            this.ParentObject.pPhysics.UsesTwoSlots = true;
            MissileWeapon mw = this.ParentObject.GetPart<MissileWeapon>() as MissileWeapon;
            if (mw != null)
            {
                mw.Skill = "Rifle";
            }
            if (this.ParentObject.Equipped != null)
                this.ParentObject.Equipped.ForceEquipObject(this.ParentObject, this.ParentObject.EquippedOn(), true);
            return base.OnInstall();
        }

        public override bool OnUninstall()
        {
            this.ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -aimingBonus, true);
            this.ParentObject.pPhysics.UsesTwoSlots = false;
            MissileWeapon mw = this.ParentObject.GetPart<MissileWeapon>() as MissileWeapon;
            if (mw != null)
            {
                mw.Skill = "Pistol";
            }
            if (this.ParentObject.Equipped != null)
                this.ParentObject.Equipped.ForceEquipObject(this.ParentObject, this.ParentObject.EquippedOn(), true);
            return base.OnUninstall();
        }

        public override string GetDescription()
        {
            string s = "Pistol stock: this pistol has a stock which increases player accuracy by " + aimingBonus.ToString() + " but uses both hands. Weapon will use rifle skill.";
            return s + base.GetDescription();
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }
    }
}