using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public static class BlueprintInventionMathService
    {
        public static InventionPlan CreatePlan(BlueprintSearchResult blueprint, InventionInfo invention, int manufacturingRuns,
            FacilityPreset facilityPreset, DecryptorOption decryptor)
        {
            var plan = new InventionPlan();
            if (blueprint == null || invention == null || manufacturingRuns <= 0)
            {
                return plan;
            }

            decryptor = NormalizeDecryptor(decryptor);
            plan.Chance = CalculateChance(invention.Probability, decryptor, facilityPreset);
            if (plan.Chance <= 0)
            {
                return plan;
            }

            var baseInventedRuns = blueprint.TechLevel == 2 && invention.MaxProductionLimit > 0
                ? invention.MaxProductionLimit
                : invention.RunsPerSuccess;
            plan.RunsPerSuccess = Math.Max(1, baseInventedRuns + decryptor.RunModifier);
            plan.SuccessfulJobsNeeded = Math.Max(1, (int)Math.Ceiling(manufacturingRuns / (double)plan.RunsPerSuccess));
            plan.Jobs = Math.Max(1, (int)Math.Ceiling((1 / plan.Chance) * plan.SuccessfulJobsNeeded));
            return plan;
        }

        public static double CalculateChance(double baseProbability, DecryptorOption decryptor, FacilityPreset facilityPreset)
        {
            decryptor = NormalizeDecryptor(decryptor);
            var encryption = ClampSkill(facilityPreset == null ? 4 : facilityPreset.EncryptionSkillLevel);
            var datacore1 = ClampSkill(facilityPreset == null ? 4 : facilityPreset.DatacoreSkill1Level);
            var datacore2 = ClampSkill(facilityPreset == null ? 4 : facilityPreset.DatacoreSkill2Level);
            return baseProbability * (1 + ((datacore1 + datacore2) / 30.0) + (encryption / 40.0)) * Math.Max(0, decryptor.ProbabilityModifier);
        }

        private static DecryptorOption NormalizeDecryptor(DecryptorOption decryptor)
        {
            return decryptor ?? new DecryptorOption { TypeId = 0, Name = "None", ProbabilityModifier = 1.0 };
        }

        private static int ClampSkill(int value)
        {
            return Math.Max(0, Math.Min(5, value));
        }
    }
}
