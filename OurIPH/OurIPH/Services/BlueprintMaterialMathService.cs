using System;

namespace OurIPH.Services
{
    public static class BlueprintMaterialMathService
    {
        public static long CalculateAdjustedQuantity(long baseQuantity, int runs, double materialMultiplier)
        {
            if (baseQuantity <= 0 || runs <= 0)
            {
                return 0;
            }

            var multiplier = Math.Max(0, materialMultiplier);
            return Math.Max(runs, (long)Math.Ceiling(Math.Round(baseQuantity * runs * multiplier, 2)));
        }
    }
}
