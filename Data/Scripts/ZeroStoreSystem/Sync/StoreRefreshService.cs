using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Domain;
using ZeroStoreSystem.Generation;

namespace ZeroStoreSystem.Sync
{
    public class StoreRefreshService
    {
        private readonly StoreInventoryGenerator _generator = new StoreInventoryGenerator();
        private readonly StoreBlockSynchronizer _synchronizer = new StoreBlockSynchronizer();

        public StoreGenerationResult Regenerate(IMyCubeBlock block, GlobalStoreConfig globalConfig)
        {
            if (block == null)
            {
                Log.Error("Regenerate called with null block");
                return new StoreGenerationResult();
            }

            var terminalBlock = block as IMyTerminalBlock;
            var blockName = terminalBlock != null ? terminalBlock.CustomName : block.DisplayNameText;

            Log.Info("Store regenerate started for '" + blockName + "'");

            var config = terminalBlock != null
                ? StoreConfigManager.ReadBlockConfig(terminalBlock)
                : new StoreBlockConfig();

            var result = _generator.Generate(config, globalConfig);
            _synchronizer.Apply(block, result);

            Log.Info("Store regenerate finished for '" + blockName + "' with TradeMode=" + config.TradeMode);
            return result;
        }
    }
}
