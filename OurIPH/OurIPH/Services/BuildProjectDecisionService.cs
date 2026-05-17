using System;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BuildProjectDecisionService
    {
        public string GetBuildBuyMode(BuildProject project, long typeId)
        {
            if (project == null || typeId <= 0)
            {
                return "";
            }

            var decision = project.BuildBuyDecisions.FirstOrDefault(item => item.TypeId == typeId);
            return decision == null ? "" : decision.Mode ?? "";
        }

        public void SetBuildBuyMode(BuildProject project, long typeId, string mode)
        {
            if (project == null || typeId <= 0)
            {
                return;
            }

            var decision = project.BuildBuyDecisions.FirstOrDefault(item => item.TypeId == typeId);
            if (string.IsNullOrWhiteSpace(mode) || string.Equals(mode, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                if (decision != null)
                {
                    project.BuildBuyDecisions.Remove(decision);
                }

                return;
            }

            if (decision == null)
            {
                decision = new BuildProjectBuildBuyDecision { TypeId = typeId };
                project.BuildBuyDecisions.Add(decision);
            }

            decision.Mode = mode;
        }
    }
}
