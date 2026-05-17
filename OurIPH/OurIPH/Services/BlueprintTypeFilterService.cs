using System;
using OurIPH.Models;

namespace OurIPH.Services
{
    public class BlueprintTypeFilterService
    {
        public bool Passes(BlueprintSearchResult blueprint, BlueprintTypeFilterOptions options)
        {
            if (blueprint == null)
            {
                return false;
            }

            options = options ?? new BlueprintTypeFilterOptions();
            if (AllEnabled(options))
            {
                return true;
            }

            var category = Classify(blueprint);
            switch (category)
            {
                case BlueprintTypeCategory.Ship:
                    return options.Ships;
                case BlueprintTypeCategory.AmmoCharge:
                    return options.AmmoCharges;
                case BlueprintTypeCategory.Module:
                    return options.Modules;
                case BlueprintTypeCategory.Rig:
                    return options.Rigs;
                case BlueprintTypeCategory.Drone:
                    return options.Drones;
                case BlueprintTypeCategory.Component:
                    return options.Components;
                case BlueprintTypeCategory.Structure:
                    return options.Structures;
                default:
                    return options.Misc;
            }
        }

        public BlueprintTypeCategory Classify(BlueprintSearchResult blueprint)
        {
            if (blueprint == null)
            {
                return BlueprintTypeCategory.Misc;
            }

            var group = blueprint.GroupName ?? "";
            var category = blueprint.CategoryName ?? "";
            var marketGroup = blueprint.ItemMarketGroup ?? "";

            if (TextContainsAny(category, "Ship"))
            {
                return BlueprintTypeCategory.Ship;
            }

            if (TextContainsAny(category, "Charge")
                || TextContainsAny(group, "Charge", "Ammo", "Missile", "Bomb")
                || TextContainsAny(marketGroup, "Charge", "Ammo", "Missile", "Bomb", "Script"))
            {
                return BlueprintTypeCategory.AmmoCharge;
            }

            if (TextContainsAny(category, "Module")
                || TextContainsAny(group, "Module", "Subsystem")
                || TextContainsAny(marketGroup, "Module", "Subsystem"))
            {
                return BlueprintTypeCategory.Module;
            }

            if (TextContainsAny(group, "Rig") || TextContainsAny(marketGroup, "Rig"))
            {
                return BlueprintTypeCategory.Rig;
            }

            if (TextContainsAny(category, "Drone") || TextContainsAny(group, "Drone") || TextContainsAny(marketGroup, "Drone"))
            {
                return BlueprintTypeCategory.Drone;
            }

            if (TextContainsAny(group, "Component", "Composite", "Hybrid Polymer", "Moon Material")
                || TextContainsAny(marketGroup, "Component", "Composite", "Hybrid Polymer", "Moon Material"))
            {
                return BlueprintTypeCategory.Component;
            }

            if (TextContainsAny(category, "Structure")
                || TextContainsAny(group, "Structure", "Citadel", "Engineering Complex", "Refinery", "Deployable")
                || TextContainsAny(marketGroup, "Structure", "Citadel", "Engineering Complex", "Refinery", "Deployable"))
            {
                return BlueprintTypeCategory.Structure;
            }

            return BlueprintTypeCategory.Misc;
        }

        private static bool AllEnabled(BlueprintTypeFilterOptions options)
        {
            return options.Ships
                   && options.AmmoCharges
                   && options.Modules
                   && options.Rigs
                   && options.Drones
                   && options.Components
                   && options.Structures
                   && options.Misc;
        }

        private static bool TextContainsAny(string value, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            foreach (var needle in needles)
            {
                if (value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public enum BlueprintTypeCategory
    {
        Ship,
        AmmoCharge,
        Module,
        Rig,
        Drone,
        Component,
        Structure,
        Misc
    }
}
