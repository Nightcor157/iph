using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class FuzzworkMarketService
    {
        public JObject LoadAggregates(long regionOrStationId, IEnumerable<long> typeIds, bool useStation)
        {
            var ids = string.Join(",", typeIds.Distinct());
            var scope = useStation ? "station" : "region";
            var url = $"https://market.fuzzwork.co.uk/aggregates/?{scope}={regionOrStationId}&types={ids}";

            using (var client = new TimeoutWebClient(30000))
            {
                client.Headers.Add("User-Agent", "OurIPH/0.1");
                var json = client.DownloadString(url);
                return JObject.Parse(json);
            }
        }

        public Dictionary<long, MarketPrice> LoadPrices(long regionOrStationId, IEnumerable<long> typeIds, bool useStation)
        {
            return LoadPrices(regionOrStationId, typeIds, useStation, null);
        }

        public Dictionary<long, MarketPrice> LoadPrices(long regionOrStationId, IEnumerable<long> typeIds, bool useStation, Action<MarketPriceLoadProgress> progress)
        {
            var prices = new Dictionary<long, MarketPrice>();
            var ids = typeIds.Distinct().ToList();
            var processed = 0;
            var missing = 0;
            foreach (var batch in Split(ids, 30))
            {
                JObject json;
                try
                {
                    json = LoadAggregatesWithRetry(regionOrStationId, batch, useStation);
                }
                catch (Exception ex)
                {
                    ReportProgress(progress, ids.Count, processed, prices.Count, missing, ex.Message);
                    throw;
                }

                foreach (var typeId in batch)
                {
                    processed++;
                    var node = json[typeId.ToString()];
                    if (node == null)
                    {
                        missing++;
                        continue;
                    }

                    prices[typeId] = new MarketPrice
                    {
                        TypeId = typeId,
                        SellMin = ParseDouble(node["sell"]?["min"]),
                        BuyMax = ParseDouble(node["buy"]?["max"]),
                        SellVolume = ParseLong(node["sell"]?["volume"]),
                        BuyVolume = ParseLong(node["buy"]?["volume"])
                    };
                }

                ReportProgress(progress, ids.Count, processed, prices.Count, missing, null);
                Thread.Sleep(150);
            }

            return prices;
        }

        private static void ReportProgress(Action<MarketPriceLoadProgress> progress, int total, int processed, int fresh, int missing, string lastError)
        {
            if (progress == null)
            {
                return;
            }

            progress(new MarketPriceLoadProgress
            {
                TotalTypeIds = total,
                ProcessedTypeIds = processed,
                FreshDownloadedCount = fresh,
                MissingCount = missing,
                LastError = lastError ?? ""
            });
        }

        private JObject LoadAggregatesWithRetry(long regionOrStationId, IEnumerable<long> typeIds, bool useStation)
        {
            Exception lastError = null;
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    return LoadAggregates(regionOrStationId, typeIds, useStation);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    Thread.Sleep(250 * attempt);
                }
            }

            throw new WebException("Fuzzwork price download failed after retries", lastError);
        }

        private static double ParseDouble(JToken token)
        {
            if (token == null)
            {
                return 0;
            }

            double value;
            return double.TryParse(token.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value)
                ? value
                : 0;
        }

        private static long ParseLong(JToken token)
        {
            if (token == null)
            {
                return 0;
            }

            long value;
            return long.TryParse(token.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value)
                ? value
                : 0;
        }

        private static IEnumerable<List<long>> Split(List<long> values, int size)
        {
            for (var i = 0; i < values.Count; i += size)
            {
                yield return values.Skip(i).Take(size).ToList();
            }
        }

    }
}
