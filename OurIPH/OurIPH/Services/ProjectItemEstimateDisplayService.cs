using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class ProjectItemEstimateDisplayService
    {
        private readonly ProjectItemEstimateApplicationService _applicationService;
        private readonly SalesFeeService _salesFeeService;
        private readonly BlueprintEstimateStatusService _statusService;
        private readonly BlueprintCopyStatusService _copyStatusService;

        public ProjectItemEstimateDisplayService()
            : this(new ProjectItemEstimateApplicationService(), new SalesFeeService(),
                new BlueprintEstimateStatusService(), new BlueprintCopyStatusService())
        {
        }

        public ProjectItemEstimateDisplayService(
            ProjectItemEstimateApplicationService applicationService,
            SalesFeeService salesFeeService,
            BlueprintEstimateStatusService statusService,
            BlueprintCopyStatusService copyStatusService)
        {
            _applicationService = applicationService ?? new ProjectItemEstimateApplicationService();
            _salesFeeService = salesFeeService ?? new SalesFeeService();
            _statusService = statusService ?? new BlueprintEstimateStatusService();
            _copyStatusService = copyStatusService ?? new BlueprintCopyStatusService();
        }

        public void ResetUnavailable(BuildProjectItem item, string status)
        {
            _applicationService.ResetUnavailable(item, status);
        }

        public void Apply(
            BuildProjectItem item,
            BlueprintSearchResult blueprint,
            BlueprintEstimate estimate,
            ContractPriceResult productPriceResult,
            long productMarketVolume,
            FacilityPreset facilityPreset,
            FacilityStation station,
            bool hasPriceCache)
        {
            if (item == null)
            {
                return;
            }

            productPriceResult = productPriceResult ?? ContractPriceResult.Empty("");
            var revenue = _salesFeeService.ApplySalesTaxesAndFees(productPriceResult.UnitPrice * item.TotalQuantity, facilityPreset, station);
            var status = _statusService.GetProjectItemStatus(estimate, hasPriceCache);
            status = _copyStatusService.AddCopyStatus(blueprint, item.Runs, status);

            _applicationService.Apply(item, estimate, revenue, productPriceResult, productMarketVolume, status, station);
        }
    }
}
