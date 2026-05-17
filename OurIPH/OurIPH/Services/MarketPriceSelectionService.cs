using OurIPH.Models;

namespace OurIPH.Services
{
    public class MarketPriceSelectionService
    {
        public double ApplyModifier(double value, double modifierPercent)
        {
            if (value <= 0)
            {
                return 0;
            }

            return System.Math.Max(0, value * (1 + modifierPercent / 100.0));
        }

        public double GetUnitPrice(MarketPrice price, string mode, double modifierPercent)
        {
            if (price == null)
            {
                return 0;
            }

            var value = IsMaxBuy(mode) ? price.BuyMax : price.SellMin;
            return ApplyModifier(value, modifierPercent);
        }

        public long GetStrictVolume(MarketPrice price, string mode)
        {
            if (price == null)
            {
                return 0;
            }

            return IsMaxBuy(mode) ? price.BuyVolume : price.SellVolume;
        }

        public long GetProductVolume(MarketPrice price, string mode)
        {
            if (price == null)
            {
                return 0;
            }

            return IsMaxBuy(mode) ? price.BuyVolume : price.SellVolume > 0 ? price.SellVolume : price.BuyVolume;
        }

        private static bool IsMaxBuy(string mode)
        {
            return string.Equals(mode, "Max Buy", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
