using System;
using XRL.Messages;

namespace XRL.World.Parts
{
    public class WWA_ModDrumLoaded : IModification
    {
        public WWA_ModDrumLoaded()
        {

        }

        public WWA_ModDrumLoaded(int Tier) : base(Tier)
        {

        }

        public override void Configure()
        {
            WorksOnSelf = true;
        }

        public override bool ModificationApplicable(GameObject Object)
        {
            if (Object.GetPart<WWA_GunFeatures>().DrumMagCapacity > 0 && /*!Object.HasPart("WWA_ModHiCapMag") && */ Object.HasPart("MagazineAmmoLoader"))
                return true;
            return false;
        }

        public override void ApplyModification(GameObject Object)
        {
            MagazineAmmoLoader part = Object.GetPart<MagazineAmmoLoader>();
            WWA_GunFeatures gf = Object.GetPart<WWA_GunFeatures>();
            if (part != null)
                part.MaxAmmo = gf.DrumMagCapacity;
            IncreaseDifficultyAndComplexity(1, 1, null);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID)
                return ID == GetShortDescriptionEvent.ID;
            return true;
        }

        public override bool HandleEvent(GetDisplayNameEvent E)
        {
            if (ParentObject.Understood() && !ParentObject.HasProperName)
                E.AddAdjective("drum-loaded", 0);
            return true;
        }

        public override bool HandleEvent(GetShortDescriptionEvent E)
        {
            E.Postfix.AppendRules("Drum-loaded magazine: This weapon may hold about 3x more additional rounds");
            return true;
        }
    }
}