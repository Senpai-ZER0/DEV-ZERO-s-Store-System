using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRage.Game.ModAPI.Ingame.Utilities;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Session;
using ZeroStoreSystem.Sync;

namespace ZeroStoreSystem.Blocks
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_StoreBlock), false, "NpcStoreBlock")]
    public class NpcStoreBlockLogic : MyGameLogicComponent
    {
        private IMyStoreBlock _storeBlock;
        private readonly MyIni _ini = new MyIni();
        private readonly StoreRefreshService _refreshService = new StoreRefreshService();
        private StoreBlockConfig _localConfig = new StoreBlockConfig();
        private bool _initialized;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            _storeBlock = Entity as IMyStoreBlock;

            if (_storeBlock == null)
                return;

            NpcStoreRegistry.Register(Entity);
            EnsureDefaultConfig();
            _initialized = true;

            if (MyAPIGateway.Session != null && MyAPIGateway.Session.IsServer)
            {
                MyLog.Default.WriteLine("[ZERO Store System] NpcStoreBlock initialized for entity " + Entity.EntityId);
            }
        }

        public override void Close()
        {
            NpcStoreRegistry.Unregister(Entity);
            base.Close();
        }

        public override void UpdateAfterSimulation100()
        {
            if (!_initialized || _storeBlock == null || MyAPIGateway.Session == null || !MyAPIGateway.Session.IsServer)
                return;

            // Temporary bootstrap behavior:
            // runs once after init, then the session/services can take over in later iterations.
            if (_initialized)
            {
                _refreshService.Regenerate(_localConfig, EconomySession.Instance != null ? EconomySession.Instance.GlobalConfig : new GlobalStoreConfig());
                _initialized = false;
            }
        }

        private void EnsureDefaultConfig()
        {
            if (!string.IsNullOrWhiteSpace(_storeBlock.CustomData))
                return;

            _ini.Clear();
            _ini.Set("Store", "Enabled", true);
            _ini.Set("Store", "UseAutoProfile", true);
            _ini.Set("Store", "ProfileId", "");
            _ini.Set("Store", "TradeMode", "BuyAndSell");
            _storeBlock.CustomData = _ini.ToString();
        }
    }
}
