using System;
using System.Collections.Generic;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Domain;

namespace ZeroStoreSystem.Generation
{
    public static class StoreRuleMerger
    {
        public static List<MergedStoreItemRule> Merge(GlobalStoreConfig globalConfig, StoreBlockConfig blockConfig)
        {
            var result = new List<MergedStoreItemRule>();

            var globalMap = BuildMap(globalConfig != null ? globalConfig.GlobalItemRules : null);
            var localMap = BuildMap(blockConfig != null ? blockConfig.ItemRules : null);

            var allIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in globalMap)
                allIds.Add(pair.Key);

            foreach (var pair in localMap)
                allIds.Add(pair.Key);

            foreach (var id in allIds)
            {
                StoreItemRule globalRule;
                StoreItemRule localRule;

                globalMap.TryGetValue(id, out globalRule);
                localMap.TryGetValue(id, out localRule);

                if (globalConfig != null && !globalConfig.AllowModdedItems)
                {
                    if (!GlobalStoreConfigManager.IsVanillaComponentId(id))
                        continue;
                }

                result.Add(MergeRule(id, globalRule, localRule));
            }

            return result;
        }

        private static MergedStoreItemRule MergeRule(string id, StoreItemRule globalRule, StoreItemRule localRule)
        {
            var merged = new MergedStoreItemRule
            {
                Id = id,
                Allowed = true,
                ForceInclude = false,
                Offer = new StoreOfferRule(),
                Order = new StoreOrderRule()
            };

            bool globalAllowed = globalRule == null || globalRule.Allowed;
            bool localAllowed = localRule == null || localRule.Allowed;

            if (!globalAllowed)
                merged.Allowed = false;
            else if (!localAllowed)
                merged.Allowed = false;
            else
                merged.Allowed = true;

            merged.ForceInclude =
                (globalRule != null && globalRule.ForceInclude) ||
                (localRule != null && localRule.ForceInclude);

            merged.Offer = MergeOffer(globalRule != null ? globalRule.Offer : null,
                localRule != null ? localRule.Offer : null);

            merged.Order = MergeOrder(globalRule != null ? globalRule.Order : null,
                localRule != null ? localRule.Order : null);

            return merged;
        }

        private static StoreOfferRule MergeOffer(StoreOfferRule globalOffer, StoreOfferRule localOffer)
        {
            if (HasMeaningfulOfferOverride(localOffer))
                return CloneOffer(localOffer);

            if (globalOffer != null)
                return CloneOffer(globalOffer);

            return new StoreOfferRule
            {
                Enabled = false,
                PriceMod = 1f,
                Amount = 0
            };
        }

        private static StoreOrderRule MergeOrder(StoreOrderRule globalOrder, StoreOrderRule localOrder)
        {
            if (HasMeaningfulOrderOverride(localOrder))
                return CloneOrder(localOrder);

            if (globalOrder != null)
                return CloneOrder(globalOrder);

            return new StoreOrderRule
            {
                Enabled = false,
                PriceMod = 1f,
                Amount = 0
            };
        }

        private static bool HasMeaningfulOfferOverride(StoreOfferRule rule)
        {
            if (rule == null)
                return false;

            if (rule.Enabled)
                return true;

            if (rule.Amount > 0)
                return true;

            if (Math.Abs(rule.PriceMod - 1f) > 0.0001f)
                return true;

            return false;
        }

        private static bool HasMeaningfulOrderOverride(StoreOrderRule rule)
        {
            if (rule == null)
                return false;

            if (rule.Enabled)
                return true;

            if (rule.Amount > 0)
                return true;

            if (Math.Abs(rule.PriceMod - 1f) > 0.0001f)
                return true;

            return false;
        }

        private static StoreOfferRule CloneOffer(StoreOfferRule source)
        {
            if (source == null)
                return new StoreOfferRule();

            return new StoreOfferRule
            {
                Enabled = source.Enabled,
                PriceMod = source.PriceMod,
                Amount = source.Amount
            };
        }

        private static StoreOrderRule CloneOrder(StoreOrderRule source)
        {
            if (source == null)
                return new StoreOrderRule();

            return new StoreOrderRule
            {
                Enabled = source.Enabled,
                PriceMod = source.PriceMod,
                Amount = source.Amount
            };
        }

        private static Dictionary<string, StoreItemRule> BuildMap(List<StoreItemRule> rules)
        {
            var map = new Dictionary<string, StoreItemRule>(StringComparer.OrdinalIgnoreCase);

            if (rules == null)
                return map;

            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (rule == null || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                map[rule.Id] = rule;
            }

            return map;
        }
    }
}
