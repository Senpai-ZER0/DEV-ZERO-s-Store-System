using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Sync;

namespace ZeroStoreSystem.Blocks
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_StoreBlock), false, "NpcStoreBlock")]
    public class NpcStoreBlockLogic : MyGameLogicComponent
    {
        private IMyCubeBlock _block;
        private IMyTerminalBlock _terminalBlock;
        private bool _regenQueued;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            _block = Entity as IMyCubeBlock;
            _terminalBlock = Entity as IMyTerminalBlock;

            Log.Info("NpcStoreBlockLogic.Init: entity=" + (Entity != null ? Entity.EntityId.ToString() : "0")
                + ", cube=" + (_block != null)
                + ", terminal=" + (_terminalBlock != null));

            if (_block == null)
                return;

            NpcStoreRegistry.Register(Entity);

            if (_terminalBlock != null)
            {
                if (string.IsNullOrWhiteSpace(_terminalBlock.CustomData))
                {
                    StoreConfigManager.WriteDefaultBlockConfig(_terminalBlock);
                    Log.Info("Default CustomData written");
                }
                else
                {
                    Log.Info("CustomData already exists");
                }
            }
            else
            {
                Log.Error("Terminal block cast failed");
            }

            _regenQueued = true;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;

            Log.Info("NpcStoreBlockLogic.Init finished");
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            if (_block == null || !_regenQueued)
                return;

            if (MyAPIGateway.Multiplayer != null && !MyAPIGateway.Multiplayer.IsServer)
                return;

            _regenQueued = false;

            Log.Info("Calling StoreRefreshService.RegenerateStore");
            var refreshService = new StoreRefreshService();
            refreshService.Regenerate(_block, null);
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
