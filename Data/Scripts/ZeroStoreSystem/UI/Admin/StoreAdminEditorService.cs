using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Session;
using ZeroStoreSystem.Sync;

namespace ZeroStoreSystem.UI.Admin
{
    public static class StoreAdminEditorService
    {
        private static string _copiedCustomData;

        public static bool HasCopiedCustomData => !string.IsNullOrWhiteSpace(_copiedCustomData);

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
            if (!SaveToBlockOnly(block, state))
                return false;

            var cube = block as IMyCubeBlock;
            if (cube != null)
            {
                var refresh = new StoreRefreshService();
                refresh.Regenerate(cube, EconomySession.Instance != null ? EconomySession.Instance.GlobalConfig : null);
            }

            return true;
        }

        public static bool CopyCurrentCustomData(StoreAdminEditorState state)
        {
            if (state == null || state.Config == null)
                return false;

            if (!AdminAccess.IsLocalAdminOrHigher())
                return false;

            _copiedCustomData = StoreConfigManager.SerializeBlockConfig(state.Config);
            return !string.IsNullOrWhiteSpace(_copiedCustomData);
        }

        public static bool PasteToBlock(IMyTerminalBlock block, StoreAdminEditorState state)
        {
            if (block == null || state == null)
                return false;

            if (!AdminAccess.IsLocalAdminOrHigher() || string.IsNullOrWhiteSpace(_copiedCustomData))
                return false;

            var parsed = StoreConfigManager.ReadBlockConfigFromText(_copiedCustomData);
            if (parsed == null)
                return false;

            block.CustomData = StoreConfigManager.SerializeBlockConfig(parsed);
            state.LoadFromText(block.CustomData);
            return true;
        }

        public static bool PasteAndRegenerate(IMyTerminalBlock block, StoreAdminEditorState state)
        {
            if (!PasteToBlock(block, state))
                return false;

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
