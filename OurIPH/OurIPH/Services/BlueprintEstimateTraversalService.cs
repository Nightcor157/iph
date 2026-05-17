using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintEstimateTraversalService
    {
        public bool ShouldLookupChildBlueprint(BlueprintEstimateRecursionState state, long materialTypeId)
        {
            return state == null || !state.Path.Contains(materialTypeId);
        }

        public void EnterParent(BlueprintEstimateRecursionState state, BlueprintSearchResult parentBlueprint)
        {
            if (state == null || parentBlueprint == null)
            {
                return;
            }

            state.Path.Add(parentBlueprint.ProductTypeId);
        }

        public void ExitParent(BlueprintEstimateRecursionState state, BlueprintSearchResult parentBlueprint)
        {
            if (state == null || parentBlueprint == null)
            {
                return;
            }

            state.Path.Remove(parentBlueprint.ProductTypeId);
        }

        public ChildBlueprintEstimateRequest CreateChildRequest(BlueprintSearchResult parentBlueprint, BlueprintSearchResult childBlueprint,
            MaterialRequirement material, long adjustedQuantity, double materialEfficiency, double timeEfficiency, DecryptorOption decryptor)
        {
            if (childBlueprint == null)
            {
                return null;
            }

            return new ChildBlueprintEstimateRequest
            {
                ParentBlueprint = parentBlueprint,
                ChildBlueprint = childBlueprint,
                Material = material,
                RequiredQuantity = adjustedQuantity,
                ChildRuns = (int)Math.Ceiling(adjustedQuantity / (double)Math.Max(1, childBlueprint.PortionSize)),
                MaterialEfficiency = materialEfficiency,
                TimeEfficiency = timeEfficiency,
                Decryptor = decryptor
            };
        }

        public double ApplySurplusOffset(double childCost, BlueprintSearchResult childBlueprint, int childRuns, long requiredQuantity,
            Func<long, double> getProductUnitPrice, Func<double, double> applySalesTaxesAndFees, bool applySurplusSellback = true)
        {
            if (!applySurplusSellback)
            {
                return childCost;
            }

            if (childBlueprint == null || childRuns <= 0 || requiredQuantity <= 0)
            {
                return childCost;
            }

            var childProduced = childRuns * Math.Max(1, childBlueprint.PortionSize);
            var surplusQuantity = Math.Max(0, childProduced - requiredQuantity);
            if (surplusQuantity <= 0)
            {
                return childCost;
            }

            var surplusUnitValue = getProductUnitPrice == null ? 0 : getProductUnitPrice(childBlueprint.ProductTypeId);
            if (surplusUnitValue <= 0)
            {
                return childCost;
            }

            var surplusGrossValue = surplusQuantity * surplusUnitValue;
            var surplusNetValue = applySalesTaxesAndFees == null ? surplusGrossValue : applySalesTaxesAndFees(surplusGrossValue);
            return Math.Max(0, childCost - surplusNetValue);
        }
    }
}
