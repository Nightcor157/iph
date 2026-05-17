using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public sealed class BlueprintInventionTimeService
    {
        public InventionTimeResult CalculateScienceTimes(InventionTimeContext context)
        {
            var result = new InventionTimeResult();
            if (context == null || context.Invention == null || context.Plan == null)
            {
                return result;
            }

            var inventionJobs = context.Plan.Jobs;
            if (context.TechLevel != 3 && context.Invention.BaseCopyTime > 0)
            {
                result.CopyTimeSeconds = context.Invention.BaseCopyTime
                    * Math.Max(0, 1 - 0.05 * context.ScienceSkillLevel)
                    * Math.Max(0, 1 - 0.03 * context.AdvancedIndustrySkillLevel)
                    * context.CopyTimeMultiplier
                    * inventionJobs;
            }

            if (context.Invention.BaseInventionTime > 0)
            {
                var laboratoryLines = Math.Max(1, context.LaboratoryLines);
                var inventionSessions = Math.Ceiling(inventionJobs / (double)laboratoryLines);
                result.InventionTimeSeconds = context.Invention.BaseInventionTime
                    * Math.Max(0, 1 - 0.03 * context.AdvancedIndustrySkillLevel)
                    * context.InventionTimeMultiplier
                    * inventionSessions;
            }

            return result;
        }
    }
}
