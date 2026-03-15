using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;

namespace ZeroStoreSystem.UI.Admin
{
    public class StoreAdminEditorState
    {
        public string SearchText = string.Empty;
        public bool ActiveOnly = false;
        public StoreBlockConfig Config = new StoreBlockConfig();
        public readonly List<StoreAdminEditorItemViewModel> Items = new List<StoreAdminEditorItemViewModel>();

        public void LoadFromBlock(IMyTerminalBlock block)
        {
            Config = StoreConfigManager.ReadBlockConfig(block);

            if (Config != null && Config.ItemRules != null)
                VanillaComponentCatalog.EnsurePresent(Config.ItemRules);

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

            if (ActiveOnly)
                query = query.Where(x => x != null && x.IsActive);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();
                query = query.Where(x => x != null &&
                    ((x.Id != null && x.Id.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (x.ShortName != null && x.ShortName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)));
            }

            return query.OrderBy(x => x.ShortName).ThenBy(x => x.Id);
        }

        public void RebuildItems()
        {
            Items.Clear();

            if (Config == null)
                Config = new StoreBlockConfig();

            if (Config.ItemRules == null)
                Config.ItemRules = new List<StoreItemRule>();

            VanillaComponentCatalog.EnsurePresent(Config.ItemRules);

            for (int i = 0; i < Config.ItemRules.Count; i++)
            {
                var rule = Config.ItemRules[i];
                if (rule == null || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                Items.Add(new StoreAdminEditorItemViewModel
                {
                    Id = rule.Id,
                    ShortName = GetShortName(rule.Id),
                    Rule = rule
                });
            }
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
