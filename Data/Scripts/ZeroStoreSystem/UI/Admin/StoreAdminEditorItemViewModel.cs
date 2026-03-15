using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;

namespace ZeroStoreSystem.UI.Admin
{
    public class StoreAdminEditorItemViewModel
    {
        public string Id;
        public string ShortName;
        public StoreItemRule Rule;
        public StoreItemCategory Category;
        public bool IsVanilla;

        public bool IsActive
        {
            get
            {
                if (Rule == null)
                    return false;

                if (!Rule.Allowed)
                    return false;

                bool offerActive = Rule.Offer != null && Rule.Offer.Enabled && Rule.Offer.Amount > 0;
                bool orderActive = Rule.Order != null && Rule.Order.Enabled && Rule.Order.Amount > 0;
                return offerActive || orderActive;
            }
        }
    }
}
