using VRage.Game.Components;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;

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
            Log.Info("Session loaded.");
        }

        protected override void UnloadData()
        {
            Log.Info("Session unloaded.");
            GlobalConfig = null;
            Instance = null;
        }
    }
}
