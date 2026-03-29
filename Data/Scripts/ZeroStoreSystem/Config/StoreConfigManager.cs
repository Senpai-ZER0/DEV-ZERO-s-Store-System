using System;
using System.Globalization;
using System.Text;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;

namespace ZeroStoreSystem.Config
{
    public static class StoreConfigManager
    {
        public static StoreBlockConfig ReadBlockConfig(IMyTerminalBlock block)
        {
            var config = new StoreBlockConfig();

            if (block == null)
                return config;

            try
            {
                if (string.IsNullOrWhiteSpace(block.CustomData))
                {
                    WriteDefaultBlockConfig(block);
                    Log.Info("Default CustomData written for '" + block.CustomName + "'");
                }

                ParseIniLike(block.CustomData, config);

                Log.Info(
                    "Store config loaded for '" + block.CustomName + "', " +
                    "TradeMode=" + config.TradeMode + ", Enabled=" + config.Enabled + ", RefreshIntervalSeconds=" + config.RefreshIntervalSeconds + ", ItemRules=" + config.ItemRules.Count + ", ShipOfferRules=" + config.ShipOfferRules.Count);
            }
            catch (Exception e)
            {
                Log.Error("Failed to read config for '" + GetBlockName(block) + "': " + e);
            }

            return config;
        }


        public static void SaveBlockConfig(IMyTerminalBlock block, StoreBlockConfig config)
        {
            if (block == null || config == null)
                return;

            block.CustomData = SerializeBlockConfig(config);
        }

