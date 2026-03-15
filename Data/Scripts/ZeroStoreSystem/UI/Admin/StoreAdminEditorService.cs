using Sandbox.ModAPI;
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

            Log.Info("Admin editor requested for '" + block.CustomName + "'. RHF client UI is not wired yet; groundwork only.");
            return true;
        }

        public static bool SaveAndRegenerate(IMyTerminalBlock block, StoreAdminEditorState state)
        {
            if (block == null || state == null)
                return false;

            if (!AdminAccess.IsLocalAdminOrHigher())
                return false;

            state.SaveToBlock(block);
            var cube = block as IMyCubeBlock;
            if (cube != null)
            {
                var refresh = new StoreRefreshService();
                refresh.Regenerate(cube, null);
            }

            return true;
        }
    }
}
