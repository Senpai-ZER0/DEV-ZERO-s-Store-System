using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Sync;

namespace ZeroStoreSystem.Blocks
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_StoreBlock), false, "NpcStoreBlock")]
    public class NpcStoreBlockLogic : MyGameLogicComponent
    {
        private const int FramesPerCheck = 10;
        private const int MinRefreshIntervalFrames = FramesPerCheck;

        private IMyCubeBlock _block;
        private IMyTerminalBlock _terminalBlock;
        private IMyFunctionalBlock _functionalBlock;
        private readonly StoreRefreshService _refreshService = new StoreRefreshService();

        private bool _refreshQueued;
        private string _queuedReason;
        private string _lastCustomData;
        private bool _lastEnabledState;
        private bool _lastWorkingState;
        private int _framesSinceRefresh;
        private StoreBlockConfig _lastConfig;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            _block = Entity as IMyCubeBlock;
            _terminalBlock = Entity as IMyTerminalBlock;
            _functionalBlock = Entity as IMyFunctionalBlock;

            Log.Info("NpcStoreBlockLogic.Init: entity=" + (Entity != null ? Entity.EntityId.ToString() : "0")
                + ", cube=" + (_block != null)
                + ", terminal=" + (_terminalBlock != null)
                + ", functional=" + (_functionalBlock != null));

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

                _lastCustomData = _terminalBlock.CustomData ?? string.Empty;
                _lastConfig = StoreConfigManager.ReadBlockConfig(_terminalBlock);
            }
            else
            {
                Log.Error("Terminal block cast failed");
                _lastCustomData = string.Empty;
                _lastConfig = new StoreBlockConfig();
            }

            _lastEnabledState = GetEnabledState();
            _lastWorkingState = GetWorkingState();
            _framesSinceRefresh = 0;

            QueueRefresh("initialization");
            NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;

            Log.Info("NpcStoreBlockLogic.Init finished");
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();

            if (_block == null)
                return;

            if (MyAPIGateway.Multiplayer != null && !MyAPIGateway.Multiplayer.IsServer)
                return;

            DetectStateChanges();
            DetectCustomDataChanges();

            _framesSinceRefresh += FramesPerCheck;
            TryPeriodicRefresh();

            if (_refreshQueued)
                ExecuteQueuedRefresh();
        }

        public override void Close()
        {
            if (Entity != null)
                NpcStoreRegistry.Unregister(Entity);

            if (_block != null)
                Log.Info("NpcStoreBlockLogic.Close: entity=" + _block.EntityId);

            base.Close();
        }

        private void DetectStateChanges()
        {
            bool enabledState = GetEnabledState();
            bool workingState = GetWorkingState();

            if (enabledState != _lastEnabledState)
            {
                _lastEnabledState = enabledState;
                QueueRefresh("enabled changed to " + enabledState);
            }

            if (workingState != _lastWorkingState)
            {
                _lastWorkingState = workingState;
                QueueRefresh("working changed to " + workingState);
            }
        }

        private void DetectCustomDataChanges()
        {
            if (_terminalBlock == null)
                return;

            string currentCustomData = _terminalBlock.CustomData ?? string.Empty;
            if (currentCustomData == _lastCustomData)
                return;

            _lastCustomData = currentCustomData;
            _lastConfig = StoreConfigManager.ReadBlockConfig(_terminalBlock);
            QueueRefresh("CustomData changed");
        }

        private void TryPeriodicRefresh()
        {
            int intervalFrames = GetRefreshIntervalFrames();
            if (intervalFrames <= 0)
                return;

            if (_framesSinceRefresh < intervalFrames)
                return;

            QueueRefresh("periodic interval elapsed");
        }

        private void ExecuteQueuedRefresh()
        {
            _refreshQueued = false;

            if (_terminalBlock != null)
            {
                _lastConfig = StoreConfigManager.ReadBlockConfig(_terminalBlock);
                _lastCustomData = _terminalBlock.CustomData ?? string.Empty;
            }

            _framesSinceRefresh = 0;
            Log.Info("Calling StoreRefreshService.RegenerateStore, reason=" + (_queuedReason ?? "<none>"));
            _refreshService.Regenerate(_block, ZeroStoreSystem.Session.EconomySession.Instance != null ? ZeroStoreSystem.Session.EconomySession.Instance.GlobalConfig : null);
            _queuedReason = null;
        }

        private void QueueRefresh(string reason)
        {
            _refreshQueued = true;
            _queuedReason = reason;
            Log.Info("Refresh queued: " + reason);
        }

        private int GetRefreshIntervalFrames()
        {
            int seconds = _lastConfig != null ? _lastConfig.RefreshIntervalSeconds : 0;
            if (seconds <= 0)
                return 0;

            int frames = seconds * 60;
            return frames < MinRefreshIntervalFrames ? MinRefreshIntervalFrames : frames;
        }

        private bool GetEnabledState()
        {
            return _functionalBlock == null || _functionalBlock.Enabled;
        }

        private bool GetWorkingState()
        {
            return _functionalBlock == null || _functionalBlock.IsWorking;
        }
    }
}
