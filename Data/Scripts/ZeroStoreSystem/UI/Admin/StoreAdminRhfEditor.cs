using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using RichHudFramework.Client;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.Sync;

namespace ZeroStoreSystem.UI.Admin
{
    public sealed class StoreAdminRhfEditor
    {
        private readonly StoreAdminEditorState _state = new StoreAdminEditorState();
        private readonly List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();

        private TerminalPageCategory _rootCategory;
        private ControlPage _page;

        private TerminalDropdown<long> _blockDropdown;
        private TerminalButton _refreshBlocksButton;
        private TerminalOnOffButton _storeEnabledToggle;
        private TerminalDropdown<string> _tradeModeDropdown;
        private TerminalTextField _refreshIntervalField;

        private TerminalDropdown<string> _filterDropdown;
        private TerminalTextField _searchField;
        private TerminalOnOffButton _activeOnlyToggle;
        private TerminalButton _reloadButton;
        private TerminalButton _saveButton;
        private TerminalButton _regenButton;

        private TerminalList<string> _itemList;
        private TerminalLabel _itemSummaryLabel;

        private TerminalOnOffButton _allowedToggle;
        private TerminalOnOffButton _forceIncludeToggle;
        private TerminalOnOffButton _offerEnabledToggle;
        private TerminalTextField _offerPriceField;
        private TerminalTextField _offerAmountField;
        private TerminalOnOffButton _orderEnabledToggle;
        private TerminalTextField _orderPriceField;
        private TerminalTextField _orderAmountField;

        private bool _suppressEvents;
        private long _selectedBlockId;
        private string _selectedItemId;
        private string _lastClickedItemId;
        private DateTime _lastItemClickUtc = DateTime.MinValue;

        public bool Ready => _page != null;

        public void Init()
        {
            if (_page != null)
                return;

            _rootCategory = new TerminalPageCategory { Name = "ZERO Store" };
            _page = new ControlPage { Name = "Admin Store Editor", Enabled = AdminAccess.IsLocalAdminOrHigher() };
            _rootCategory.Enabled = _page.Enabled;

            BuildUi();

            _rootCategory.Add(_page);
            RichHudTerminal.Root.Add(_rootCategory);

            RefreshBlockList();
            RefreshAdminAccess();
        }

        public void Close()
        {
            _page = null;
            _rootCategory = null;
            _blocks.Clear();
        }

        public void RefreshAdminAccess()
        {
            bool admin = AdminAccess.IsLocalAdminOrHigher();
            if (_page != null)
                _page.Enabled = admin;
            if (_rootCategory != null)
                _rootCategory.Enabled = admin;
        }

        public bool OpenForLocalAdmin()
        {
            RefreshAdminAccess();

            if (!AdminAccess.IsLocalAdminOrHigher())
            {
                Notify("RHF editor is available only to administrators.");
                return false;
            }

            if (!RichHudClient.Registered || _page == null)
            {
                Notify("RHF is not ready yet.");
                return false;
            }

            RefreshBlockList();
            RichHudTerminal.OpenToPage(_page);
            return true;
        }

