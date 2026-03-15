using System;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Definitions;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Domain;

namespace ZeroStoreSystem.Sync
{
    public class StoreBlockSynchronizer
    {
        private readonly List<Sandbox.ModAPI.Ingame.MyStoreQueryItem> _storeItems = new List<Sandbox.ModAPI.Ingame.MyStoreQueryItem>();
        private readonly Dictionary<MyDefinitionId, int> _componentMinimalPrice = new Dictionary<MyDefinitionId, int>();
        private readonly Dictionary<MyDefinitionId, int> _blockMinimalPrice = new Dictionary<MyDefinitionId, int>();

        public bool Apply(VRage.Game.ModAPI.IMyCubeBlock cubeBlock, StoreGenerationResult result)
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

            var store = cubeBlock as Sandbox.ModAPI.IMyStoreBlock;
            var terminalBlock = cubeBlock as Sandbox.ModAPI.IMyTerminalBlock;
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

        private void ClearExistingStoreEntries(Sandbox.ModAPI.IMyStoreBlock store)
        {
            _storeItems.Clear();
            store.GetPlayerStoreItems(_storeItems);

            foreach (var item in _storeItems)
                store.CancelStoreItem(item.Id);

            Log.Info("ClearExistingStoreEntries: removed=" + _storeItems.Count);
            _storeItems.Clear();
        }

        private void AddOffer(Sandbox.ModAPI.IMyStoreBlock store, StoreEntryPlan entry)
        {
            if (entry == null)
                return;

            int finalPrice = ComputeValidOfferPrice(entry.ItemId, entry.PricePerUnit);
            Log.Info("AddOffer pricing: item=" + entry.ItemId + ", requested=" + entry.PricePerUnit + ", final=" + finalPrice);

            long id;
            var itemData = new MyStoreItemData(entry.ItemId, entry.Amount, finalPrice, null, null);
            var result = store.InsertOffer(itemData, out id);

            if (result != Sandbox.ModAPI.Ingame.MyStoreInsertResults.Success)
            {
                Log.Error("AddOffer failed for " + entry.ItemId + ": " + result);
                return;
            }

            try
            {
                var currentAmount = MyVisualScriptLogicProvider.GetEntityInventoryItemAmount(store.Name, entry.ItemId);
                if (currentAmount > 0)
                    MyVisualScriptLogicProvider.RemoveFromEntityInventory(store.Name, entry.ItemId, currentAmount);

                MyVisualScriptLogicProvider.AddToInventory(store.Name, entry.ItemId, entry.Amount);
            }
            catch (Exception e)
            {
                Log.Error("AddOffer inventory sync failed for " + entry.ItemId + ": " + e);
            }

            Log.Info("AddOffer success: " + entry.ItemId + " x" + entry.Amount + " @ " + finalPrice + ", id=" + id);
        }

        private int ComputeValidOfferPrice(MyDefinitionId itemId, int requestedPrice)
        {
            int minimalPrice = 0;
            CalculateItemMinimalPrice(itemId, 1f, ref minimalPrice);

            int finalPrice = requestedPrice;
            if (minimalPrice > 0)
            {
                // repeat the old mod's safe behavior: sell slightly above the calculated minimum
                finalPrice = Math.Max(requestedPrice, (int)Math.Ceiling(minimalPrice * 1.10));
            }
            else
            {
                finalPrice = Math.Max(requestedPrice, 1000);
            }

            Log.Info("ComputeValidOfferPrice: item=" + itemId + ", calculatedMinimal=" + minimalPrice + ", requested=" + requestedPrice + ", final=" + finalPrice);
            return finalPrice;
        }

        private void CalculateItemMinimalPrice(MyDefinitionId itemId, float baseCostProductionSpeedMultiplier, ref int minimalPrice)
        {
            minimalPrice = 0;

            MyPhysicalItemDefinition physicalDef;
            if (MyDefinitionManager.Static.TryGetDefinition(itemId, out physicalDef) && physicalDef.MinimalPricePerUnit != -1)
            {
                minimalPrice += physicalDef.MinimalPricePerUnit;
                return;
            }

            MyBlueprintDefinitionBase blueprintDef;
            if (!MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(itemId, out blueprintDef))
                return;

            float efficiencyDivisor = (physicalDef != null && physicalDef.IsIngot) ? 1f : MyAPIGateway.Session.AssemblerEfficiencyMultiplier;
            int prerequisitesCost = 0;

            foreach (var prerequisite in blueprintDef.Prerequisites)
            {
                int prerequisiteCost = 0;
                CalculateItemMinimalPrice(prerequisite.Id, baseCostProductionSpeedMultiplier, ref prerequisiteCost);
                float amountAdjusted = (float)prerequisite.Amount / efficiencyDivisor;
                prerequisitesCost += (int)(prerequisiteCost * amountAdjusted);
            }

            float speedDivisor = (physicalDef != null && physicalDef.IsIngot)
                ? MyAPIGateway.Session.RefinerySpeedMultiplier
                : MyAPIGateway.Session.AssemblerSpeedMultiplier;

            for (int i = 0; i < blueprintDef.Results.Length; i++)
            {
                var result = blueprintDef.Results[i];
                if (result.Id != itemId)
                    continue;

                float resultAmount = (float)result.Amount;
                if (resultAmount == 0f)
                    return;

                float productionFactor = 1f + (float)Math.Log(blueprintDef.BaseProductionTimeInSeconds + 1f) * baseCostProductionSpeedMultiplier / speedDivisor;
                minimalPrice += (int)(prerequisitesCost * (1f / resultAmount) * productionFactor);
                return;
            }
        }

        private void AddOrder(Sandbox.ModAPI.IMyStoreBlock store, StoreEntryPlan entry)
        {
            if (entry == null)
                return;

            long id;
            var itemData = new MyStoreItemData(entry.ItemId, entry.Amount, entry.PricePerUnit, null, null);
            var result = store.InsertOrder(itemData, out id);

            if (result != Sandbox.ModAPI.Ingame.MyStoreInsertResults.Success)
            {
                Log.Error("AddOrder failed for " + entry.ItemId + ": " + result);
                return;
            }

            Log.Info("AddOrder success: " + entry.ItemId + " x" + entry.Amount + " @ " + entry.PricePerUnit + ", id=" + id);
        }
    }
}
