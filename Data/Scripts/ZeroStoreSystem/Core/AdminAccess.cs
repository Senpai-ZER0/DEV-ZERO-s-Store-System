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

            if (MyAPIGateway.Session.HasCreativeRights)
                return true;

            var player = MyAPIGateway.Session.Player;
            if (player == null)
                return false;

            try
            {
                return MyAPIGateway.Session.IsUserAdmin(player.SteamUserId);
            }
            catch
            {
                return false;
            }
        }
    }
}