        private void BuildUi()
        {
            var storeCategory = new ControlCategory
            {
                HeaderText = "Store",
                SubheaderText = "Select a registered NPC store block and edit its basic settings."
            };

            var storeTile1 = new ControlTile();
            _blockDropdown = new TerminalDropdown<long> { Name = "Target Store" };
            _blockDropdown.ControlChanged += BlockDropdownChanged;
            _refreshBlocksButton = new TerminalButton { Name = "Refresh Store List" };
            _refreshBlocksButton.ControlChanged += (s, e) => RefreshBlockList();
            storeTile1.Add(_blockDropdown);
            storeTile1.Add(_refreshBlocksButton);

            var storeTile2 = new ControlTile();
            _storeEnabledToggle = new TerminalOnOffButton { Name = "Store Enabled" };
            _storeEnabledToggle.ControlChanged += StoreEnabledChanged;
            _tradeModeDropdown = new TerminalDropdown<string> { Name = "Trade Mode" };
            _tradeModeDropdown.ControlChanged += TradeModeChanged;
            PopulateTradeModes();
            _refreshIntervalField = new TerminalTextField { Name = "Refresh Interval (s)" };
            _refreshIntervalField.CharFilterFunc = AllowIntegerChar;
            _refreshIntervalField.ControlChanged += RefreshIntervalChanged;
            storeTile2.Add(_storeEnabledToggle);
            storeTile2.Add(_tradeModeDropdown);
            storeTile2.Add(_refreshIntervalField);

            storeCategory.Add(storeTile1);
            storeCategory.Add(storeTile2);
            _page.Add(storeCategory);

            var filterCategory = new ControlCategory
            {
                HeaderText = "Filter & Actions",
                SubheaderText = "Search items, hide inactive entries, reload from CustomData, save changes, or regenerate the store immediately."
            };

            var filterTile1 = new ControlTile();
            _filterDropdown = new TerminalDropdown<string> { Name = "Category" };
            _filterDropdown.ControlChanged += FilterChanged;
            PopulateFilters();
            _searchField = new TerminalTextField { Name = "Search" };
            _searchField.Value = string.Empty;
            _searchField.ControlChanged += SearchChanged;
            _activeOnlyToggle = new TerminalOnOffButton { Name = "Active Only" };
            _activeOnlyToggle.Value = false;
            _activeOnlyToggle.ControlChanged += ActiveOnlyChanged;
            filterTile1.Add(_filterDropdown);
            filterTile1.Add(_searchField);
            filterTile1.Add(_activeOnlyToggle);

            var filterTile2 = new ControlTile();
            _reloadButton = new TerminalButton { Name = "Reload from CustomData" };
            _reloadButton.ControlChanged += (s, e) => ReloadCurrentBlock();
            _saveButton = new TerminalButton { Name = "Save to CustomData" };
            _saveButton.ControlChanged += (s, e) => SaveCurrentBlock();
            _regenButton = new TerminalButton { Name = "Regenerate Store" };
            _regenButton.ControlChanged += (s, e) => RegenerateCurrentBlock();
            filterTile2.Add(_reloadButton);
            filterTile2.Add(_saveButton);
            filterTile2.Add(_regenButton);

            filterCategory.Add(filterTile1);
            filterCategory.Add(filterTile2);
            _page.Add(filterCategory);

            var itemCategory = new ControlCategory
            {
                HeaderText = "Items",
                SubheaderText = "Select an item or ship from the filtered list. Double-click an item to enable trade quickly according to the current Trade Mode."
            };

            var itemTile1 = new ControlTile();
            _itemList = new TerminalList<string> { Name = "Catalog Entries" };
            _itemList.ControlChanged += ItemSelectionChanged;
            _itemSummaryLabel = new TerminalLabel { Name = "No item selected." };
            itemTile1.Add(_itemList);
            itemTile1.Add(_itemSummaryLabel);

            var editTile1 = new ControlTile();
            _allowedToggle = new TerminalOnOffButton { Name = "Allowed" };
            _allowedToggle.ControlChanged += AllowedChanged;
            _forceIncludeToggle = new TerminalOnOffButton { Name = "Force Include" };
            _forceIncludeToggle.ControlChanged += ForceIncludeChanged;
            editTile1.Add(_allowedToggle);
            editTile1.Add(_forceIncludeToggle);

            var editTile2 = new ControlTile();
            _offerEnabledToggle = new TerminalOnOffButton { Name = "Offer Enabled" };
            _offerEnabledToggle.ControlChanged += OfferEnabledChanged;
            _offerPriceField = new TerminalTextField { Name = "Offer Price Mod" };
            _offerPriceField.CharFilterFunc = AllowFloatChar;
            _offerPriceField.ControlChanged += OfferPriceChanged;
            _offerAmountField = new TerminalTextField { Name = "Offer Amount" };
            _offerAmountField.CharFilterFunc = AllowIntegerChar;
            _offerAmountField.ControlChanged += OfferAmountChanged;
            editTile2.Add(_offerEnabledToggle);
            editTile2.Add(_offerPriceField);
            editTile2.Add(_offerAmountField);

            var editTile3 = new ControlTile();
            _orderEnabledToggle = new TerminalOnOffButton { Name = "Order Enabled" };
            _orderEnabledToggle.ControlChanged += OrderEnabledChanged;
            _orderPriceField = new TerminalTextField { Name = "Order Price Mod" };
            _orderPriceField.CharFilterFunc = AllowFloatChar;
            _orderPriceField.ControlChanged += OrderPriceChanged;
            _orderAmountField = new TerminalTextField { Name = "Order Amount" };
            _orderAmountField.CharFilterFunc = AllowIntegerChar;
            _orderAmountField.ControlChanged += OrderAmountChanged;
            editTile3.Add(_orderEnabledToggle);
            editTile3.Add(_orderPriceField);
            editTile3.Add(_orderAmountField);

            itemCategory.Add(itemTile1);
            itemCategory.Add(editTile1);
            itemCategory.Add(editTile2);
            itemCategory.Add(editTile3);
            _page.Add(itemCategory);

            SetEditorControlsEnabled(false);
        }

