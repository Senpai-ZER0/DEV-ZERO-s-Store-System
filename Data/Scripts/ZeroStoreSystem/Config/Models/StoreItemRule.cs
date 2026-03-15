namespace ZeroStoreSystem.Config.Models
{
    public class StoreItemRule
    {
        public string Id = string.Empty;
        public bool Allowed = true;
        public bool ForceInclude = false;
        public StoreOfferRule Offer = new StoreOfferRule();
        public StoreOrderRule Order = new StoreOrderRule();
    }
}
