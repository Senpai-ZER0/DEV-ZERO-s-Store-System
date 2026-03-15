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

        public static void GetRegisteredTerminalBlocks(List<IMyTerminalBlock> output)
        {
            if (output == null)
                return;

            output.Clear();
            if (MyAPIGateway.Entities == null)
                return;

            List<long> stale = null;
            foreach (var id in KnownIds)
            {
                IMyEntity entity;
                if (!MyAPIGateway.Entities.TryGetEntityById(id, out entity) || entity == null || entity.MarkedForClose)
                {
                    if (stale == null)
                        stale = new List<long>();
                    stale.Add(id);
                    continue;
                }

                var terminal = entity as IMyTerminalBlock;
                if (terminal != null)
                    output.Add(terminal);
            }

            if (stale != null)
            {
                for (int i = 0; i < stale.Count; i++)
                    KnownIds.Remove(stale[i]);
            }
        }
    }
}