        private void PopulateTradeModes()
        {
            _tradeModeDropdown.List.Clear();
            _tradeModeDropdown.List.Add(new RichText("BuyAndSell"), "BuyAndSell");
            _tradeModeDropdown.List.Add(new RichText("BuyOnly"), "BuyOnly");
            _tradeModeDropdown.List.Add(new RichText("SellOnly"), "SellOnly");
        }

        private void PopulateFilters()
        {
            _filterDropdown.List.Clear();
            _filterDropdown.List.Add(new RichText("All"), nameof(StoreEditorFilter.All));
            _filterDropdown.List.Add(new RichText("Components"), nameof(StoreEditorFilter.Components));
            _filterDropdown.List.Add(new RichText("Ingots"), nameof(StoreEditorFilter.Ingots));
            _filterDropdown.List.Add(new RichText("Ores"), nameof(StoreEditorFilter.Ores));
            _filterDropdown.List.Add(new RichText("Ammo"), nameof(StoreEditorFilter.Ammo));
            _filterDropdown.List.Add(new RichText("Tools"), nameof(StoreEditorFilter.Tools));
            _filterDropdown.List.Add(new RichText("Bottles"), nameof(StoreEditorFilter.Bottles));
            _filterDropdown.List.Add(new RichText("Consumables"), nameof(StoreEditorFilter.Consumables));
            _filterDropdown.List.Add(new RichText("Power"), nameof(StoreEditorFilter.Power));
            _filterDropdown.List.Add(new RichText("Ships"), nameof(StoreEditorFilter.Ships));
        }

        private void RefreshBlockList()
        {
            _blocks.Clear();
            NpcStoreRegistry.GetRegisteredTerminalBlocks(_blocks);
            _blocks.Sort(CompareBlocks);

            _suppressEvents = true;
            _blockDropdown.List.Clear();

            foreach (var block in _blocks)
            {
                string label = GetBlockDisplayName(block);
                _blockDropdown.List.Add(new RichText(label), block.EntityId);
            }

            _suppressEvents = false;

            if (_blocks.Count == 0)
            {
                _selectedBlockId = 0;
                _state.Config = new StoreBlockConfig();
                _state.Items.Clear();
                RefreshItemList();
                SetEditorControlsEnabled(false);
                _itemSummaryLabel.Name = "No registered NPC store blocks are currently loaded.";
                return;
            }

            long idToSelect = _selectedBlockId != 0 ? _selectedBlockId : _blocks[0].EntityId;
            if (!SelectBlockById(idToSelect))
                SelectBlockById(_blocks[0].EntityId);
        }

        private bool SelectBlockById(long blockId)
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                if (_blocks[i] != null && _blocks[i].EntityId == blockId)
                {
                    _suppressEvents = true;
                    _blockDropdown.List.SetSelection(blockId);
                    _suppressEvents = false;
                    LoadBlock(_blocks[i]);
                    return true;
                }
            }

