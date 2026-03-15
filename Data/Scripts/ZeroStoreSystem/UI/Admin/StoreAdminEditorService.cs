using VRage.Game.ModAPI;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Sync;

namespace ZeroStoreSystem.UI.Admin
{
    public static class StoreAdminEditorService
    {
        public static bool TryOpenForLocalAdmin(IMyTerminalBlock block)
        {
            if (block == null)
                return false;

            if (!AdminAccess.IsLocalAdminOrHigher())
            {
                Log.Error("Admin editor access denied for non-admin user.");
                return false;
            }

            return true;
        }

        public static bool SaveToBlockOnly(IMyTerminalBlock block, StoreAdminEditorState state)
        {
            if (block == null || state == null)
                return false;

            if (!AdminAccess.IsLocalAdminOrHigher())
                return false;

            state.SaveToBlock(block);
            return true;
        }

        public static bool SaveAndRegenerate(IMyTerminalBlock block, StoreAdminEditorState state)
        {
            if (!SaveToBlockOnly(block, state))
                return false;

            var cube = block as IMyCubeBlock;
            if (cube == null)
                return false;

            var refresh = new StoreRefreshService();
            refresh.Regenerate(cube, ZeroStoreSystem.Session.EconomySession.Instance != null ? ZeroStoreSystem.Session.EconomySession.Instance.GlobalConfig : null);
            return true;
        }
    }
}
