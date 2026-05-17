using System;
using System.Collections.Generic;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BlueprintProductionTypeService
    {
        public string GetProductionType(BlueprintSearchResult blueprint)
        {
            if (blueprint == null)
            {
                return "Manufacturing";
            }

            if (blueprint.HasReactionActivity)
            {
                return "Reactions";
            }

            switch (blueprint.GroupId)
            {
                case 30:
                case 659:
                    return "Supercapital Manufacturing";
                case 963:
                    return "T3 Cruiser Manufacturing";
                case 1305:
                    return "T3 Destroyer Manufacturing";
                case 485:
                case 513:
                case 547:
                case 883:
                case 902:
                case 941:
                case 1538:
                    return "Capital Manufacturing";
                case 873:
                case 913:
                    return "Capital Components";
                case 334:
                    return "Components";
            }

            if (blueprint.CategoryId == 32)
            {
                return "Subsystem Manufacturing";
            }

            if (blueprint.CategoryId == 20)
            {
                return "Booster Manufacturing";
            }

            if (blueprint.CategoryId == 17)
            {
                return "Components";
            }

            if (Contains(blueprint.CategoryName, "Ship") && Contains(blueprint.GroupName, "Capital"))
            {
                return "Capital Manufacturing";
            }

            if (Contains(blueprint.GroupName, "Component"))
            {
                return "Components";
            }

            if (Contains(blueprint.GroupName, "Composite")
                || Contains(blueprint.GroupName, "Hybrid Polymer")
                || Contains(blueprint.GroupName, "Biochemical")
                || Contains(blueprint.GroupName, "Moon"))
            {
                return "Reactions";
            }

            return "Manufacturing";
        }

        public string GetInventionProductionType(BlueprintSearchResult blueprint)
        {
            if (blueprint != null && (blueprint.CategoryId == 32 || blueprint.GroupId == 963 || blueprint.GroupId == 1305))
            {
                return "T3 Invention";
            }

            return "Invention";
        }

        public int GetBuildWave(string productionType)
        {
            switch (productionType)
            {
                case "Reactions":
                case "Reprocessing":
                    return 1;
                case "Components":
                case "Capital Components":
                case "T3 Cruiser Manufacturing":
                case "T3 Destroyer Manufacturing":
                case "Subsystem Manufacturing":
                case "Booster Manufacturing":
                    return 2;
                default:
                    return 3;
            }
        }

        public double GetFactionWarfareCostMultiplier(int level)
        {
            switch (Math.Max(0, Math.Min(5, level)))
            {
                case 1:
                    return 0.9;
                case 2:
                    return 0.8;
                case 3:
                    return 0.7;
                case 4:
                    return 0.6;
                case 5:
                    return 0.5;
                default:
                    return 1.0;
            }
        }

        public double GetParallelProductionTime(IEnumerable<double> jobTimes, int lineCount)
        {
            var jobs = (jobTimes ?? Enumerable.Empty<double>())
                .Where(time => time > 0)
                .OrderByDescending(time => time)
                .ToList();
            if (jobs.Count == 0)
            {
                return 0;
            }

            var lines = new double[Math.Max(1, lineCount)];
            foreach (var job in jobs)
            {
                var bestLine = 0;
                for (var i = 1; i < lines.Length; i++)
                {
                    if (lines[i] < lines[bestLine])
                    {
                        bestLine = i;
                    }
                }

                lines[bestLine] += job;
            }

            return lines.Max();
        }

        public double CalculateBlueprintProductionTime(double baseProductionTime, int runs, double timeMultiplier, FacilityPreset facilityPreset)
        {
            if (baseProductionTime <= 0 || runs <= 0)
            {
                return 0;
            }

            var lineCount = Math.Max(1, facilityPreset == null ? 10 : facilityPreset.ProductionLines);
            var jobsPerBatch = Math.Max(1, Math.Min(lineCount, runs));
            var sessions = Math.Ceiling(runs / (double)jobsPerBatch);
            return baseProductionTime * Math.Max(0, timeMultiplier) * sessions;
        }

        private static bool Contains(string value, string needle)
        {
            return value != null && value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
