using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace ZeroStoreSystem.Core
{
    public static class AdminAccess
    {
        public static bool IsLocalAdminOrHigher()
        {
            if (MyAPIGateway.Session == null)
                return false;

            try
            {
                if (MyAPIGateway.Session.HasCreativeRights)
                    return true;
            }
            catch { }

            try
            {
                if (MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.MultiplayerActive)
                    return true;
            }
            catch { }

            var player = MyAPIGateway.Session.Player;
            if (player == null)
                return false;

            try
            {
                if (player.PromoteLevel == MyPromoteLevel.Admin ||
                    player.PromoteLevel == MyPromoteLevel.Owner ||
                    player.PromoteLevel == MyPromoteLevel.SpaceMaster ||
                    player.PromoteLevel == MyPromoteLevel.Scripter)
                    return true;
            }
            catch { }

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
