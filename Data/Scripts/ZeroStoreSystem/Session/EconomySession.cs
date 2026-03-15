using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Utils;
using ZeroStoreSystem.Config.Models;

namespace ZeroStoreSystem.Session
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class EconomySession : MySessionComponentBase
    {
        public static EconomySession Instance { get; private set; }
        public GlobalStoreConfig GlobalConfig { get; private set; }

        public override void LoadData()
        {
            Instance = this;
            GlobalConfig = new GlobalStoreConfig();
            MyLog.Default.WriteLine("[ZERO Store System] Session loaded.");
        }

        protected override void UnloadData()
        {
            MyLog.Default.WriteLine("[ZERO Store System] Session unloaded.");
            GlobalConfig = null;
            Instance = null;
        }
    }
}
