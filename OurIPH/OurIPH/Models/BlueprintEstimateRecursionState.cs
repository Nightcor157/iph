using System.Collections.Generic;

namespace OurIPH.Models
{
    public sealed class BlueprintEstimateRecursionState
    {
        public BlueprintEstimateRecursionState()
            : this(new HashSet<long>(), new Dictionary<string, BlueprintEstimate>())
        {
        }

        public BlueprintEstimateRecursionState(HashSet<long> path, Dictionary<string, BlueprintEstimate> estimateCache)
        {
            Path = path ?? new HashSet<long>();
            EstimateCache = estimateCache ?? new Dictionary<string, BlueprintEstimate>();
        }

        public HashSet<long> Path { get; }
        public Dictionary<string, BlueprintEstimate> EstimateCache { get; }
    }
}
