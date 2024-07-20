using System;

namespace XRL.World.Parts
{
    [Serializable]
    public class WWA_HighCalibreAmmoSlug : IAmmo
    {
        public string ProjectileObject;

        public override bool SameAs(IPart p)
        {
            if ((p as WWA_HighCalibreAmmoSlug).ProjectileObject != ProjectileObject)
                return false;
            return base.SameAs(p);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != GetProjectileObjectEvent.ID)
                return ID == QueryEquippableListEvent.ID;
            return true;
        }

        public override bool HandleEvent(QueryEquippableListEvent E)
        {
            if (E.SlotType.Contains(nameof(WWA_HighCalibreAmmoSlug)) && !E.List.Contains(ParentObject))
                E.List.Add(ParentObject);
            return true;
        }

        public override bool HandleEvent(GetProjectileObjectEvent E)
        {
            if (string.IsNullOrEmpty(ProjectileObject))
                return true;
            E.Projectile = GameObject.Create(ProjectileObject);
            return false;
        }

        public override bool AllowStaticRegistration()
        {
            return true;
        }
    }
}
