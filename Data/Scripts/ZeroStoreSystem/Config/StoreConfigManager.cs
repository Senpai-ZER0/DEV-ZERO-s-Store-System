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

        public static void WriteDefaultBlockConfig(IMyTerminalBlock block)
        {
            if (block == null)
                return;

            var sb = new StringBuilder();

            sb.AppendLine("[Store]");
            sb.AppendLine("Enabled=true");
            sb.AppendLine("UseAutoProfile=true");
            sb.AppendLine("ProfileId=");
            sb.AppendLine("TradeMode=BuyAndSell");
            sb.AppendLine("RefreshIntervalSeconds=0");
            sb.AppendLine();

            sb.AppendLine("; ===== Components: structural =====");
            AppendItemRule(sb, "MyObjectBuilder_Component/SteelPlate", true, true, true, 1.0f, 200, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/Construction", true, true, false, 1.0f, 0, true, 1.0f, 150);
            AppendItemRule(sb, "MyObjectBuilder_Component/InteriorPlate", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/Girder", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/SmallTube", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/LargeTube", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/MetalGrid", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/BulletproofGlass", true, false, false, 1.0f, 0, false, 1.0f, 0);

            sb.AppendLine("; ===== Components: electronics =====");
            AppendItemRule(sb, "MyObjectBuilder_Component/Computer", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/Display", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/Detector", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/RadioCommunication", true, false, false, 1.0f, 0, false, 1.0f, 0);

            sb.AppendLine("; ===== Components: machinery =====");
            AppendItemRule(sb, "MyObjectBuilder_Component/Motor", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/Reactor", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/Thrust", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/Superconductor", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/GravityGenerator", true, false, false, 1.0f, 0, false, 1.0f, 0);

            sb.AppendLine("; ===== Components: specialized =====");
            AppendItemRule(sb, "MyObjectBuilder_Component/Explosives", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/Medical", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/PowerCell", true, false, false, 1.0f, 0, false, 1.0f, 0);
            AppendItemRule(sb, "MyObjectBuilder_Component/SolarCell", true, false, false, 1.0f, 0, false, 1.0f, 0);

            block.CustomData = sb.ToString();
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
