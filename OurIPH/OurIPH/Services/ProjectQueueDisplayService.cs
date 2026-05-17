using System;
using System.Collections.Generic;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class ProjectQueueDisplayService
    {
        public string GetJobMaterialStatus(BuildProjectItem job, IEnumerable<BuildProjectMaterial> materials)
        {
            if (job == null)
            {
                return "";
            }

            var blockers = (materials ?? Enumerable.Empty<BuildProjectMaterial>())
                .Where(material => material != null
                                   && material.Wave == job.Wave
                                   && string.Equals(material.UsedBy, job.ProductName, StringComparison.OrdinalIgnoreCase)
                                   && material.RemainingToBuy > 0)
                .OrderByDescending(material => material.RemainingToBuy)
                .ThenBy(material => material.Name)
                .ToList();
            if (blockers.Count == 0)
            {
                return "Ready";
            }

            var text = "Need: " + string.Join("; ", blockers.Take(3).Select(material => material.Name + " " + material.RemainingToBuyText));
            return blockers.Count > 3 ? text + "; +" + (blockers.Count - 3) : text;
        }
    }
}
