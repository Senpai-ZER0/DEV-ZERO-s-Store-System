using System.Collections.Generic;
using ZeroStoreSystem.Core;

namespace ZeroStoreSystem.Config.Models
{
    public class StoreBlockConfig
    {
        public bool Enabled = true;
        public bool UseAutoProfile = true;
        public bool ManualRegenerationOnly = false;
        public string ProfileId = string.Empty;
        public StoreTradeMode TradeMode = StoreTradeMode.BuyAndSell;
        public int RefreshIntervalSeconds = 0;
        public List<StoreItemRule> ItemRules = new List<StoreItemRule>();
    }
}
