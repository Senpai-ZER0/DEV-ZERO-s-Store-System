using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.ShipOffers;
using ZeroStoreSystem.ShipOffers.Models;

namespace ZeroStoreSystem.UI.Admin
{
    public enum StoreEditorFilter
    {
        All,
        Components,
        Ingots,
        Ores,
        Ammo,
        Ships
    }

    public class StoreAdminEditorState
    {
        public string SearchText = string.Empty;
        public bool ActiveOnly = false;
        public StoreEditorFilter SelectedFilter = StoreEditorFilter.All;
        public StoreBlockConfig Config = new StoreBlockConfig();
        public readonly List<StoreAdminEditorItemViewModel> Items = new List<StoreAdminEditorItemViewModel>();

        public void LoadFromBlock(IMyTerminalBlock block)
        {
            Config = StoreConfigManager.ReadBlockConfig(block);
            RebuildItems();
        }

        public void SaveToBlock(IMyTerminalBlock block)
        {
            if (block == null || Config == null)
                return;

            RemoveLegacyShipRules();
            StoreConfigManager.SaveBlockConfig(block, Config);
            RebuildItems();
        }

        public IEnumerable<StoreAdminEditorItemViewModel> GetFilteredItems()
        {
            IEnumerable<StoreAdminEditorItemViewModel> query = Items;

            switch (SelectedFilter)
            {
                case StoreEditorFilter.Components:
                    query = query.Where(x => x != null && x.Category == StoreItemCategory.Component);
                    break;
                case StoreEditorFilter.Ingots:
                    query = query.Where(x => x != null && x.Category == StoreItemCategory.Ingot);
                    break;
                case StoreEditorFilter.Ores:
                    query = query.Where(x => x != null && x.Category == StoreItemCategory.Ore);
                    break;
                case StoreEditorFilter.Ammo:
                    query = query.Where(x => x != null && x.Category == StoreItemCategory.Ammo);
                    break;
                case StoreEditorFilter.Ships:
                    query = query.Where(x => x != null && x.Category == StoreItemCategory.Ship);
                    break;
            }

            if (ActiveOnly)
                query = query.Where(x => x != null && x.IsActive);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();
                query = query.Where(x => x != null &&
                    (Contains(x.Id, search)
                    || Contains(x.ShortName, search)
                    || Contains(x.Description, search)));
            }

            return query
                .OrderBy(x => StoreItemCatalog.GetCategorySortOrder(x.Category))
                .ThenBy(x => x.IsVanilla ? 0 : 1)
                .ThenBy(x => x.ShortName)
                .ThenBy(x => x.Id);
        }

        public StoreAdminEditorItemViewModel GetItemById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (item != null && string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return null;
        }

        public void RebuildItems()
        {
            Items.Clear();

            if (Config == null)
                Config = new StoreBlockConfig();

            if (Config.ItemRules == null)
                Config.ItemRules = new List<StoreItemRule>();

            // Ships are catalog-only for now and should not create regular ItemRules.
            foreach (var catalogItem in StoreItemCatalog.EnumerateCatalogItems())
            {
                if (catalogItem.Category == StoreItemCategory.Ship)
                    continue;

                EnsureRule(catalogItem.Id);
            }

            for (int i = 0; i < Config.ItemRules.Count; i++)
            {
                var rule = Config.ItemRules[i];
                if (rule == null || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                var category = StoreItemCatalog.GetCategory(rule.Id);

                // Do not render legacy ship token rules here; real ship entries are added below.
                if (category == StoreItemCategory.Ship)
                    continue;

                Items.Add(new StoreAdminEditorItemViewModel
                {
                    Id = rule.Id,
                    ShortName = GetShortName(rule.Id),
                    Description = rule.Id,
                    Rule = rule,
                    Category = category,
                    IsVanilla = StoreItemCatalog.IsVanilla(rule.Id)
                });
            }

            foreach (var ship in ShipStoreOfferCatalog.GetOffers())
            {
                if (ship == null || string.IsNullOrWhiteSpace(ship.Id))
                    continue;

                Items.Add(new StoreAdminEditorItemViewModel
                {
                    Id = ship.Id,
                    ShortName = string.IsNullOrWhiteSpace(ship.DisplayName) ? ship.PrefabSubtypeId : ship.DisplayName,
                    Description = ship.Description,
                    Rule = null,
                    ShipOffer = ship,
                    Category = StoreItemCategory.Ship,
                    IsVanilla = ship.IsVanilla
                });
            }
        }

        private void RemoveLegacyShipRules()
        {
            if (Config == null || Config.ItemRules == null || Config.ItemRules.Count == 0)
                return;

            for (int i = Config.ItemRules.Count - 1; i >= 0; i--)
            {
                var rule = Config.ItemRules[i];
                if (rule == null || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                ShipStoreOfferDefinition offer;
                if (ShipStoreOfferCatalog.TryGetById(rule.Id, out offer))
                    Config.ItemRules.RemoveAt(i);
            }
        }

        private StoreItemRule EnsureRule(string id)
        {
            for (int i = 0; i < Config.ItemRules.Count; i++)
            {
                var rule = Config.ItemRules[i];
                if (rule != null && string.Equals(rule.Id, id, StringComparison.OrdinalIgnoreCase))
                    return rule;
            }

            var created = StoreItemCatalog.CreateDefaultRule(id);
            Config.ItemRules.Add(created);
            return created;
        }

        private static string GetShortName(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;

            int slash = id.LastIndexOf('/');
            if (slash >= 0 && slash < id.Length - 1)
                return id.Substring(slash + 1);

            return id;
        }

        private static bool Contains(string haystack, string needle)
        {
            return !string.IsNullOrWhiteSpace(haystack) && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
