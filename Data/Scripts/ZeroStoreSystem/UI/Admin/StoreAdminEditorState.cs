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
                    query = query.Where(x => x != null && x.Category == StoreItemCategory.Ships);
                    break;
            }

            if (ActiveOnly)
                query = query.Where(x => x != null && x.IsActive);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();
                query = query.Where(x => (x != null && Contains(x.Id, search))
                    || (x != null && Contains(x.ShortName, search))
                    || (x != null && Contains(x.Description, search)));
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

            int i;
            for (i = 0; i < Items.Count; i++)
            {
                StoreAdminEditorItemViewModel item = Items[i];
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
            if (Config.ShipOfferRules == null)
                Config.ShipOfferRules = new List<ShipOfferRule>();

            foreach (StoreCatalogItem catalogItem in StoreItemCatalog.EnumerateCatalogItems())
            {
                if (catalogItem == null || string.IsNullOrWhiteSpace(catalogItem.Id))
                    continue;

                if (catalogItem.Category == StoreItemCategory.Ships)
                    continue;

                EnsureRule(catalogItem.Id);
            }

            int i;
            for (i = 0; i < Config.ItemRules.Count; i++)
            {
                StoreItemRule rule = Config.ItemRules[i];
                if (rule == null || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                Items.Add(new StoreAdminEditorItemViewModel
                {
                    Id = rule.Id,
                    ShortName = GetShortName(rule.Id),
                    Description = rule.Id,
                    Rule = rule,
                    Category = StoreItemCatalog.GetCategory(rule.Id),
                    IsVanilla = StoreItemCatalog.IsVanilla(rule.Id)
                });
            }

            List<ShipStoreOfferDefinition> offers = ShipStoreOfferCatalog.GetOffers();
            if (offers != null)
            {
                for (i = 0; i < offers.Count; i++)
                {
                    ShipStoreOfferDefinition ship = offers[i];
                    if (ship == null || string.IsNullOrWhiteSpace(ship.Id))
                        continue;

                    Items.Add(new StoreAdminEditorItemViewModel
                    {
                        Id = ship.Id,
                        ShortName = string.IsNullOrWhiteSpace(ship.DisplayName) ? ship.PrefabSubtypeId : ship.DisplayName,
                        Description = ship.Description,
                        Rule = null,
                        ShipOffer = ship,
                        ShipRule = FindShipRule(ship.Id),
                        Category = StoreItemCategory.Ships,
                        IsVanilla = ship.IsVanilla
                    });
                }
            }
        }

        private StoreItemRule EnsureRule(string id)
        {
            int i;
            for (i = 0; i < Config.ItemRules.Count; i++)
            {
                StoreItemRule rule = Config.ItemRules[i];
                if (rule != null && string.Equals(rule.Id, id, StringComparison.OrdinalIgnoreCase))
                    return rule;
            }

            StoreItemRule created = StoreItemCatalog.CreateDefaultRule(id);
            Config.ItemRules.Add(created);
            return created;
        }


        public ShipOfferRule GetShipRuleById(string id)
        {
            return FindShipRule(id);
        }

        public ShipOfferRule EnsureShipRule(string id)
        {
            var existing = FindShipRule(id);
            if (existing != null)
                return existing;

            var created = new ShipOfferRule();
            created.Id = id;
            Config.ShipOfferRules.Add(created);
            return created;
        }

        private ShipOfferRule FindShipRule(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || Config == null || Config.ShipOfferRules == null)
                return null;

            for (int i = 0; i < Config.ShipOfferRules.Count; i++)
            {
                var rule = Config.ShipOfferRules[i];
                if (rule != null && string.Equals(rule.Id, id, StringComparison.OrdinalIgnoreCase))
                    return rule;
            }

            return null;
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
