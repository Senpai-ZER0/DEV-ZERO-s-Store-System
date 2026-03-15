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
                    "TradeMode=" + config.TradeMode + ", Enabled=" + config.Enabled + ", RefreshIntervalSeconds=" + config.RefreshIntervalSeconds + ", ItemRules=" + config.ItemRules.Count);
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
            AddRule(config, "MyObjectBuilder_Component/InteriorPlate", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Girder", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/SmallTube", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/LargeTube", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/MetalGrid", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/BulletproofGlass", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Computer", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Display", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Detector", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/RadioCommunication", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Motor", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Reactor", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Thrust", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Superconductor", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/GravityGenerator", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Explosives", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/Medical", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/PowerCell", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AddRule(config, "MyObjectBuilder_Component/SolarCell", true, false, false, 1.0f, 0, false, 1.0f, 0);
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
            AppendGroupHeader(sb, "Components: structural");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/SteelPlate");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Construction");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/InteriorPlate");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Girder");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/SmallTube");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/LargeTube");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/MetalGrid");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/BulletproofGlass");

            AppendGroupHeader(sb, "Components: electronics");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Computer");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Display");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Detector");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/RadioCommunication");

            AppendGroupHeader(sb, "Components: machinery");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Motor");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Reactor");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Thrust");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Superconductor");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/GravityGenerator");

            AppendGroupHeader(sb, "Components: specialized");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Explosives");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/Medical");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/PowerCell");
            AppendKnownRule(sb, config, "MyObjectBuilder_Component/SolarCell");

            if (config == null || config.ItemRules == null)
                return;

            foreach (var rule in config.ItemRules)
            {
                if (rule == null || string.IsNullOrWhiteSpace(rule.Id))
                    continue;

                if (IsKnownVanillaComponent(rule.Id))
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
            switch (id)
            {
                case "MyObjectBuilder_Component/SteelPlate":
                case "MyObjectBuilder_Component/Construction":
                case "MyObjectBuilder_Component/InteriorPlate":
                case "MyObjectBuilder_Component/Girder":
                case "MyObjectBuilder_Component/SmallTube":
                case "MyObjectBuilder_Component/LargeTube":
                case "MyObjectBuilder_Component/MetalGrid":
                case "MyObjectBuilder_Component/BulletproofGlass":
                case "MyObjectBuilder_Component/Computer":
                case "MyObjectBuilder_Component/Display":
                case "MyObjectBuilder_Component/Detector":
                case "MyObjectBuilder_Component/RadioCommunication":
                case "MyObjectBuilder_Component/Motor":
                case "MyObjectBuilder_Component/Reactor":
                case "MyObjectBuilder_Component/Thrust":
                case "MyObjectBuilder_Component/Superconductor":
                case "MyObjectBuilder_Component/GravityGenerator":
                case "MyObjectBuilder_Component/Explosives":
                case "MyObjectBuilder_Component/Medical":
                case "MyObjectBuilder_Component/PowerCell":
                case "MyObjectBuilder_Component/SolarCell":
                    return true;
                default:
                    return false;
            }
        }

        private static void ParseIniLike(string text, StoreBlockConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(text))
                return;

            string currentSection = string.Empty;
            StoreItemRule currentItemRule = null;

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
