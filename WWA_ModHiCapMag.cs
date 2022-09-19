using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    public class WWA_ModHiCapMag : IModification
    {
        public WWA_ModHiCapMag()
        {

        }

        public WWA_ModHiCapMag(int Tier) : base(Tier)
        {

        }

        public override void Configure()
        {
            this.WorksOnSelf = true;
        }

        public override bool ModificationApplicable(GameObject Object)
        {
            if (Object.GetPart<WWA_GunFeatures>().HighCapacityMagSize > 0 && !Object.HasPart("WWA_ModDrumLoaded") && Object.HasPart("MagazineAmmoLoader"))
                return true;
            return false;
        }

        public override void ApplyModification(GameObject Object)
        {
            MagazineAmmoLoader part = Object.GetPart<MagazineAmmoLoader>();
            WWA_GunFeatures gf = Object.GetPart<WWA_GunFeatures>();   
            if (part != null)            
                part.MaxAmmo = gf.HighCapacityMagSize;            
            //this.IncreaseDifficultyAndComplexity(1, 1, (GameObject)null);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID)
                return ID == GetShortDescriptionEvent.ID;
            return true;
        }

        public override bool HandleEvent(GetDisplayNameEvent E)
        {
            if (this.ParentObject.Understood() && !this.ParentObject.HasProperName)
                E.AddAdjective("hi-cap", 0);
            return true;
        }

        public override bool HandleEvent(GetShortDescriptionEvent E)
        {
            E.Postfix.AppendRules("High capacity magazine: This weapon may hold about 1.5x more additional rounds");
            return true;
        }
    }
}