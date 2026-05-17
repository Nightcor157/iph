using System;

namespace OurIPH.Services
{
    public sealed class BlueprintManufacturingMathService
    {
        public double CalculateMaterialMultiplier(double materialEfficiency, double stationMaterialMultiplier)
        {
            return Math.Max(0, (1 - materialEfficiency / 100.0) * stationMaterialMultiplier);
        }

        public double CalculateFacilityMaterialBonusPercent(double stationMaterialMultiplier)
        {
            return Math.Max(0, (1 - stationMaterialMultiplier) * 100);
        }

        public double CalculateTimeMultiplier(double timeEfficiency, double stationTimeMultiplier, double implantPercent,
            int industrySkillLevel, int advancedIndustrySkillLevel, double advancedManufacturingSkillMultiplier)
        {
            var implantMultiplier = Math.Max(0, 1 - (implantPercent / 100.0));
            return Math.Max(0, (1 - timeEfficiency / 100.0)
                * stationTimeMultiplier
                * implantMultiplier
                * (1 - industrySkillLevel * 0.04)
                * (1 - advancedIndustrySkillLevel * 0.03)
                * advancedManufacturingSkillMultiplier);
        }

        public double CalculateInstallationCost(double estimatedInputValue, double costIndex, double stationCostMultiplier,
            double factionWarfareMultiplier, double stationTaxRate, double sccIndustryFeeRate, double alphaAccountTaxRate)
        {
            return estimatedInputValue * ((costIndex * stationCostMultiplier * factionWarfareMultiplier)
                + stationTaxRate
                + sccIndustryFeeRate
                + alphaAccountTaxRate);
        }
    }
}
