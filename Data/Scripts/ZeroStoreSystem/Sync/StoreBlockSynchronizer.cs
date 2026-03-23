using System;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.Definitions;
using ZeroStoreSystem.Core;
using VRageMath;
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

            Action<int, int, long, long, long> callback = null;
            if (IsPrefabOffer(entry.ItemId))
                callback = (amountSold, amountRemaining, totalPrice, ownerOfBlock, buyerSeller) => OnPrefabTransaction(store, entry.ItemId, amountSold, totalPrice, buyerSeller);

            long id;
            var itemData = new MyStoreItemData(entry.ItemId, entry.Amount, finalPrice, callback, null);
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

        private bool IsPrefabOffer(MyDefinitionId itemId)
        {
            try
            {
                return MyDefinitionManager.Static.GetPrefabDefinition(itemId.SubtypeName) != null;
            }
            catch
            {
                return false;
            }
        }

        private void OnPrefabTransaction(Sandbox.ModAPI.IMyStoreBlock store, MyDefinitionId itemId, int amountSold, long totalPrice, long buyerIdentityId)
        {
            try
            {
                var prefab = MyDefinitionManager.Static.GetPrefabDefinition(itemId.SubtypeName);
                if (prefab == null)
                    return;

                var player = FindPlayerByIdentity(buyerIdentityId);
                if (player == null || player.Character == null)
                {
                    RefundPlayer(buyerIdentityId, totalPrice, "Buyer player or character not found for prefab purchase.");
                    return;
                }

                if (!RemovePurchasedToken(player, itemId, amountSold))
                {
                    RefundPlayer(buyerIdentityId, totalPrice, "Purchased prefab token not found in buyer inventory.");
                    return;
                }

                Vector3D spawnPos;
                Vector3D forwardDir;
                Vector3D upDir;
                if (!TryFindPrefabSpawn(store, player, prefab, out spawnPos, out forwardDir, out upDir))
                {
                    RefundPlayer(buyerIdentityId, totalPrice, "No valid spawn position found for prefab purchase.");
                    return;
                }

                var options = SpawningOptions.RotateFirstCockpitTowardsDirection | SpawningOptions.SetAuthorship | SpawningOptions.UseOnlyWorldMatrix;
                float naturalGravityInterference;
                MyAPIGateway.Physics.CalculateNaturalGravityAt(spawnPos, out naturalGravityInterference);

                if (naturalGravityInterference != 0f)
                    MyVisualScriptLogicProvider.SpawnPrefabInGravity(itemId.SubtypeName, spawnPos, forwardDir, ownerId: player.IdentityId, spawningOptions: options);
                else
                    MyVisualScriptLogicProvider.SpawnPrefab(itemId.SubtypeName, spawnPos, forwardDir, upDir, ownerId: player.IdentityId, spawningOptions: options);

                MyVisualScriptLogicProvider.AddGPS(itemId.SubtypeName, itemId.SubtypeName, spawnPos, Color.Green, disappearsInS: 0, playerId: player.IdentityId);
                Log.Info("Prefab spawned from store purchase: prefab=" + itemId.SubtypeName + ", buyer=" + buyerIdentityId + ", price=" + totalPrice + ", pos=" + spawnPos);
            }
            catch (Exception e)
            {
                RefundPlayer(buyerIdentityId, totalPrice, "Prefab purchase failed: " + e.Message);
                Log.Error("OnPrefabTransaction failed for " + itemId + ": " + e);
            }
        }

        private IMyPlayer FindPlayerByIdentity(long identityId)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(players);
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player != null && player.IdentityId == identityId)
                    return player;
            }
            return null;
        }

        private bool RemovePurchasedToken(IMyPlayer player, MyDefinitionId itemId, int amountSold)
        {
            try
            {
                if (player == null || player.Character == null)
                    return false;

                var inventory = player.Character.GetInventory();
                if (inventory == null)
                    return false;

                inventory.RemoveItemsOfType((VRage.MyFixedPoint)amountSold, itemId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryFindPrefabSpawn(Sandbox.ModAPI.IMyStoreBlock store, IMyPlayer player, MyPrefabDefinition prefab, out Vector3D spawnPos, out Vector3D forwardDir, out Vector3D upDir)
        {
            spawnPos = Vector3D.Zero;
            forwardDir = Vector3D.Forward;
            upDir = Vector3D.Up;

            if (player == null || player.Character == null)
                return false;

            var position = player.GetPosition() + (player.Character.WorldMatrix.Forward * 100d);
            float naturalGravityInterference;
            MyAPIGateway.Physics.CalculateNaturalGravityAt(position, out naturalGravityInterference);

            if (naturalGravityInterference != 0f)
            {
                var planet = MyGamePruningStructure.GetClosestPlanet(position);
                if (planet == null)
                    return false;

                var surfacePosition = planet.GetClosestSurfacePointGlobal(position);
                upDir = Vector3D.Normalize(surfacePosition - planet.PositionComp.GetPosition());
                forwardDir = Vector3D.CalculatePerpendicularVector(upDir);

                var freePlace = MyEntities.FindFreePlace(surfacePosition, (float)prefab.BoundingSphere.Radius);
                if (!freePlace.HasValue)
                    return false;

                spawnPos = freePlace.Value;
                return true;
            }
            else
            {
                var freePlace = MyEntities.FindFreePlace(position, (float)prefab.BoundingSphere.Radius);
                if (!freePlace.HasValue)
                    return false;

                spawnPos = freePlace.Value;
                forwardDir = Vector3D.Forward;
                upDir = Vector3D.Up;
                return true;
            }
        }

        private void RefundPlayer(long buyerIdentityId, long totalPrice, string reason)
        {
            try
            {
                var player = FindPlayerByIdentity(buyerIdentityId);
                if (player != null)
                    player.RequestChangeBalance(totalPrice);
            }
            catch
            {
            }

            Log.Error(reason + " Buyer=" + buyerIdentityId + ", refund=" + totalPrice);
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
