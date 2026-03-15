using System.Collections.Generic;
using Sandbox.ModAPI;
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

        public static void GetRegisteredTerminalBlocks(List<IMyTerminalBlock> result)
        {
            if (result == null)
                return;

            result.Clear();

            if (MyAPIGateway.Entities == null)
                return;

            foreach (var id in KnownIds)
            {
                IMyEntity entity;
                if (!MyAPIGateway.Entities.TryGetEntityById(id, out entity))
                    continue;

                var terminal = entity as IMyTerminalBlock;
                if (terminal != null && !terminal.MarkedForClose)
                    result.Add(terminal);
            }
        }
    }
}
