using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace ZeroStoreSystem.Core
{
    public static class AdminAccess
    {
        public static bool IsLocalAdminOrHigher()
        {
            if (MyAPIGateway.Session == null)
                return false;

            var player = MyAPIGateway.Session.Player;

            // Singleplayer / local offline worlds should always be treated as admin-capable.
            if (player != null && MyAPIGateway.Multiplayer != null && !MyAPIGateway.Multiplayer.MultiplayerActive)
                return true;

            if (MyAPIGateway.Session.HasCreativeRights)
                return true;

            if (player == null)
                return false;

            try
            {
                if (MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
                    return true;
            }
            catch
            {
            }

            try
            {
                return player.PromoteLevel != 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
