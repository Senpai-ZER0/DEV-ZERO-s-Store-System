using System;
using System.Collections.Generic;
using VRage.Game;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Domain;
using ZeroStoreSystem.Pricing;

namespace ZeroStoreSystem.Generation
{
    public class StoreInventoryGenerator
    {
        public StoreGenerationResult Generate(StoreBlockConfig blockConfig, GlobalStoreConfig globalConfig)
        {
            if (blockConfig == null || !blockConfig.Enabled)
            {
                var disabled = new StoreGenerationResult();
                disabled.Diagnostics.Add("Block config disabled or null.");
                return disabled;
            }

            var effectiveGlobalConfig = globalConfig ?? GlobalStoreConfigManager.GetDefaultConfig();

            if (effectiveGlobalConfig == null || !effectiveGlobalConfig.UseGlobalRules)
            {
                if (blockConfig.ItemRules == null || blockConfig.ItemRules.Count == 0)
                    return GenerateFallback(blockConfig, effectiveGlobalConfig);

                return GenerateFromLocalOnly(blockConfig, effectiveGlobalConfig);
            }

            var mergedRules = StoreRuleMerger.Merge(effectiveGlobalConfig, blockConfig);

            if (mergedRules == null || mergedRules.Count == 0)
                return GenerateFallback(blockConfig, effectiveGlobalConfig);

            return GenerateFromMerged(blockConfig, effectiveGlobalConfig, mergedRules);
        }

        private StoreGenerationResult GenerateFromLocalOnly(StoreBlockConfig blockConfig, GlobalStoreConfig globalConfig)
        {
            var result = new StoreGenerationResult();
            result.ProfileId = string.IsNullOrWhiteSpace(blockConfig.ProfileId)
                ? (globalConfig != null ? globalConfig.DefaultProfileId : "neutral")
                : blockConfig.ProfileId;

            if (blockConfig.ItemRules == null || blockConfig.ItemRules.Count == 0)
                return GenerateFallback(blockConfig, globalConfig);

            for (int i = 0; i < blockConfig.ItemRules.Count; i++)
            {
                var rule = blockConfig.ItemRules[i];
                if (rule == null || !rule.Allowed || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                MyDefinitionId itemId;
                try
                {
                    itemId = MyDefinitionId.Parse(rule.Id);
                }
                catch (Exception e)
                {
                    Log.Error("Invalid item Id '" + rule.Id + "': " + e.Message);
                    continue;
                }

                int basePrice = BasePriceCalculator.GetBasePrice(itemId);

                if (CanCreateOffer(blockConfig.TradeMode, rule.Offer))
                {
                    result.Offers.Add(new StoreEntryPlan
                    {
                        ItemId = itemId,
                        Amount = rule.Offer.Amount,
                        PricePerUnit = BasePriceCalculator.ApplyPriceModifier(basePrice, rule.Offer.PriceMod)
                    });
                }

                if (CanCreateOrder(blockConfig.TradeMode, rule.Order))
                {
                    result.Orders.Add(new StoreEntryPlan
                    {
                        ItemId = itemId,
                        Amount = rule.Order.Amount,
                        PricePerUnit = BasePriceCalculator.ApplyPriceModifier(basePrice, rule.Order.PriceMod)
                    });
                }
            }

            result.Diagnostics.Add("Inventory generated from local ItemRules.");
            return result;
        }

        private StoreGenerationResult GenerateFromMerged(StoreBlockConfig blockConfig, GlobalStoreConfig globalConfig, List<MergedStoreItemRule> mergedRules)
        {
            var result = new StoreGenerationResult();
            result.ProfileId = string.IsNullOrWhiteSpace(blockConfig.ProfileId)
                ? (globalConfig != null ? globalConfig.DefaultProfileId : "neutral")
                : blockConfig.ProfileId;

            for (int i = 0; i < mergedRules.Count; i++)
            {
                var rule = mergedRules[i];
                if (rule == null || !rule.Allowed || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                MyDefinitionId itemId;
                try
                {
                    itemId = MyDefinitionId.Parse(rule.Id);
                }
                catch (Exception e)
                {
                    Log.Error("Invalid merged item Id '" + rule.Id + "': " + e.Message);
                    continue;
                }

                int basePrice = BasePriceCalculator.GetBasePrice(itemId);

                if (CanCreateOffer(blockConfig.TradeMode, rule.Offer))
                {
                    result.Offers.Add(new StoreEntryPlan
                    {
                        ItemId = itemId,
                        Amount = rule.Offer.Amount,
                        PricePerUnit = BasePriceCalculator.ApplyPriceModifier(basePrice, rule.Offer.PriceMod)
                    });
                }

                if (CanCreateOrder(blockConfig.TradeMode, rule.Order))
                {
                    result.Orders.Add(new StoreEntryPlan
                    {
                        ItemId = itemId,
                        Amount = rule.Order.Amount,
                        PricePerUnit = BasePriceCalculator.ApplyPriceModifier(basePrice, rule.Order.PriceMod)
                    });
                }
            }

            result.Diagnostics.Add("Inventory generated from merged global + block rules.");
            return result;
        }

        private StoreGenerationResult GenerateFallback(StoreBlockConfig blockConfig, GlobalStoreConfig globalConfig)
        {
            var result = new StoreGenerationResult();
            result.ProfileId = string.IsNullOrWhiteSpace(blockConfig.ProfileId)
                ? (globalConfig != null ? globalConfig.DefaultProfileId : "neutral")
                : blockConfig.ProfileId;

            var steelPlateId = MyDefinitionId.Parse("MyObjectBuilder_Component/SteelPlate");
            var constructionId = MyDefinitionId.Parse("MyObjectBuilder_Component/Construction");

            if (blockConfig.TradeMode == StoreTradeMode.BuyAndSell || blockConfig.TradeMode == StoreTradeMode.SellOnly)
            {
                result.Offers.Add(new StoreEntryPlan
                {
                    ItemId = steelPlateId,
                    Amount = 200,
                    PricePerUnit = 100
                });
            }

            if (blockConfig.TradeMode == StoreTradeMode.BuyAndSell || blockConfig.TradeMode == StoreTradeMode.BuyOnly)
            {
                result.Orders.Add(new StoreEntryPlan
                {
                    ItemId = constructionId,
                    Amount = 150,
                    PricePerUnit = 8
                });
            }

            result.Diagnostics.Add("Fallback inventory generated.");
            return result;
        }

        private static bool CanCreateOffer(StoreTradeMode tradeMode, StoreOfferRule offer)
        {
            if (tradeMode == StoreTradeMode.BuyOnly)
                return false;

            if (offer == null)
                return false;

            if (!offer.Enabled)
                return false;

            if (offer.Amount <= 0)
                return false;

            return true;
        }

        private static bool CanCreateOrder(StoreTradeMode tradeMode, StoreOrderRule order)
        {
            if (tradeMode == StoreTradeMode.SellOnly)
                return false;

            if (order == null)
                return false;

            if (!order.Enabled)
                return false;

            if (order.Amount <= 0)
                return false;

            return true;
        }
    }
}
