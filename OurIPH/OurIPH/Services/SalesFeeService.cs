using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class SalesFeeService
    {
        public double ApplySalesTaxesAndFees(double grossValue, FacilityPreset preset, FacilityStation station)
        {
            if (grossValue <= 0)
            {
                return 0;
            }

            var salesTax = preset != null && preset.IncludeSalesTax ? GetSalesTax(grossValue, preset) : 0;
            var brokerFee = GetSalesBrokerFee(grossValue, preset);
            var stationFee = (preset != null && preset.IncludeSalesTax) || station == null
                ? 0
                : grossValue * Math.Max(0, station.SalesFeePercent) / 100.0;
            return Math.Max(0, grossValue - salesTax - brokerFee - stationFee);
        }

        public double GetSalesTax(double grossValue, FacilityPreset preset)
        {
            var accounting = ClampSkill(preset == null ? 5 : preset.AccountingSkillLevel);
            var baseSalesTax = Math.Max(0, preset == null ? 4.5 : preset.BaseSalesTaxPercent);
            return (baseSalesTax - accounting * 0.11 * baseSalesTax) / 100.0 * grossValue;
        }

        public double GetSalesBrokerFee(double grossValue, FacilityPreset preset)
        {
            if (preset == null || preset.BrokerFeeMode <= 0)
            {
                return 0;
            }

            double fee;
            if (preset.BrokerFeeMode == 2)
            {
                fee = ((Math.Max(0, preset.SpecialBrokerFeePercent) + Math.Max(0, preset.SccBrokerFeeSurchargePercent)) / 100.0) * grossValue;
            }
            else
            {
                var brokerRelations = ClampSkill(preset.BrokerRelationsSkillLevel);
                var brokerPercent = Math.Max(0, preset.BaseBrokerFeePercent)
                    - 0.3 * brokerRelations
                    - 0.03 * preset.BrokerFactionStanding
                    - 0.02 * preset.BrokerCorpStanding;
                fee = Math.Max(0, brokerPercent) / 100.0 * grossValue;
            }

            return fee <= 0 ? 0 : Math.Max(100, fee);
        }

        private static int ClampSkill(int value)
        {
            if (value < 0)
            {
                return 0;
            }

            return value > 5 ? 5 : value;
        }
    }
}
