using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Sync;

namespace ZeroStoreSystem.Blocks
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_StoreBlock), false, "NpcStoreBlock")]
    public class NpcStoreBlockLogic : MyGameLogicComponent
    {
        private IMyCubeBlock _block;
        private bool _initialized;
        private bool _regenQueued;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            _block = Entity as IMyCubeBlock;
            if (_block == null)
                return;

            Entity.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            Log.Info("NpcStoreBlockLogic.Init: entity=" + _block.EntityId);
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            if (_block == null || _initialized)
                return;

            _initialized = true;
            NpcStoreRegistry.Register(Entity);

            var terminalBlock = _block as IMyTerminalBlock;
            if (terminalBlock != null && string.IsNullOrWhiteSpace(terminalBlock.CustomData))
            {
                StoreConfigManager.WriteDefaultBlockConfig(terminalBlock);
                Log.Info("Default CustomData written for entity=" + _block.EntityId);
            }

            _regenQueued = true;
            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            Log.Info("NpcStoreBlockLogic initialized: entity=" + _block.EntityId);
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            if (_block == null || !_regenQueued)
                return;

            _regenQueued = false;

            if (MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            var globalConfig = ZeroStoreSystem.Session.EconomySession.Instance != null
                ? ZeroStoreSystem.Session.EconomySession.Instance.GlobalConfig
                : new GlobalStoreConfig();

            var refreshService = new StoreRefreshService();
            refreshService.Regenerate(_block, globalConfig);
        }

        public override void Close()
        {
            if (Entity != null)
                NpcStoreRegistry.Unregister(Entity);

            if (_block != null)
                Log.Info("NpcStoreBlockLogic.Close: entity=" + _block.EntityId);

            base.Close();
        }
    }
}
