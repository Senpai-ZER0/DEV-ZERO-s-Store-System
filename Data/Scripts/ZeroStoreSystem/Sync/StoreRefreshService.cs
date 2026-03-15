using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Domain;
using ZeroStoreSystem.Generation;

namespace ZeroStoreSystem.Sync
{
    public class StoreRefreshService
    {
        private readonly StoreInventoryGenerator _generator = new StoreInventoryGenerator();
        private readonly StoreBlockSynchronizer _synchronizer = new StoreBlockSynchronizer();

        public StoreGenerationResult Regenerate(StoreBlockConfig blockConfig, GlobalStoreConfig globalConfig)
        {
            var result = _generator.Generate(blockConfig, globalConfig);
            _synchronizer.SyncEmpty();
            return result;
        }
    }
}
