using System;

namespace OurIPH.Services
{
    public class PriceUpdateStatusService
    {
        public string Format(int total, int fresh, int cacheHits, int missing, string lastError, bool inProgress, int processed, DateTime? timestamp = null)
        {
            var prefix = inProgress
                ? string.Format("Обновляю цены: {0}/{1}", processed, total)
                : string.Format("Цены: total {0}", total);
            var status = string.Format("{0}; fresh {1}; cache {2}; missing {3}", prefix, fresh, cacheHits, Math.Max(0, missing));
            if (!string.IsNullOrWhiteSpace(lastError))
            {
                status += "; last error: " + lastError;
            }
            else if (!inProgress)
            {
                status += string.Format("; {0:HH:mm:ss}", timestamp ?? DateTime.Now);
            }

            return status;
        }
    }
}
