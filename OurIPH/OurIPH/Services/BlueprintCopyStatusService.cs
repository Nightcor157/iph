using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintCopyStatusService
    {
        public string AddCopyStatus(BlueprintSearchResult blueprint, int runs, string status)
        {
            if (blueprint == null || !blueprint.IsCopyOnlyBlueprint)
            {
                return status;
            }

            var runsPerCopy = Math.Max(1, blueprint.MaxProductionLimit);
            var copies = (int)Math.Ceiling(Math.Max(1, runs) / (double)runsPerCopy);
            return string.Format("{0} | BPC only ME/TE 0/0, {1:N0}x{2:N0} run", status, copies, runsPerCopy);
        }
    }
}
