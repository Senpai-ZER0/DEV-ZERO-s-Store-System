using System.Collections.Generic;

namespace ZeroStoreSystem.Domain
{
    public class StoreGenerationResult
    {
        public string ProfileId = "neutral";
        public readonly List<string> Diagnostics = new List<string>();
        public int OfferCount;
        public int OrderCount;
    }
}
