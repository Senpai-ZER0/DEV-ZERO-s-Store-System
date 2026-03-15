using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;

namespace ZeroStoreSystem.UI.Admin
{
    public enum StoreEditorFilter
    {
        All,
        Components,
        Ingots,
        Ores,
        Ammo
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
            }

            if (ActiveOnly)
                query = query.Where(x => x != null && x.IsActive);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();
                query = query.Where(x => x != null &&
                    ((x.Id != null && x.Id.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (x.ShortName != null && x.ShortName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)));
            }

            return query
                .OrderBy(x => StoreItemCatalog.GetCategorySortOrder(x.Category))
                .ThenBy(x => x.IsVanilla ? 0 : 1)
                .ThenBy(x => x.ShortName)
                .ThenBy(x => x.Id);
        }

        public void RebuildItems()
        {
            Items.Clear();

            if (Config == null)
                Config = new StoreBlockConfig();

            if (Config.ItemRules == null)
                Config.ItemRules = new List<StoreItemRule>();

            foreach (var id in StoreItemCatalog.EnumerateKnownVanillaIds())
            {
                EnsureRule(id);
            }

            for (int i = 0; i < Config.ItemRules.Count; i++)
            {
                var rule = Config.ItemRules[i];
                if (rule == null || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                Items.Add(new StoreAdminEditorItemViewModel
                {
                    Id = rule.Id,
                    ShortName = GetShortName(rule.Id),
                    Rule = rule,
                    Category = StoreItemCatalog.GetCategory(rule.Id),
                    IsVanilla = StoreItemCatalog.IsVanilla(rule.Id)
                });
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
    }
}
