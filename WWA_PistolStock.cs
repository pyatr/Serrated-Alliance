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
            ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", aimingBonus, true);
            ParentObject.Physics.UsesTwoSlots = true;
            MissileWeapon mw = ParentObject.GetPart<MissileWeapon>();
            if (mw != null)
            {
                mw.Skill = "Rifle";
            }
            if (ParentObject.Equipped != null)
                ParentObject.Equipped.ForceEquipObject(ParentObject, ParentObject.EquippedOn(), true);
            return base.OnInstall();
        }

        public override bool OnUninstall()
        {
            ParentObject.ModIntProperty("MissileWeaponAccuracyBonus", -aimingBonus, true);
            ParentObject.Physics.UsesTwoSlots = false;
            MissileWeapon mw = ParentObject.GetPart<MissileWeapon>();
            if (mw != null)
            {
                mw.Skill = "Pistol";
            }
            if (ParentObject.Equipped != null)
                ParentObject.Equipped.ForceEquipObject(ParentObject, ParentObject.EquippedOn(), true);
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