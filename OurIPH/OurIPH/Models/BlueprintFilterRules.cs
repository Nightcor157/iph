using System.Collections.Generic;

namespace OurIPH.Models
{
    public class BlueprintFilterRules
    {
        public List<int> LimitedMetaGroupIds { get; set; } = new List<int>();
        public List<string> LimitedMarketGroupKeywords { get; set; } = new List<string>();
        public List<string> LimitedGroupKeywords { get; set; } = new List<string>();
        public List<string> RareProductKeywords { get; set; } = new List<string>();
        public List<string> RareBlueprintKeywords { get; set; } = new List<string>();
        public List<string> RareGroupKeywords { get; set; } = new List<string>();
        public List<string> RareMarketGroupKeywords { get; set; } = new List<string>();
        public List<string> CapitalKeywords { get; set; } = new List<string>();
        public List<string> ReactionGroupKeywords { get; set; } = new List<string>();
        public List<string> ReactionMarketGroupKeywords { get; set; } = new List<string>();

        public static BlueprintFilterRules CreateDefault()
        {
            return new BlueprintFilterRules
            {
                LimitedMetaGroupIds = new List<int> { 3, 4, 5, 15, 19, 52, 53, 54 },
                LimitedMarketGroupKeywords = new List<string> { "Pirate Faction", "Navy Faction", "Storyline", "Officer", "Deadspace" },
                LimitedGroupKeywords = new List<string> { "Special Edition", "Tournament" },
                RareProductKeywords = new List<string> { "Officer", "'s Modified", "'s Power Diagnostic", "Tournament", "Alliance Tournament" },
                RareBlueprintKeywords = new List<string> { "Officer", "'s Modified", "Tournament", "Alliance Tournament" },
                RareGroupKeywords = new List<string> { "Officer", "Special Edition", "Tournament", "Prototype" },
                RareMarketGroupKeywords = new List<string> { "Officer", "Deadspace", "Special Edition", "Tournament" },
                CapitalKeywords = new List<string> { "Capital", "Freighter", "Dreadnought", "Carrier", "Force Auxiliary", "Supercarrier", "Titan" },
                ReactionGroupKeywords = new List<string> { "Moon", "Composite", "Hybrid Polymer", "Biochemical" },
                ReactionMarketGroupKeywords = new List<string> { "Moon Materials", "Hybrid Polymers", "Booster Materials" }
            };
        }
    }
}
