using System.Collections.Generic;

namespace ZeroStoreSystem.Domain
{
    public class StoreGenerationResult
    {
        public readonly List<StoreEntryPlan> Offers = new List<StoreEntryPlan>();
        public readonly List<StoreEntryPlan> Orders = new List<StoreEntryPlan>();
        public readonly List<string> Diagnostics = new List<string>();
        public string ProfileId = "neutral";
    }
}
