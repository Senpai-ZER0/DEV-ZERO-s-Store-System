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

        private static string GetBlockName(IMyCubeBlock block)
        {
            var terminalBlock = block as IMyTerminalBlock;
            if (terminalBlock != null)
                return terminalBlock.CustomName;

            return block != null ? block.DisplayNameText : "<null>";
        }

        public StoreGenerationResult Regenerate(IMyCubeBlock block, GlobalStoreConfig globalConfig)
        {
            if (block == null)
            {
                Log.Error("Regenerate called with null block");
                return new StoreGenerationResult();
            }

            Log.Info("Store regenerate started for '" + GetBlockName(block) + "'");

            var terminalBlock = block as IMyTerminalBlock;
            var config = terminalBlock != null
                ? StoreConfigManager.ReadBlockConfig(terminalBlock)
                : new StoreBlockConfig();

            var result = _generator.Generate(config, globalConfig ?? new GlobalStoreConfig());
            _synchronizer.Apply(block, result);

            Log.Info("Store regenerate finished for '" + GetBlockName(block) + "' with TradeMode=" + config.TradeMode);
            return result;
        }
    }
}
