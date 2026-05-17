using System;
using System.Collections.Generic;
using System.Linq;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BlueprintSkillRequirementService
    {
        public IReadOnlyList<SkillRequirement> BuildRequiredSkills(
            BlueprintSearchResult blueprint,
            Func<BlueprintSearchResult, IEnumerable<SkillRequirement>> loadOwnSkills,
            Func<long, IEnumerable<MaterialRequirement>> loadManufacturingMaterials,
            Func<long, BlueprintSearchResult> findBlueprintByProduct,
            Func<BlueprintSearchResult, bool> shouldAlwaysBuy,
            Func<BlueprintSearchResult, BlueprintSearchResult, bool> shouldStopDrilldown)
        {
            var skillsByType = new Dictionary<long, SkillRequirement>();
            AddBuildChainSkills(
                blueprint,
                loadOwnSkills,
                loadManufacturingMaterials,
                findBlueprintByProduct,
                shouldAlwaysBuy,
                shouldStopDrilldown,
                new HashSet<long>(),
                skillsByType);

            return skillsByType.Values
                .OrderBy(skill => skill.Name)
                .Select(CloneSkillRequirement)
                .ToList();
        }

        private static void AddBuildChainSkills(
            BlueprintSearchResult blueprint,
            Func<BlueprintSearchResult, IEnumerable<SkillRequirement>> loadOwnSkills,
            Func<long, IEnumerable<MaterialRequirement>> loadManufacturingMaterials,
            Func<long, BlueprintSearchResult> findBlueprintByProduct,
            Func<BlueprintSearchResult, bool> shouldAlwaysBuy,
            Func<BlueprintSearchResult, BlueprintSearchResult, bool> shouldStopDrilldown,
            HashSet<long> path,
            Dictionary<long, SkillRequirement> skillsByType)
        {
            if (blueprint == null || loadOwnSkills == null || path.Contains(blueprint.ProductTypeId))
            {
                return;
            }

            path.Add(blueprint.ProductTypeId);
            foreach (var skill in loadOwnSkills(blueprint) ?? Enumerable.Empty<SkillRequirement>())
            {
                AddRequiredSkill(skillsByType, skill);
            }

            if (loadManufacturingMaterials != null && findBlueprintByProduct != null)
            {
                foreach (var material in loadManufacturingMaterials(blueprint.BlueprintTypeId) ?? Enumerable.Empty<MaterialRequirement>())
                {
                    if (material == null || path.Contains(material.TypeId))
                    {
                        continue;
                    }

                    var childBlueprint = findBlueprintByProduct(material.TypeId);
                    if (childBlueprint == null)
                    {
                        continue;
                    }

                    if (shouldAlwaysBuy != null && shouldAlwaysBuy(childBlueprint))
                    {
                        continue;
                    }

                    if (shouldStopDrilldown != null && shouldStopDrilldown(blueprint, childBlueprint))
                    {
                        continue;
                    }

                    AddBuildChainSkills(
                        childBlueprint,
                        loadOwnSkills,
                        loadManufacturingMaterials,
                        findBlueprintByProduct,
                        shouldAlwaysBuy,
                        shouldStopDrilldown,
                        path,
                        skillsByType);
                }
            }

            path.Remove(blueprint.ProductTypeId);
        }

        private static void AddRequiredSkill(Dictionary<long, SkillRequirement> skillsByType, SkillRequirement skill)
        {
            if (skill == null || skill.TypeId <= 0)
            {
                return;
            }

            SkillRequirement existing;
            if (!skillsByType.TryGetValue(skill.TypeId, out existing) || skill.Level > existing.Level)
            {
                skillsByType[skill.TypeId] = CloneSkillRequirement(skill);
            }
        }

        private static SkillRequirement CloneSkillRequirement(SkillRequirement skill)
        {
            return new SkillRequirement
            {
                TypeId = skill.TypeId,
                Name = skill.Name,
                Level = skill.Level,
                OwnedLevel = skill.OwnedLevel
            };
        }
    }
}
