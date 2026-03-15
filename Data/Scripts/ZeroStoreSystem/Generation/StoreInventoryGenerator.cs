using System;
using VRage.Game;
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

            if (blockConfig.ItemRules == null || blockConfig.ItemRules.Count == 0)
                return GenerateFallback(blockConfig, globalConfig);

            return GenerateFromConfig(blockConfig, globalConfig);
        }

        private StoreGenerationResult GenerateFromConfig(StoreBlockConfig blockConfig, GlobalStoreConfig globalConfig)
        {
            var result = new StoreGenerationResult();
            result.ProfileId = string.IsNullOrWhiteSpace(blockConfig.ProfileId)
                ? (globalConfig != null ? globalConfig.DefaultProfileId : "neutral")
                : blockConfig.ProfileId;

            foreach (var rule in blockConfig.ItemRules)
            {
                if (rule == null)
                    continue;

                if (!rule.Allowed)
                {
                    result.Diagnostics.Add("Skipped item (Allowed=false): " + rule.Id);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(rule.Id))
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

                if (CanCreateOffer(blockConfig, rule))
                {
                    result.Offers.Add(new StoreEntryPlan
                    {
                        ItemId = itemId,
                        Amount = rule.Offer.Amount,
                        PricePerUnit = BasePriceCalculator.ApplyPriceModifier(basePrice, rule.Offer.PriceMod)
                    });
                }

                if (CanCreateOrder(blockConfig, rule))
                {
                    result.Orders.Add(new StoreEntryPlan
                    {
                        ItemId = itemId,
                        Amount = rule.Order.Amount,
                        PricePerUnit = BasePriceCalculator.ApplyPriceModifier(basePrice, rule.Order.PriceMod)
                    });
                }
            }

            result.Diagnostics.Add("Inventory generated from ItemRules.");
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

        private static bool CanCreateOffer(StoreBlockConfig config, StoreItemRule rule)
        {
            if (config.TradeMode == StoreTradeMode.BuyOnly)
                return false;

            if (rule.Offer == null)
                return false;

            if (!rule.Offer.Enabled)
                return false;

            if (rule.Offer.Amount <= 0)
                return false;

            return true;
        }

        private static bool CanCreateOrder(StoreBlockConfig config, StoreItemRule rule)
        {
            if (config.TradeMode == StoreTradeMode.SellOnly)
                return false;

            if (rule.Order == null)
                return false;

            if (!rule.Order.Enabled)
                return false;

            if (rule.Order.Amount <= 0)
                return false;

            return true;
        }
    }
}
