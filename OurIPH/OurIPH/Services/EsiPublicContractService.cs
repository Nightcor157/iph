using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class EsiPublicContractService
    {
        public List<ContractPriceSample> LoadSamples(long regionId, IEnumerable<BlueprintSearchResult> targets, int maxRegionPages, int maxContractsToInspect)
        {
            var targetList = targets == null
                ? new List<BlueprintSearchResult>()
                : targets.Where(item => item != null && item.ProductTypeId > 0 && !string.IsNullOrWhiteSpace(item.ProductName)).ToList();
            var samples = new List<ContractPriceSample>();
            if (regionId <= 0 || targetList.Count == 0)
            {
                return samples;
            }

            var inspected = 0;
            var pageLimit = Math.Max(1, maxRegionPages);
            var inspectLimit = Math.Max(1, maxContractsToInspect);
            var totalPages = pageLimit;
            for (var page = 1; page <= Math.Min(pageLimit, totalPages); page++)
            {
                int pages;
                var contracts = LoadContractPage(regionId, page, out pages);
                totalPages = Math.Max(1, pages);
                foreach (var contract in contracts.Where(IsCandidateContract))
                {
                    if (inspected >= inspectLimit)
                    {
                        return samples;
                    }

                    var candidateTargets = GetCandidateTargets(contract, targetList);
                    if (candidateTargets.Count == 0)
                    {
                        continue;
                    }

                    inspected++;
                    var items = LoadContractItems(contract.ContractId);
                    foreach (var target in candidateTargets)
                    {
                        var sample = TryCreateSample(target, contract, items);
                        if (sample != null)
                        {
                            samples.Add(sample);
                        }
                    }

                    Thread.Sleep(120);
                }
            }

            return samples;
        }

        public ContractPriceSample TryCreateSample(BlueprintSearchResult target, PublicContractSummary contract, IEnumerable<PublicContractItem> items)
        {
            if (target == null || contract == null || items == null || contract.Price <= 0)
            {
                return null;
            }

            var included = items.Where(item => item != null && item.IsIncluded && item.TypeId > 0 && item.Quantity > 0).ToList();
            if (included.Count == 0)
            {
                return null;
            }

            var distinctIncludedTypes = included.Select(item => item.TypeId).Distinct().Count();
            var targetQuantity = included.Where(item => item.TypeId == target.ProductTypeId).Sum(item => item.Quantity);
            if (targetQuantity <= 0 || distinctIncludedTypes != 1)
            {
                return null;
            }

            return new ContractPriceSample
            {
                TypeId = target.ProductTypeId,
                TypeName = target.ProductName,
                Price = contract.Price / targetQuantity,
                ObservedAt = DateTime.Now,
                ContractId = contract.ContractId,
                Source = "ESI public contracts",
                LocationId = contract.StartLocationId,
                Quantity = targetQuantity,
                ItemCount = included.Count,
                Title = contract.Title ?? ""
            };
        }

        private static List<BlueprintSearchResult> GetCandidateTargets(PublicContractSummary contract, IEnumerable<BlueprintSearchResult> targets)
        {
            if (!string.IsNullOrWhiteSpace(contract.Title))
            {
                return targets.Where(target => contract.Title.IndexOf(target.ProductName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            return targets.ToList();
        }

        private static bool IsCandidateContract(PublicContractSummary contract)
        {
            return contract != null
                   && contract.ContractId > 0
                   && contract.Price > 0
                   && string.Equals(contract.Type, "item_exchange", StringComparison.OrdinalIgnoreCase)
                   && (string.IsNullOrWhiteSpace(contract.Availability) || string.Equals(contract.Availability, "public", StringComparison.OrdinalIgnoreCase))
                   && (contract.DateExpired == default(DateTime) || contract.DateExpired > DateTime.UtcNow);
        }

        private List<PublicContractSummary> LoadContractPage(long regionId, int page, out int totalPages)
        {
            var url = string.Format(CultureInfo.InvariantCulture,
                "https://esi.evetech.net/latest/contracts/public/{0}/?datasource=tranquility&page={1}",
                regionId,
                page);

            using (var client = new TimeoutWebClient(30000))
            {
                client.Headers.Add("User-Agent", "OurIPH/0.1");
                var json = DownloadStringWithRetry(client, url);
                totalPages = ReadPages(client);
                return JArray.Parse(json).Select(ReadContractSummary).Where(item => item.ContractId > 0).ToList();
            }
        }

        private List<PublicContractItem> LoadContractItems(long contractId)
        {
            var results = new List<PublicContractItem>();
            var totalPages = 1;
            for (var page = 1; page <= totalPages; page++)
            {
                var url = string.Format(CultureInfo.InvariantCulture,
                    "https://esi.evetech.net/latest/contracts/public/items/{0}/?datasource=tranquility&page={1}",
                    contractId,
                    page);

                using (var client = new TimeoutWebClient(30000))
                {
                    client.Headers.Add("User-Agent", "OurIPH/0.1");
                    var json = DownloadStringWithRetry(client, url);
                    totalPages = Math.Max(totalPages, ReadPages(client));
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        continue;
                    }

                    results.AddRange(JArray.Parse(json).Select(ReadContractItem));
                }
            }

            return results;
        }

        private static PublicContractSummary ReadContractSummary(JToken row)
        {
            return new PublicContractSummary
            {
                ContractId = (long?)row["contract_id"] ?? 0,
                Type = (string)row["type"] ?? "",
                Availability = (string)row["availability"] ?? "",
                Title = (string)row["title"] ?? "",
                Price = (double?)row["price"] ?? 0,
                StartLocationId = (long?)row["start_location_id"] ?? 0,
                DateIssued = ReadDate(row["date_issued"]),
                DateExpired = ReadDate(row["date_expired"])
            };
        }

        private static PublicContractItem ReadContractItem(JToken row)
        {
            return new PublicContractItem
            {
                TypeId = (long?)row["type_id"] ?? 0,
                Quantity = (long?)row["quantity"] ?? 0,
                IsIncluded = (bool?)row["is_included"] == true,
                IsBlueprintCopy = (bool?)row["is_blueprint_copy"] == true,
                IsSingleton = (bool?)row["is_singleton"] == true
            };
        }

        private static DateTime ReadDate(JToken token)
        {
            DateTime value;
            return token != null && DateTime.TryParse(token.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out value)
                ? value
                : default(DateTime);
        }

        private static int ReadPages(WebClient client)
        {
            int value;
            return client.ResponseHeaders != null && int.TryParse(client.ResponseHeaders["X-Pages"], out value) ? Math.Max(1, value) : 1;
        }

        private static string DownloadStringWithRetry(WebClient client, string url)
        {
            Exception lastError = null;
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    return client.DownloadString(url);
                }
                catch (WebException ex)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null && response.StatusCode == HttpStatusCode.NoContent)
                    {
                        return "";
                    }

                    lastError = ex;
                    Thread.Sleep(250 * attempt);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    Thread.Sleep(250 * attempt);
                }
            }

            throw new WebException("ESI public contract download failed after retries", lastError);
        }

    }

    public class PublicContractSummary
    {
        public long ContractId { get; set; }
        public string Type { get; set; }
        public string Availability { get; set; }
        public string Title { get; set; }
        public double Price { get; set; }
        public long StartLocationId { get; set; }
        public DateTime DateIssued { get; set; }
        public DateTime DateExpired { get; set; }
    }

    public class PublicContractItem
    {
        public long TypeId { get; set; }
        public long Quantity { get; set; }
        public bool IsIncluded { get; set; }
        public bool IsBlueprintCopy { get; set; }
        public bool IsSingleton { get; set; }
    }
}
