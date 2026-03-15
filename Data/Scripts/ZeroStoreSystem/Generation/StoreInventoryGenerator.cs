using VRage.Game;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Domain;

namespace ZeroStoreSystem.Generation
{
    public class StoreInventoryGenerator
    {
        public StoreGenerationResult Generate(StoreBlockConfig blockConfig, GlobalStoreConfig globalConfig)
        {
            var result = new StoreGenerationResult();

            if (blockConfig == null || !blockConfig.Enabled)
            {
                result.Diagnostics.Add("Block config disabled or null.");
                return result;
            }

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
                    PricePerUnit = 15
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

            result.Diagnostics.Add("Test inventory generated.");
            return result;
        }
    }
}
