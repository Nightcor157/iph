using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class EsiMarketOrderService
    {
        public MarketOrderStats LoadStats(long regionId, long typeId)
        {
            var result = new MarketOrderStats
            {
                RegionId = regionId,
                TypeId = typeId
            };

            var page = 1;
            while (true)
            {
                var url = string.Format(CultureInfo.InvariantCulture,
                    "https://esi.evetech.net/latest/markets/{0}/orders/?datasource=tranquility&order_type=all&page={1}&type_id={2}",
                    regionId,
                    page,
                    typeId);

                JArray rows;
                using (var client = new TimeoutWebClient(30000))
                {
                    client.Headers.Add("User-Agent", "OurIPH/0.1");
                    var json = DownloadStringWithRetry(client, url);
                    rows = JArray.Parse(json);
                }

                if (rows.Count == 0)
                {
                    break;
                }

                foreach (var row in rows)
                {
                    var volumeRemain = (long?)row["volume_remain"] ?? 0;
                    if ((bool?)row["is_buy_order"] == true)
                    {
                        result.BuyOrders++;
                        result.BuyVolume += volumeRemain;
                    }
                    else
                    {
                        result.SellOrders++;
                        result.SellVolume += volumeRemain;
                    }
                }

                if (rows.Count < 1000)
                {
                    break;
                }

                page++;
            }

            return result;
        }

        public Dictionary<long, MarketOrderStats> LoadStats(long regionId, IEnumerable<long> typeIds)
        {
            return typeIds.Distinct().ToDictionary(typeId => typeId, typeId => LoadStats(regionId, typeId));
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

            throw new WebException("ESI market order download failed after retries", lastError);
        }

    }
}
