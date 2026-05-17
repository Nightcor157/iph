using OurIPH.Models;

namespace OurIPH.Services
{
    public class BlueprintEfficiencyService
    {
        public void GetEffectiveEfficiency(
            BlueprintSearchResult blueprint,
            double enteredMe,
            double enteredTe,
            DecryptorOption decryptor,
            bool useInventionCosts,
            out double me,
            out double te)
        {
            me = enteredMe;
            te = enteredTe;
            if (useInventionCosts)
            {
                decryptor = decryptor ?? NoDecryptor();
                me = 2 + decryptor.MaterialEfficiencyModifier;
                te = 4 + decryptor.TimeEfficiencyModifier;
                return;
            }

            if (blueprint != null && blueprint.IsCopyOnlyBlueprint)
            {
                me = 0;
                te = 0;
            }
        }

        public void GetDefaultChildEfficiency(
            BlueprintSearchResult blueprint,
            FacilityPreset facilityPreset,
            DecryptorOption decryptor,
            bool hasOwnedEfficiency,
            int ownedMe,
            int ownedTe,
            out double me,
            out double te)
        {
            if (blueprint == null || blueprint.HasReactionActivity || blueprint.IsCopyOnlyBlueprint)
            {
                me = 0;
                te = 0;
                return;
            }

            if (hasOwnedEfficiency)
            {
                me = ownedMe;
                te = ownedTe;
                return;
            }

            if (blueprint.TechLevel >= 2)
            {
                decryptor = decryptor ?? NoDecryptor();
                me = 2 + decryptor.MaterialEfficiencyModifier;
                te = 4 + decryptor.TimeEfficiencyModifier;
                return;
            }

            me = facilityPreset == null ? 10 : facilityPreset.DefaultBlueprintMe;
            te = facilityPreset == null ? 20 : facilityPreset.DefaultBlueprintTe;
        }

        private static DecryptorOption NoDecryptor()
        {
            return new DecryptorOption { TypeId = 0, Name = "None", ProbabilityModifier = 1.0 };
        }
    }
}
