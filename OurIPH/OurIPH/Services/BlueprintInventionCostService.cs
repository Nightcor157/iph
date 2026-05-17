using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintInventionCostService
    {
        public InventionCostResult CalculateMaterialCosts(InventionCostContext context)
        {
            var result = new InventionCostResult();
            if (context == null || context.Invention == null || context.Plan == null)
            {
                return result;
            }

            var getUnitPrice = context.GetUnitPrice ?? (_ => 0);
            var inventionJobs = context.Plan.Jobs;
            var totalInventedRuns = Math.Max(1, context.Plan.SuccessfulJobsNeeded * context.Plan.RunsPerSuccess);

            if (context.Invention.SourceBlueprintTypeId > 0)
            {
                var unitPrice = getUnitPrice(context.Invention.SourceBlueprintTypeId);
                if (unitPrice > 0)
                {
                    result.SourceCost = inventionJobs * unitPrice;
                }
                else if (context.TechLevel == 3)
                {
                    result.MissingPrice = true;
                }
            }

            foreach (var material in context.Invention.Materials)
            {
                var unitPrice = getUnitPrice(material.TypeId);
                if (unitPrice <= 0)
                {
                    result.MissingPrice = true;
                    continue;
                }

                result.MaterialCost += material.Quantity * inventionJobs * unitPrice;
            }

            if (context.DecryptorTypeId > 0)
            {
                var unitPrice = getUnitPrice(context.DecryptorTypeId);
                if (unitPrice <= 0)
                {
                    result.MissingPrice = true;
                }
                else
                {
                    result.DecryptorCost = inventionJobs * unitPrice;
                }
            }

            if (context.TechLevel != 3)
            {
                var totalCopyCost = 0.0;
                foreach (var material in context.Invention.CopyMaterials)
                {
                    var unitPrice = getUnitPrice(material.TypeId);
                    if (unitPrice <= 0)
                    {
                        result.MissingPrice = true;
                        continue;
                    }

                    totalCopyCost += material.Quantity * inventionJobs * unitPrice;
                }

                result.CopyMaterialCost = totalCopyCost / totalInventedRuns * context.ManufacturingRuns;
            }

            return result;
        }

        public InventionUsageCostResult CalculateUsageCost(InventionUsageCostContext context)
        {
            var result = new InventionUsageCostResult();
            if (context == null || context.Jobs <= 0 || context.TotalInventedRuns <= 0 || context.RequestedRuns <= 0)
            {
                return result;
            }

            result.JobGrossCost = (Math.Max(0, context.EstimatedInputValue) * 0.02)
                * context.CostIndex
                * context.FactionWarfareMultiplier
                * context.FacilityCostMultiplier;
            result.JobTax = result.JobGrossCost * Math.Max(0, context.IndustryTaxPercent) / 100.0;
            result.UsageCost = ((result.JobGrossCost + result.JobTax) * context.Jobs) / context.TotalInventedRuns * context.RequestedRuns;
            return result;
        }
    }
}
