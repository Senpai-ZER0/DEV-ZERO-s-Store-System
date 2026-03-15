using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Domain;

namespace ZeroStoreSystem.Generation
{
    public class StoreInventoryGenerator
    {
        public StoreGenerationResult Generate(StoreBlockConfig blockConfig, GlobalStoreConfig globalConfig)
        {
            var result = new StoreGenerationResult();
            result.ProfileId = string.IsNullOrWhiteSpace(blockConfig.ProfileId) ? globalConfig.DefaultProfileId : blockConfig.ProfileId;
            result.Diagnostics.Add("Generator skeleton initialized.");
            return result;
        }
    }
}
