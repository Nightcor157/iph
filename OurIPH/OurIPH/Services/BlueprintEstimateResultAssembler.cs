using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintEstimateResultAssembler
    {
        private readonly BlueprintProductionTypeService _productionTypeService;

        public BlueprintEstimateResultAssembler()
            : this(new BlueprintProductionTypeService())
        {
        }

        public BlueprintEstimateResultAssembler(BlueprintProductionTypeService productionTypeService)
        {
            _productionTypeService = productionTypeService ?? new BlueprintProductionTypeService();
        }

        public BlueprintEstimate Assemble(BlueprintEstimateAssemblyContext context)
        {
            if (context == null)
            {
                return new BlueprintEstimate();
            }

            var inventionPlan = context.InventionPlan ?? new InventionPlan();
            var productionTime = _productionTypeService.CalculateBlueprintProductionTime(
                context.Blueprint == null ? 0 : context.Blueprint.BaseProductionTime,
                context.Runs,
                context.TimeMultiplier,
                context.FacilityPreset)
                + context.InventionTimeSeconds
                + _productionTypeService.GetParallelProductionTime(
                    context.ComponentProductionTimes,
                    context.FacilityPreset == null ? 10 : context.FacilityPreset.ProductionLines);

            return new BlueprintEstimate
            {
                MaterialCost = context.MaterialCost,
                InstallationCost = context.InstallationCost,
                FacilityMaterialBonusPercent = context.FacilityMaterialBonusPercent,
                CostIndexPercent = context.CostIndexPercent,
                ProductionTimeSeconds = productionTime,
                InventionCost = context.InventionCost,
                InventionMissing = context.InventionMissing,
                InventionChancePercent = inventionPlan.Chance * 100.0,
                InventionJobs = inventionPlan.Jobs,
                InventedRunsPerSuccess = inventionPlan.RunsPerSuccess,
                InventionMaterialsCost = inventionPlan.SourceCost + inventionPlan.MaterialCost + inventionPlan.DecryptorCost,
                InventionCopyCost = inventionPlan.CopyMaterialCost,
                InventionJobUsageCost = inventionPlan.InventionUsageCost + inventionPlan.CopyUsageCost,
                InventionStationName = inventionPlan.InventionStationName,
                InventionStationSystem = inventionPlan.InventionStationSystem,
                CopyStationName = inventionPlan.CopyStationName,
                CopyStationSystem = inventionPlan.CopyStationSystem,
                BuildMaterialLines = context.BuildMaterialLines,
                BuyMaterialLines = context.BuyMaterialLines
            };
        }
    }
}