        public static string SerializeBlockConfig(StoreBlockConfig config)
        {
            if (config == null)
                config = new StoreBlockConfig();

            var sb = new StringBuilder();

            sb.AppendLine("[Store]");
            sb.AppendLine("Enabled=" + config.Enabled.ToString().ToLowerInvariant());
            sb.AppendLine("UseAutoProfile=" + config.UseAutoProfile.ToString().ToLowerInvariant());
            sb.AppendLine("ProfileId=" + (config.ProfileId ?? string.Empty));
            sb.AppendLine("TradeMode=" + config.TradeMode);
            sb.AppendLine("RefreshIntervalSeconds=" + config.RefreshIntervalSeconds.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine();

            AppendItemGroups(sb, config);
            AppendShipOfferGroups(sb, config);
            return sb.ToString();
        }

        public static void WriteDefaultBlockConfig(IMyTerminalBlock block)
        {
            if (block == null)
                return;

            var config = new StoreBlockConfig();
            AddDefaultVanillaComponentRules(config);
            block.CustomData = SerializeBlockConfig(config);
        }

        private static void AppendItemRule(
            StringBuilder sb,
            string itemId,
            bool allowed,
            bool forceInclude,
            bool offerEnabled,
            float offerPriceMod,
            int offerAmount,
            bool orderEnabled,
            float orderPriceMod,
            int orderAmount)
        {
            sb.AppendLine("[Item:" + itemId + "]");
            sb.AppendLine("Allowed=" + allowed.ToString().ToLowerInvariant());
            sb.AppendLine("ForceInclude=" + forceInclude.ToString().ToLowerInvariant());
            sb.AppendLine("Offer.Enabled=" + offerEnabled.ToString().ToLowerInvariant());
            sb.AppendLine("Offer.PriceMod=" + offerPriceMod.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("Offer.Amount=" + offerAmount.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("Order.Enabled=" + orderEnabled.ToString().ToLowerInvariant());
            sb.AppendLine("Order.PriceMod=" + orderPriceMod.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("Order.Amount=" + orderAmount.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine();
        }

        private static void AddDefaultVanillaComponentRules(StoreBlockConfig config)
        {
            if (config == null)
                return;

            AddRule(config, "MyObjectBuilder_Component/SteelPlate", true, true, true, 1.0f, 200, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Construction", true, true, false, 1.0f, 0, true, 1.0f, 150);

            foreach (var id in StoreItemCatalog.VanillaComponentIds)
            {
                if (FindRule(config, id) == null)
                    AddRule(config, id, true, false, false, 1.0f, 0, false, 1.0f, 0);
            }

            foreach (var id in StoreItemCatalog.VanillaIngotIds)
            {
                if (FindRule(config, id) == null)
                    AddRule(config, id, true, false, false, 1.0f, 0, false, 1.0f, 0);
            }

            foreach (var id in StoreItemCatalog.VanillaOreIds)
            {
                if (FindRule(config, id) == null)
                    AddRule(config, id, true, false, false, 1.0f, 0, false, 1.0f, 0);
            }

            foreach (var id in StoreItemCatalog.VanillaAmmoIds)
            {
                if (FindRule(config, id) == null)
                    AddRule(config, id, true, false, false, 1.0f, 0, false, 1.0f, 0);
            }

        }

        private static void AddRule(StoreBlockConfig config, string itemId, bool allowed, bool forceInclude, bool offerEnabled, float offerPriceMod, int offerAmount, bool orderEnabled, float orderPriceMod, int orderAmount)
        {
            var rule = new StoreItemRule();
            rule.Id = itemId;
            rule.Allowed = allowed;
            rule.ForceInclude = forceInclude;
            rule.Offer.Enabled = offerEnabled;
            rule.Offer.PriceMod = offerPriceMod;
            rule.Offer.Amount = offerAmount;
            rule.Order.Enabled = orderEnabled;
            rule.Order.PriceMod = orderPriceMod;
            rule.Order.Amount = orderAmount;
            config.ItemRules.Add(rule);
        }

        private static void AppendItemGroups(StringBuilder sb, StoreBlockConfig config)
        {
            AppendCategoryGroup(sb, config, "Components", StoreItemCatalog.VanillaComponentIds);
            AppendCategoryGroup(sb, config, "Ingots", StoreItemCatalog.VanillaIngotIds);
            AppendCategoryGroup(sb, config, "Ores", StoreItemCatalog.VanillaOreIds);
            AppendCategoryGroup(sb, config, "Ammo", StoreItemCatalog.VanillaAmmoIds);

            if (config == null || config.ItemRules == null)
                return;

            foreach (var rule in config.ItemRules)
            {
                if (rule == null || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                if (StoreItemCatalog.IsVanilla(rule.Id) || StoreItemCatalog.GetCategory(rule.Id) == StoreItemCategory.Ships)
                    continue;

                AppendItemRule(sb, rule.Id, rule.Allowed, rule.ForceInclude,
                    rule.Offer != null && rule.Offer.Enabled,
                    rule.Offer != null ? rule.Offer.PriceMod : 1.0f,
                    rule.Offer != null ? rule.Offer.Amount : 0,
                    rule.Order != null && rule.Order.Enabled,
                    rule.Order != null ? rule.Order.PriceMod : 1.0f,
                    rule.Order != null ? rule.Order.Amount : 0);
            }
        }

        private static void AppendCategoryGroup(StringBuilder sb, StoreBlockConfig config, string title, string[] ids)
        {
            AppendGroupHeader(sb, title);

            if (ids == null)
                return;

            for (int i = 0; i < ids.Length; i++)
            {
                AppendKnownRule(sb, config, ids[i]);
            }

            sb.AppendLine();
        }

        private static void AppendGroupHeader(StringBuilder sb, string title)
        {
            sb.AppendLine("; ===== " + title + " =====");
        }

        private static void AppendKnownRule(StringBuilder sb, StoreBlockConfig config, string id)
        {
            var rule = FindRule(config, id);
            if (rule == null)
                return;

            AppendItemRule(sb, rule.Id, rule.Allowed, rule.ForceInclude,
                rule.Offer != null && rule.Offer.Enabled,
                rule.Offer != null ? rule.Offer.PriceMod : 1.0f,
                rule.Offer != null ? rule.Offer.Amount : 0,
                rule.Order != null && rule.Order.Enabled,
                rule.Order != null ? rule.Order.PriceMod : 1.0f,
                rule.Order != null ? rule.Order.Amount : 0);
        }


        private static void AppendShipOfferGroups(StringBuilder sb, StoreBlockConfig config)
        {
            if (config == null || config.ShipOfferRules == null || config.ShipOfferRules.Count == 0)
                return;

            AppendGroupHeader(sb, "Ships");

            for (int i = 0; i < config.ShipOfferRules.Count; i++)
            {
                var rule = config.ShipOfferRules[i];
                if (rule == null || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                sb.AppendLine("[ShipOffer:" + rule.Id + "]");
                sb.AppendLine("Enabled=" + rule.Enabled.ToString().ToLowerInvariant());
                sb.AppendLine("PriceOverride=" + rule.PriceOverride.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("StockOverride=" + rule.StockOverride.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine();
            }
        }

        private static ShipOfferRule FindShipOfferRule(StoreBlockConfig config, string id)
        {
            if (config == null || config.ShipOfferRules == null || string.IsNullOrWhiteSpace(id))
                return null;

            for (int i = 0; i < config.ShipOfferRules.Count; i++)
            {
                var rule = config.ShipOfferRules[i];
                if (rule != null && string.Equals(rule.Id, id, StringComparison.OrdinalIgnoreCase))
                    return rule;
            }

            return null;
        }
        private static StoreItemRule FindRule(StoreBlockConfig config, string id)
        {
            if (config == null || config.ItemRules == null || string.IsNullOrWhiteSpace(id))
                return null;

            for (int i = 0; i < config.ItemRules.Count; i++)
            {
                var rule = config.ItemRules[i];
                if (rule != null && string.Equals(rule.Id, id, StringComparison.OrdinalIgnoreCase))
                    return rule;
            }

            return null;
        }

        private static bool IsKnownVanillaComponent(string id)
        {
            return StoreItemCatalog.IsVanilla(id);
        }

        private static void ParseIniLike(string text, StoreBlockConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(text))
                return;

            string currentSection = string.Empty;
            StoreItemRule currentItemRule = null;
            ShipOfferRule currentShipRule = null;

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith(";") || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string sectionName = line.Substring(1, line.Length - 2).Trim();

                    currentSection = string.Empty;
                    currentItemRule = null;
                    currentShipRule = null;

                    if (sectionName.Equals("Store", StringComparison.OrdinalIgnoreCase))
                    {
                        currentSection = "Store";
                    }
                    else if (sectionName.StartsWith("Item:", StringComparison.OrdinalIgnoreCase))
                    {
                        string itemId = sectionName.Substring(5).Trim();
                        if (!string.IsNullOrWhiteSpace(itemId))
                        {
                            currentSection = "Item";
                            currentItemRule = new StoreItemRule();
                            currentItemRule.Id = itemId;
                            config.ItemRules.Add(currentItemRule);
                        }
                    }
                    else if (sectionName.StartsWith("ShipOffer:", StringComparison.OrdinalIgnoreCase))
                    {
                        string shipOfferId = sectionName.Substring("ShipOffer:".Length).Trim();
                        if (!string.IsNullOrWhiteSpace(shipOfferId))
                        {
                            currentSection = "ShipOffer";
                            currentShipRule = new ShipOfferRule();
                            currentShipRule.Id = shipOfferId;
                            config.ShipOfferRules.Add(currentShipRule);
                        }
                    }

                    continue;
                }

                int separator = line.IndexOf('=');
                if (separator <= 0)
                    continue;

                string key = line.Substring(0, separator).Trim();
                string value = line.Substring(separator + 1).Trim();

                if (currentSection == "Store")
                {
                    ParseStoreKey(config, key, value);
                }
                else if (currentSection == "Item" && currentItemRule != null)
                {
                    ParseItemKey(currentItemRule, key, value);
                }
                else if (currentSection == "ShipOffer" && currentShipRule != null)
                {
                    ParseShipOfferKey(currentShipRule, key, value);
                }
            }
        }

        private static void ParseStoreKey(StoreBlockConfig config, string key, string value)
        {
            switch (key)
            {
                case "Enabled":
                    config.Enabled = ParseBool(value, config.Enabled);
                    break;

                case "UseAutoProfile":
                    config.UseAutoProfile = ParseBool(value, config.UseAutoProfile);
                    break;

                case "ProfileId":
                    config.ProfileId = value ?? string.Empty;
                    break;

                case "TradeMode":
                    config.TradeMode = ParseTradeMode(value);
                    break;

                case "RefreshIntervalSeconds":
                    config.RefreshIntervalSeconds = Math.Max(0, ParseInt(value, config.RefreshIntervalSeconds));
                    break;
            }
        }

        private static void ParseItemKey(StoreItemRule rule, string key, string value)
        {
            switch (key)
            {
                case "Allowed":
                    rule.Allowed = ParseBool(value, rule.Allowed);
                    break;

                case "ForceInclude":
                    rule.ForceInclude = ParseBool(value, rule.ForceInclude);
                    break;

                case "Offer.Enabled":
                    rule.Offer.Enabled = ParseBool(value, rule.Offer.Enabled);
                    break;

                case "Offer.PriceMod":
                    rule.Offer.PriceMod = ParseFloat(value, rule.Offer.PriceMod);
                    break;

                case "Offer.Amount":
                    rule.Offer.Amount = Math.Max(0, ParseInt(value, rule.Offer.Amount));
                    break;

                case "Order.Enabled":
                    rule.Order.Enabled = ParseBool(value, rule.Order.Enabled);
                    break;

                case "Order.PriceMod":
                    rule.Order.PriceMod = ParseFloat(value, rule.Order.PriceMod);
                    break;

                case "Order.Amount":
                    rule.Order.Amount = Math.Max(0, ParseInt(value, rule.Order.Amount));
                    break;
            }
        }

        private static void ParseShipOfferKey(ShipOfferRule rule, string key, string value)
        {
            switch (key)
            {
                case "Enabled":
                    rule.Enabled = ParseBool(value, rule.Enabled);
                    break;
                case "PriceOverride":
                    rule.PriceOverride = ParseInt(value, rule.PriceOverride);
                    break;
                case "StockOverride":
                    rule.StockOverride = ParseInt(value, rule.StockOverride);
                    break;
            }
        }

        private static StoreTradeMode ParseTradeMode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return StoreTradeMode.BuyAndSell;

            switch (value.Trim())
            {
                case "BuyAndSell":
                    return StoreTradeMode.BuyAndSell;
                case "BuyOnly":
                    return StoreTradeMode.BuyOnly;
                case "SellOnly":
                    return StoreTradeMode.SellOnly;
                default:
                    Log.Error("Unknown TradeMode '" + value + "', fallback to BuyAndSell");
                    return StoreTradeMode.BuyAndSell;
            }
        }

        private static bool ParseBool(string value, bool fallback)
        {
            bool result;
            return bool.TryParse(value, out result) ? result : fallback;
        }

        private static int ParseInt(string value, int fallback)
        {
            int result;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) ? result : fallback;
        }

        private static float ParseFloat(string value, float fallback)
        {
            float result;
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ? result : fallback;
        }

        private static string GetBlockName(IMyTerminalBlock block)
        {
            return block != null ? block.CustomName : "<null>";
        }
    }
}