            return false;
        }

        private void LoadBlock(IMyTerminalBlock block)
        {
            if (block == null)
                return;

            _selectedBlockId = block.EntityId;
            _state.LoadFromBlock(block);
            _selectedItemId = null;
            RefreshStoreControls();
            RefreshItemList();
            SetEditorControlsEnabled(true);
        }

        private void RefreshStoreControls()
        {
            _suppressEvents = true;
            _storeEnabledToggle.Value = _state.Config != null && _state.Config.Enabled;
            _refreshIntervalField.Value = _state.Config != null ? _state.Config.RefreshIntervalSeconds.ToString() : "0";
            _tradeModeDropdown.List.SetSelection(_state.Config != null ? _state.Config.TradeMode.ToString() : "BuyAndSell");
            _filterDropdown.List.SetSelection(_state.SelectedFilter.ToString());
            _searchField.Value = _state.SearchText ?? string.Empty;
            _activeOnlyToggle.Value = _state.ActiveOnly;
            _suppressEvents = false;
        }

        private void RefreshItemList()
        {
            _suppressEvents = true;
            _itemList.List.Clear();

            foreach (var item in _state.GetFilteredItems())
            {
                string marker = item.IsActive ? "[On] " : "[Off] ";
                _itemList.List.Add(new RichText(marker + item.ShortName), item.Id);
            }

            _suppressEvents = false;

            if (_itemList.List.Count == 0)
            {
                _selectedItemId = null;
                RefreshSelectedItemControls();
                _itemSummaryLabel.Name = "No items match the current filter.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(_selectedItemId) && SelectItemById(_selectedItemId))
                return;

            SelectItemByIndex(0);
        }

        private bool SelectItemById(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return false;

            for (int i = 0; i < _itemList.List.Count; i++)
            {
                var entry = _itemList.List[i];
                if (entry != null && string.Equals(entry.AssocObject, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    _suppressEvents = true;
                    _itemList.List.SetSelection(i);
                    _suppressEvents = false;
                    _selectedItemId = itemId;
                    RefreshSelectedItemControls();
                    return true;
                }
            }

            return false;
        }

        private void SelectItemByIndex(int index)
        {
            if (_itemList.List.Count == 0 || index < 0 || index >= _itemList.List.Count)
                return;

            _suppressEvents = true;
            _itemList.List.SetSelection(index);
            _suppressEvents = false;
            var entry = _itemList.List[index];
            _selectedItemId = entry != null ? entry.AssocObject : null;
            RefreshSelectedItemControls();
        }

        private StoreItemRule GetSelectedRule()
        {
            if (string.IsNullOrWhiteSpace(_selectedItemId) || _state.Config == null || _state.Config.ItemRules == null)
                return null;

            for (int i = 0; i < _state.Config.ItemRules.Count; i++)
            {
                var rule = _state.Config.ItemRules[i];
                if (rule != null && string.Equals(rule.Id, _selectedItemId, StringComparison.OrdinalIgnoreCase))
                    return rule;
            }

            return null;
        }

        private void RefreshSelectedItemControls()
        {
            var selectedItem = _state.GetItemById(_selectedItemId);
            var rule = GetSelectedRule();
            bool hasRule = rule != null;
            bool isShip = selectedItem != null && selectedItem.IsShip;

            _suppressEvents = true;
            _allowedToggle.Enabled = hasRule || isShip;
            _forceIncludeToggle.Enabled = hasRule;
            _offerEnabledToggle.Enabled = hasRule;
            _offerPriceField.Enabled = hasRule;
            _offerAmountField.Enabled = hasRule;
            _orderEnabledToggle.Enabled = hasRule;
            _orderPriceField.Enabled = hasRule;
            _orderAmountField.Enabled = hasRule;

            if (hasRule)
            {
                _allowedToggle.Value = rule.Allowed;
                _forceIncludeToggle.Value = rule.ForceInclude;
                _offerEnabledToggle.Value = rule.Offer != null && rule.Offer.Enabled;
                _offerPriceField.Value = rule.Offer != null ? rule.Offer.PriceMod.ToString("0.###") : "1";
                _offerAmountField.Value = rule.Offer != null ? rule.Offer.Amount.ToString() : "0";
                _orderEnabledToggle.Value = rule.Order != null && rule.Order.Enabled;
                _orderPriceField.Value = rule.Order != null ? rule.Order.PriceMod.ToString("0.###") : "1";
                _orderAmountField.Value = rule.Order != null ? rule.Order.Amount.ToString() : "0";
                _itemSummaryLabel.Name = rule.Id;
            }
            else if (isShip)
            {
                var shipRule = selectedItem != null && selectedItem.ShipOffer != null ? _state.FindShipOfferRule(selectedItem.ShipOffer.Id) : null;
                _allowedToggle.Value = shipRule != null && shipRule.Enabled;
                _forceIncludeToggle.Value = false;
                _offerEnabledToggle.Value = false;
                _offerPriceField.Value = string.Empty;
                _offerAmountField.Value = string.Empty;
                _orderEnabledToggle.Value = false;
                _orderPriceField.Value = string.Empty;
                _orderAmountField.Value = string.Empty;

                var ship = selectedItem.ShipOffer;
                if (ship != null)
                    _itemSummaryLabel.Name = ship.DisplayName + " | " + ship.PrefabSubtypeId + " | Price: " + ship.Price;
                else
                    _itemSummaryLabel.Name = selectedItem.ShortName;
            }
            else
            {
                _allowedToggle.Value = false;
                _forceIncludeToggle.Value = false;
                _offerEnabledToggle.Value = false;
                _offerPriceField.Value = "1";
                _offerAmountField.Value = "0";
                _orderEnabledToggle.Value = false;
                _orderPriceField.Value = "1";
                _orderAmountField.Value = "0";
                _itemSummaryLabel.Name = "No item selected.";
            }

            _suppressEvents = false;
        }

        private void SetEditorControlsEnabled(bool enabled)
        {
            _storeEnabledToggle.Enabled = enabled;
            _tradeModeDropdown.Enabled = enabled;
            _refreshIntervalField.Enabled = enabled;
            _filterDropdown.Enabled = enabled;
            _searchField.Enabled = enabled;
            _activeOnlyToggle.Enabled = enabled;
            _reloadButton.Enabled = enabled;
            _saveButton.Enabled = enabled;
            _regenButton.Enabled = enabled;
            _itemList.Enabled = enabled;
            if (!enabled)
                RefreshSelectedItemControls();
        }

        private void BlockDropdownChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var selection = _blockDropdown.Value;
            if (selection == null)
                return;

            long blockId = selection.AssocObject;
            for (int i = 0; i < _blocks.Count; i++)
            {
                var block = _blocks[i];
                if (block != null && block.EntityId == blockId)
                {
                    LoadBlock(block);
                    return;
                }
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var selection = _filterDropdown.Value;
            string value = selection != null ? selection.AssocObject : null;

            StoreEditorFilter filter;
            if (!Enum.TryParse(value, out filter))
                filter = StoreEditorFilter.All;

            _state.SelectedFilter = filter;
            RefreshItemList();
        }

        private void SearchChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            _state.SearchText = _searchField.Value ?? string.Empty;
            RefreshItemList();
        }

        private void ActiveOnlyChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            _state.ActiveOnly = _activeOnlyToggle.Value;
            RefreshItemList();
        }

        private void ItemSelectionChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var selection = _itemList.Value;
            string itemId = selection != null ? selection.AssocObject : null;
            bool sameSelection = !string.IsNullOrWhiteSpace(itemId)
                && string.Equals(_selectedItemId, itemId, StringComparison.OrdinalIgnoreCase);

            _selectedItemId = itemId;
            RefreshSelectedItemControls();

            if (sameSelection)
                TryQuickEnableSelectedItem(itemId);
            else
                RememberItemClick(itemId);
        }

        private void RememberItemClick(string itemId)
        {
            _lastClickedItemId = itemId;
            _lastItemClickUtc = DateTime.UtcNow;
        }

        private void TryQuickEnableSelectedItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return;

            DateTime now = DateTime.UtcNow;
            bool isDoubleClick = string.Equals(_lastClickedItemId, itemId, StringComparison.OrdinalIgnoreCase)
                && (now - _lastItemClickUtc).TotalMilliseconds <= 450d;

            _lastClickedItemId = itemId;
            _lastItemClickUtc = now;

            if (!isDoubleClick)
                return;

            if (_state.ApplyQuickTradeSetup(itemId))
            {
                RefreshItemList();
                SelectItemById(itemId);
                Notify("Quick trade setup applied for " + itemId + ".");
            }
        }

        private void StoreEnabledChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _state.Config == null)
                return;

            _state.Config.Enabled = _storeEnabledToggle.Value;
        }

        private void TradeModeChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _state.Config == null)
                return;

            var selection = _tradeModeDropdown.Value;
            if (selection == null || string.IsNullOrWhiteSpace(selection.AssocObject))
                return;

            StoreTradeMode tradeMode;
            if (Enum.TryParse(selection.AssocObject, out tradeMode))
                _state.Config.TradeMode = tradeMode;
        }

        private void RefreshIntervalChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || _state.Config == null)
                return;

            int value;
            if (int.TryParse(_refreshIntervalField.Value ?? "0", out value))
                _state.Config.RefreshIntervalSeconds = value < 0 ? 0 : value;
        }

        private void AllowedChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var selectedItem = _state.GetItemById(_selectedItemId);
            if (selectedItem != null && selectedItem.IsShip && selectedItem.ShipOffer != null)
            {
                var shipRule = _state.EnsureShipOfferRule(selectedItem.ShipOffer.Id);
                shipRule.Enabled = _allowedToggle.Value;
                selectedItem.ShipEnabled = shipRule.Enabled;
                RefreshItemList();
                return;
            }

            var rule = GetSelectedRule();
            if (rule != null)
            {
                rule.Allowed = _allowedToggle.Value;
                RefreshItemList();
            }
        }

        private void ForceIncludeChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var rule = GetSelectedRule();
            if (rule != null)
                rule.ForceInclude = _forceIncludeToggle.Value;
        }

        private void OfferEnabledChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var rule = GetSelectedRule();
            if (rule != null && rule.Offer != null)
            {
                rule.Offer.Enabled = _offerEnabledToggle.Value;
                RefreshItemList();
            }
        }

        private void OfferPriceChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var rule = GetSelectedRule();
            if (rule == null || rule.Offer == null)
                return;

            float value;
            if (float.TryParse(_offerPriceField.Value ?? "1", System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
                rule.Offer.PriceMod = value <= 0f ? 1f : value;
        }

        private void OfferAmountChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var rule = GetSelectedRule();
            if (rule == null || rule.Offer == null)
                return;

            int value;
            if (int.TryParse(_offerAmountField.Value ?? "0", out value))
            {
                rule.Offer.Amount = value < 0 ? 0 : value;
                RefreshItemList();
            }
        }

        private void OrderEnabledChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var rule = GetSelectedRule();
            if (rule != null && rule.Order != null)
            {
                rule.Order.Enabled = _orderEnabledToggle.Value;
                RefreshItemList();
            }
        }

        private void OrderPriceChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var rule = GetSelectedRule();
            if (rule == null || rule.Order == null)
                return;

            float value;
            if (float.TryParse(_orderPriceField.Value ?? "1", System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
                rule.Order.PriceMod = value <= 0f ? 1f : value;
        }

        private void OrderAmountChanged(object sender, EventArgs e)
        {
            if (_suppressEvents)
                return;

            var rule = GetSelectedRule();
            if (rule == null || rule.Order == null)
                return;

            int value;
            if (int.TryParse(_orderAmountField.Value ?? "0", out value))
            {
                rule.Order.Amount = value < 0 ? 0 : value;
                RefreshItemList();
            }
        }

        private void ReloadCurrentBlock()
        {
            var block = GetSelectedBlock();
            if (block == null)
            {
                Notify("No store block selected.");
                return;
            }

            _state.LoadFromBlock(block);
            RefreshStoreControls();
            RefreshItemList();
            Notify("Store config reloaded from CustomData.");
        }

        private void SaveCurrentBlock()
        {
            var block = GetSelectedBlock();
            if (block == null)
            {
                Notify("No store block selected.");
                return;
            }

            if (StoreAdminEditorService.SaveToBlockOnly(block, _state))
            {
                ReloadCurrentBlock();
                Notify("Store config saved to CustomData.");
            }
            else
            {
                Notify("Failed to save store config.");
            }
        }

        private void RegenerateCurrentBlock()
        {
            var block = GetSelectedBlock();
            if (block == null)
            {
                Notify("No store block selected.");
                return;
            }

            if (StoreAdminEditorService.SaveAndRegenerate(block, _state))
            {
                ReloadCurrentBlock();
                Notify("Store regenerated.");
            }
            else
            {
                Notify("Failed to regenerate store.");
            }
        }

        private IMyTerminalBlock GetSelectedBlock()
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                if (_blocks[i] != null && _blocks[i].EntityId == _selectedBlockId)
                    return _blocks[i];
            }

            return null;
        }

        private static int CompareBlocks(IMyTerminalBlock a, IMyTerminalBlock b)
        {
            string an = GetBlockDisplayName(a);
            string bn = GetBlockDisplayName(b);
            return string.Compare(an, bn, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetBlockDisplayName(IMyTerminalBlock block)
        {
            if (block == null)
                return "<null>";

            string name = block.CustomName;
            if (string.IsNullOrWhiteSpace(name))
                name = block.DefinitionDisplayNameText;
            if (string.IsNullOrWhiteSpace(name))
                name = block.EntityId.ToString();

            return name + " [" + block.EntityId + "]";
        }

        private static bool AllowIntegerChar(char c)
        {
            return char.IsDigit(c) || c == '-';
        }

        private static bool AllowFloatChar(char c)
        {
            return char.IsDigit(c) || c == '-' || c == '.' || c == ',';
        }

        private static void Notify(string text)
        {
            if (MyAPIGateway.Utilities != null)
                MyAPIGateway.Utilities.ShowMessage("ZERO Store", text);
        }
    }
}
