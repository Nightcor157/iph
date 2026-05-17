using System;

namespace OurIPH.Services
{
    public static class BlueprintProfitabilityService
    {
        public static BlueprintProfitabilityResult Calculate(double totalCost, double revenue, double productionTimeSeconds)
        {
            var profit = revenue - totalCost;
            return new BlueprintProfitabilityResult
            {
                TotalCost = totalCost,
                Revenue = revenue,
                Profit = profit,
                IskPerHour = productionTimeSeconds > 0 ? profit / productionTimeSeconds * 3600.0 : 0,
                MarginPercent = revenue > 0 ? profit / revenue * 100.0 : 0,
                ReturnOnInvestmentPercent = totalCost > 0 ? profit / totalCost * 100.0 : 0
            };
        }

        public static BlueprintProfitabilityResult Calculate(double materialCost, double installationCost, double inventionCost, double revenue, double productionTimeSeconds)
        {
            return Calculate(Math.Max(0, materialCost) + Math.Max(0, installationCost) + Math.Max(0, inventionCost), revenue, productionTimeSeconds);
        }
    }

    public class BlueprintProfitabilityResult
    {
        public double TotalCost { get; set; }
        public double Revenue { get; set; }
        public double Profit { get; set; }
        public double IskPerHour { get; set; }
        public double MarginPercent { get; set; }
        public double ReturnOnInvestmentPercent { get; set; }
    }
}
