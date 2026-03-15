using System;
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
                    Log.Info("Default CustomData written for block '" + block.CustomName + "'");
                    return config;
                }

                ParseIniLike(block.CustomData, config);
                Log.Info("Store config loaded for '" + block.CustomName + "', TradeMode=" + config.TradeMode + ", RefreshIntervalSeconds=" + config.RefreshIntervalSeconds);
            }
            catch (Exception e)
            {
                Log.Error("Failed to read config for '" + (block != null ? block.CustomName : "<null>") + "': " + e);
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

            block.CustomData = sb.ToString();
        }

        private static void ParseIniLike(string text, StoreBlockConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(text))
                return;

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("["))
                    continue;

                if (line.StartsWith(";") || line.StartsWith("#"))
                    continue;

                int separator = line.IndexOf('=');
                if (separator <= 0)
                    continue;

                string key = line.Substring(0, separator).Trim();
                string value = line.Substring(separator + 1).Trim();

                switch (key)
                {
                    case "Enabled":
                        bool enabled;
                        if (bool.TryParse(value, out enabled))
                            config.Enabled = enabled;
                        break;

                    case "UseAutoProfile":
                        bool useAutoProfile;
                        if (bool.TryParse(value, out useAutoProfile))
                            config.UseAutoProfile = useAutoProfile;
                        break;

                    case "ProfileId":
                        config.ProfileId = value ?? string.Empty;
                        break;

                    case "TradeMode":
                        config.TradeMode = ParseTradeMode(value);
                        break;

                    case "RefreshIntervalSeconds":
                        int refreshIntervalSeconds;
                        if (int.TryParse(value, out refreshIntervalSeconds))
                            config.RefreshIntervalSeconds = Math.Max(0, refreshIntervalSeconds);
                        break;
                }
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
    }
}
