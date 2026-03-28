using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Session;
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

            var editor = EconomySession.Instance != null ? EconomySession.Instance.AdminEditor : null;
            return editor != null && editor.OpenForLocalAdmin();
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
            if (block == null || state == null)
                return false;
            if (!AdminAccess.IsLocalAdminOrHigher())
                return false;

            if (state.Config != null && state.Config.UseAutoProfile)
                StoreConfigManager.RebuildAutoProfileRules(block, state.Config);

            state.SaveToBlock(block);

            var cube = block as IMyCubeBlock;
            if (cube != null)
            {
                var refresh = new StoreRefreshService();
                refresh.Regenerate(cube, EconomySession.Instance != null ? EconomySession.Instance.GlobalConfig : null);
            }

            return true;
        }
    }
}
