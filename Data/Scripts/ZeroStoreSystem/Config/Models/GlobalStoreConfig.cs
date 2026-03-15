using System.Collections.Generic;
using ZeroStoreSystem.Core;

namespace ZeroStoreSystem.Config.Models
{
    public class GlobalStoreConfig
    {
        public string DefaultProfileId = "neutral";
        public bool AllowModdedItems = true;
        public bool AutoRegisterNpcStores = true;
        public int RefreshIntervalTicks = 36000;
        public StoreTradeMode DefaultTradeMode = StoreTradeMode.BuyAndSell;
        public List<StoreItemRule> GlobalItemRules = new List<StoreItemRule>();
    }
}
