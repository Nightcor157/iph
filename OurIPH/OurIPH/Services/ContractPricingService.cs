using System;
using System.Collections.Generic;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class ContractPricingService
    {
        public ContractPriceResult GetEffectiveProductPrice(
            BlueprintSearchResult blueprint,
            MarketPrice marketPrice,
            IEnumerable<ContractPriceSample> samples,
            bool useContracts,
            Func<MarketPrice, double> getMarketUnitPrice,
            Func<MarketPrice, long> getMarketVolume,
            double manualPriceModifierPercent,
            int maxSampleAgeDays)
        {
            var marketUnitPrice = marketPrice == null || getMarketUnitPrice == null ? 0 : getMarketUnitPrice(marketPrice);
            var marketVolume = marketPrice == null || getMarketVolume == null ? 0 : getMarketVolume(marketPrice);
            var contract = SelectContractPrice(blueprint, samples, maxSampleAgeDays, manualPriceModifierPercent);
            var shouldUseContract = useContracts
                                    && contract.UnitPrice > 0
                                    && ShouldPreferContractPrice(blueprint, marketVolume, contract.Confidence);

            if (shouldUseContract)
            {
                return new ContractPriceResult
                {
                    UnitPrice = contract.UnitPrice,
                    ContractUnitPrice = contract.UnitPrice,
                    Source = contract.Source,
                    Detail = contract.Detail,
                    Confidence = contract.Confidence,
                    AcceptedSamples = contract.AcceptedSamples,
                    RejectedSamples = contract.RejectedSamples
                };
            }

            return new ContractPriceResult
            {
                UnitPrice = marketUnitPrice,
                ContractUnitPrice = contract.UnitPrice,
                Source = contract.UnitPrice > 0 ? "Market (contract fallback)" : "Market",
                Detail = contract.Detail,
                Confidence = marketUnitPrice > 0 ? "Market" : "Missing",
                AcceptedSamples = contract.AcceptedSamples,
                RejectedSamples = contract.RejectedSamples
            };
        }

        public ContractPriceResult SelectContractPrice(
            BlueprintSearchResult blueprint,
            IEnumerable<ContractPriceSample> samples,
            int maxSampleAgeDays,
            double manualPriceModifierPercent)
        {
            if (blueprint == null || samples == null)
            {
                return ContractPriceResult.Empty("No contract samples");
            }

            var cutoff = DateTime.Now.AddDays(-Math.Max(1, maxSampleAgeDays));
            var candidates = samples
                .Where(item => item != null
                               && item.TypeId == blueprint.ProductTypeId
                               && item.Price > 0
                               && item.ObservedAt >= cutoff
                               && IsTargetSample(blueprint, item))
                .OrderBy(item => item.Price)
                .ToList();

            if (candidates.Count == 0)
            {
                return ContractPriceResult.Empty("No recent target contract samples");
            }

            var median = Median(candidates.Select(item => item.Price));
            var filtered = candidates
                .Where(item => IsWithinContractBand(item.Price, median))
                .ToList();

            if (filtered.Count == 0)
            {
                return ContractPriceResult.Empty("Contract samples rejected as anomalous", candidates.Count);
            }

            var unitPrice = Median(filtered.Select(item => item.Price));
            if (manualPriceModifierPercent != 0)
            {
                unitPrice = Math.Max(0, unitPrice * (1 + manualPriceModifierPercent / 100.0));
            }

            return new ContractPriceResult
            {
                UnitPrice = unitPrice,
                ContractUnitPrice = unitPrice,
                Source = "Contract",
                Detail = string.Format("Contract median from {0}/{1} samples", filtered.Count, candidates.Count),
                Confidence = filtered.Count >= 3 ? "High" : "Manual",
                AcceptedSamples = filtered.Count,
                RejectedSamples = candidates.Count - filtered.Count
            };
        }

        public ContractPriceReviewResult ReviewSamples(BlueprintSearchResult blueprint, IEnumerable<ContractPriceSample> samples, int maxSampleAgeDays)
        {
            var result = new ContractPriceReviewResult();
            if (blueprint == null || samples == null)
            {
                result.Detail = "No contract samples";
                return result;
            }

            var cutoff = DateTime.Now.AddDays(-Math.Max(1, maxSampleAgeDays));
            var sameTypeSamples = samples
                .Where(item => item != null && item.TypeId == blueprint.ProductTypeId && item.Price > 0)
                .OrderByDescending(item => item.ObservedAt)
                .ToList();
            var candidates = sameTypeSamples
                .Where(item => item.ObservedAt >= cutoff && IsTargetSample(blueprint, item))
                .OrderBy(item => item.Price)
                .ToList();

            var median = Median(candidates.Select(item => item.Price));
            result.MedianPrice = median;

            foreach (var sample in sameTypeSamples)
            {
                var review = new ContractPriceSampleReview
                {
                    ContractId = sample.ContractId,
                    TypeName = sample.TypeName,
                    Price = sample.Price,
                    ObservedAt = sample.ObservedAt,
                    Source = sample.Source,
                    Quantity = sample.Quantity,
                    ItemCount = sample.ItemCount,
                    Title = sample.Title
                };

                if (sample.ObservedAt < cutoff)
                {
                    review.ReviewStatus = "Stale";
                    review.ReviewReason = "Older than sample window";
                }
                else if (!IsTargetSample(blueprint, sample))
                {
                    review.ReviewStatus = "Non-target";
                    review.ReviewReason = "Type name does not match product";
                }
                else if (median <= 0 || !IsWithinContractBand(sample.Price, median))
                {
                    review.ReviewStatus = "Rejected";
                    review.ReviewReason = median > 0 ? "Outside median band" : "No target median";
                    result.RejectedSamples++;
                }
                else
                {
                    review.ReviewStatus = "Accepted";
                    review.ReviewReason = "Inside median band";
                    result.AcceptedSamples++;
                }

                result.Reviews.Add(review);
            }

            result.Detail = result.Reviews.Count == 0
                ? "No samples for selected product"
                : string.Format("Median {0:N0}; accepted {1}; rejected {2}", result.MedianPrice, result.AcceptedSamples, result.RejectedSamples);
            return result;
        }

        public bool ShouldPreferContractPrice(BlueprintSearchResult blueprint, long marketVolume, string contractConfidence)
        {
            if (blueprint == null)
            {
                return false;
            }

            if (!IsContractPreferredProduct(blueprint))
            {
                return marketVolume <= 0 && string.Equals(contractConfidence, "High", StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        public bool IsContractPreferredProduct(BlueprintSearchResult blueprint)
        {
            if (blueprint == null)
            {
                return false;
            }

            return ContainsAny(blueprint.GroupName, "Capital", "Freighter", "Dreadnought", "Carrier", "Force Auxiliary", "Supercarrier", "Titan")
                   || ContainsAny(blueprint.ItemMarketGroup, "Capital", "Freighter", "Dreadnought", "Carrier", "Force Auxiliary", "Supercarrier", "Titan");
        }

        private static bool IsTargetSample(BlueprintSearchResult blueprint, ContractPriceSample sample)
        {
            if (sample == null || sample.TypeId != blueprint.ProductTypeId)
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(sample.TypeName)
                   || string.Equals(sample.TypeName.Trim(), blueprint.ProductName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWithinContractBand(double price, double median)
        {
            if (price <= 0 || median <= 0)
            {
                return false;
            }

            return price >= median * 0.55 && price <= median * 1.8;
        }

        private static double Median(IEnumerable<double> values)
        {
            var ordered = values.Where(item => item > 0).OrderBy(item => item).ToList();
            if (ordered.Count == 0)
            {
                return 0;
            }

            var middle = ordered.Count / 2;
            return ordered.Count % 2 == 1
                ? ordered[middle]
                : (ordered[middle - 1] + ordered[middle]) / 2.0;
        }

        private static bool ContainsAny(string value, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return needles.Any(needle => value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    public class ContractPriceResult
    {
        public double UnitPrice { get; set; }
        public double ContractUnitPrice { get; set; }
        public string Source { get; set; }
        public string Detail { get; set; }
        public string Confidence { get; set; }
        public int AcceptedSamples { get; set; }
        public int RejectedSamples { get; set; }

        public static ContractPriceResult Empty(string detail, int rejectedSamples = 0)
        {
            return new ContractPriceResult
            {
                Source = "Market",
                Detail = detail,
                Confidence = "Missing",
                RejectedSamples = rejectedSamples
            };
        }
    }

    public class ContractPriceReviewResult
    {
        public double MedianPrice { get; set; }
        public int AcceptedSamples { get; set; }
        public int RejectedSamples { get; set; }
        public string Detail { get; set; }
        public List<ContractPriceSampleReview> Reviews { get; } = new List<ContractPriceSampleReview>();
    }
}
