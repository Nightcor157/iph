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
    public class EsiMarketHistoryService
    {
        public MarketHistoryStats LoadStats(long regionId, long typeId, int days)
        {
            var url = string.Format(CultureInfo.InvariantCulture,
                "https://esi.evetech.net/latest/markets/{0}/history/?datasource=tranquility&type_id={1}",
                regionId,
                typeId);

            using (var client = new TimeoutWebClient(30000))
            {
                client.Headers.Add("User-Agent", "OurIPH/0.1");
                var json = DownloadStringWithRetry(client, url);
                var rows = JArray.Parse(json);
                var startDate = DateTime.UtcNow.Date.AddDays(-(days + 1));
                var endDate = DateTime.UtcNow.Date;
                var selected = rows
                    .Select(row => new
                    {
                        Date = DateTime.Parse((string)row["date"], CultureInfo.InvariantCulture).Date,
                        Volume = (long?)row["volume"] ?? 0,
                        Orders = (long?)row["order_count"] ?? 0,
                        AveragePrice = (double?)row["average"] ?? 0
                    })
                    .Where(row => row.Date >= startDate && row.Date < endDate)
                    .ToList();

                var totalVolume = selected.Sum(row => row.Volume);
                return new MarketHistoryStats
                {
                    TypeId = typeId,
                    RegionId = regionId,
                    Days = days,
                    TotalVolume = totalVolume,
                    TotalOrders = selected.Sum(row => row.Orders),
                    AverageDailyVolume = selected.Count == 0 ? 0 : totalVolume / (double)selected.Count,
                    PriceTrend = CalculatePriceTrend(selected.Select((row, index) => new PricePoint
                    {
                        X = index + 1,
                        Price = row.AveragePrice
                    })),
                    UpdatedAt = DateTime.Now
                };
            }
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
                catch (Exception ex)
                {
                    lastError = ex;
                    Thread.Sleep(250 * attempt);
                }
            }

            throw new WebException("ESI market history download failed after retries", lastError);
        }

        public Dictionary<long, MarketHistoryStats> LoadStats(long regionId, IEnumerable<long> typeIds, int days)
        {
            var result = new Dictionary<long, MarketHistoryStats>();
            foreach (var typeId in typeIds.Distinct())
            {
                result[typeId] = LoadStats(regionId, typeId, days);
            }

            return result;
        }

        private static double CalculatePriceTrend(IEnumerable<PricePoint> points)
        {
            var values = points.Where(item => item.Price > 0).ToList();
            if (values.Count <= 1)
            {
                return 0;
            }

            var n = values.Count;
            var xySum = values.Sum(item => item.X * item.Price);
            var xSum = values.Sum(item => item.X);
            var ySum = values.Sum(item => item.Price);
            var xSquaredSum = values.Sum(item => item.X * item.X);
            var denominator = n * xSquaredSum - xSum * xSum;
            if (Math.Abs(denominator) < 0.0000001)
            {
                return 0;
            }

            var slope = (n * xySum - xSum * ySum) / denominator;
            var intercept = (ySum - slope * xSum) / n;
            var todayTrendLinePrice = slope * n + intercept;
            return todayTrendLinePrice == 0 ? 0 : (todayTrendLinePrice - intercept) / todayTrendLinePrice;
        }

        private struct PricePoint
        {
            public int X { get; set; }
            public double Price { get; set; }
        }

    }
}
