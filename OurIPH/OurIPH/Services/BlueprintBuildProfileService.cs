using OurIPH.Models;

namespace OurIPH.Services
{
    public class BlueprintBuildProfileService
    {
        private readonly BlueprintFilteringService _filteringService;

        public BlueprintBuildProfileService(BlueprintFilteringService filteringService)
        {
            _filteringService = filteringService ?? new BlueprintFilteringService();
        }

        public bool Passes(BlueprintSearchResult blueprint, BlueprintBuildProfileOptions options)
        {
            if (blueprint == null)
            {
                return false;
            }

            options = options ?? new BlueprintBuildProfileOptions();
            if (!options.AllowTech2 && blueprint.TechLevel == 2)
            {
                return false;
            }

            if (!options.AllowTech3 && blueprint.TechLevel == 3)
            {
                return false;
            }

            if (!options.AllowReactions && _filteringService.IsReactionBlueprint(blueprint))
            {
                return false;
            }

            if (!options.AllowCapital && _filteringService.IsCapitalBlueprint(blueprint))
            {
                return false;
            }

            if (options.HideLimitedSource && _filteringService.IsLimitedSourceBlueprint(blueprint))
            {
                return false;
            }

            return !options.HideRareOrNoise || !_filteringService.IsRareOrNoiseBlueprint(blueprint);
        }
    }
}
