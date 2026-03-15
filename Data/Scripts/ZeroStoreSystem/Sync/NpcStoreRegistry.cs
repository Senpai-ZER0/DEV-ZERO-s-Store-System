using System.Collections.Generic;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Core;

namespace ZeroStoreSystem.Sync
{
    public static class NpcStoreRegistry
    {
        private static readonly HashSet<long> KnownIds = new HashSet<long>();

        public static void Register(IMyEntity entity)
        {
            if (entity == null)
                return;

            if (KnownIds.Add(entity.EntityId))
                Log.Info("Npc store registered: entity=" + entity.EntityId);
        }

        public static void Unregister(IMyEntity entity)
        {
            if (entity == null)
                return;

            if (KnownIds.Remove(entity.EntityId))
                Log.Info("Npc store unregistered: entity=" + entity.EntityId);
        }

        public static int Count
        {
            get { return KnownIds.Count; }
        }
    }
}
