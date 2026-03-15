using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace ZeroStoreSystem.Sync
{
    public static class NpcStoreRegistry
    {
        private static readonly HashSet<long> _knownIds = new HashSet<long>();

        public static void Register(IMyEntity entity)
        {
            if (entity == null)
                return;

            _knownIds.Add(entity.EntityId);
        }

        public static void Unregister(IMyEntity entity)
        {
            if (entity == null)
                return;

            _knownIds.Remove(entity.EntityId);
        }

        public static int Count
        {
            get { return _knownIds.Count; }
        }
    }
}
