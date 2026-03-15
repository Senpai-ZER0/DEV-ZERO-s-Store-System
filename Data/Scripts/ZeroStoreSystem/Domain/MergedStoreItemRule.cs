using ZeroStoreSystem.Config.Models;

namespace ZeroStoreSystem.Domain
{
    public class MergedStoreItemRule
    {
        public string Id = string.Empty;
        public bool Allowed = true;
        public bool ForceInclude = false;
        public StoreOfferRule Offer = new StoreOfferRule();
        public StoreOrderRule Order = new StoreOrderRule();
    }
}
