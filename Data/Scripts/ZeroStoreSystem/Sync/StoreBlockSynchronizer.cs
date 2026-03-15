using System;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Domain;

namespace ZeroStoreSystem.Sync
{
    public class StoreBlockSynchronizer
    {
        public bool Apply(IMyCubeBlock cubeBlock, StoreGenerationResult result)
        {
            if (cubeBlock == null)
            {
                Log.Error("StoreBlockSynchronizer.Apply: cubeBlock is null");
                return false;
            }

            if (result == null)
            {
                Log.Error("StoreBlockSynchronizer.Apply: result is null");
                return false;
            }

            var store = cubeBlock as IMyStoreBlock;
            var terminalBlock = cubeBlock as IMyTerminalBlock;
            var blockName = terminalBlock != null ? terminalBlock.CustomName : cubeBlock.DisplayNameText;

            if (store == null)
            {
                Log.Error("Block '" + blockName + "' is not IMyStoreBlock");
                return false;
            }

            try
            {
                ClearExistingStoreEntries(store);

                foreach (var offer in result.Offers)
                    AddOffer(store, offer);

                foreach (var order in result.Orders)
                    AddOrder(store, order);

                Log.Info("Store synchronized for '" + blockName + "': offers=" + result.Offers.Count + ", orders=" + result.Orders.Count);
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Failed to synchronize store '" + blockName + "': " + e);
                return false;
            }
        }

        private void ClearExistingStoreEntries(IMyStoreBlock store)
        {
            Log.Info("ClearExistingStoreEntries called");
        }

        private void AddOffer(IMyStoreBlock store, StoreEntryPlan entry)
        {
            if (entry == null)
                return;

            Log.Info("AddOffer: " + entry.ItemId + " x" + entry.Amount + " @ " + entry.PricePerUnit);
        }

        private void AddOrder(IMyStoreBlock store, StoreEntryPlan entry)
        {
            if (entry == null)
                return;

            Log.Info("AddOrder: " + entry.ItemId + " x" + entry.Amount + " @ " + entry.PricePerUnit);
        }
    }
}
