using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;

namespace ZeroStoreSystem.Profiles
{
    [XmlRoot("StoreGenerationConfig")]
    public class StoreGenerationConfigDefinition
    {
        public bool Enabled = true;
        public string FallbackProfileId = "Neutral";
        public bool UseBuiltInProfilesAsFallback = true;

        [XmlArray("TagAliases")]
        [XmlArrayItem("StoreGenerationTagAliasEntry")]
        public List<StoreGenerationTagAliasEntry> TagAliases = new List<StoreGenerationTagAliasEntry>();

        [XmlArray("CustomProfiles")]
        [XmlArrayItem("StoreGenerationProfileDefinition")]
        public List<StoreGenerationProfileDefinition> CustomProfiles = new List<StoreGenerationProfileDefinition>();
    }

    public class StoreGenerationTagAliasEntry
    {
        public string Tag = string.Empty;
        public string ProfileId = string.Empty;
    }

    public class StoreGenerationProfileDefinition
    {
        public string ProfileId = string.Empty;

        [XmlArray("Variants")]
        [XmlArrayItem("StoreProfileVariantDefinition")]
        public List<StoreProfileVariantDefinition> Variants = new List<StoreProfileVariantDefinition>();
    }

    public class StoreProfileVariantDefinition
    {
        public string VariantId = "Default";
        public string TradeMode = "BuyAndSell";
        public int RefreshIntervalSeconds = 0;
        public float OfferPriceMultiplier = 1.0f;
        public float OrderPriceMultiplier = 1.0f;
        public float OfferAmountMultiplier = 1.0f;
        public float OrderAmountMultiplier = 1.0f;
        public string PlayerServiceName = string.Empty;
        public string PlayerRestrictionsSummary = string.Empty;
        public string PlayerDetailsDescription = string.Empty;

        [XmlArray("AllowedCategories")]
        [XmlArrayItem("string")]
        public List<string> AllowedCategories = new List<string>();

        [XmlArray("OfferCategories")]
        [XmlArrayItem("string")]
        public List<string> OfferCategories = new List<string>();

        [XmlArray("OrderCategories")]
        [XmlArrayItem("string")]
        public List<string> OrderCategories = new List<string>();

        [XmlArray("ForceOfferItems")]
        [XmlArrayItem("string")]
        public List<string> ForceOfferItems = new List<string>();

        [XmlArray("ForceOrderItems")]
        [XmlArrayItem("string")]
        public List<string> ForceOrderItems = new List<string>();

        [XmlArray("ForbiddenItemIds")]
        [XmlArrayItem("string")]
        public List<string> ForbiddenItemIds = new List<string>();
    }

    public class StoreResolvedGenerationProfile
    {
        public string ProfileId = string.Empty;
        public string VariantId = "Default";
        public StoreTradeMode TradeMode = StoreTradeMode.BuyAndSell;
        public int RefreshIntervalSeconds = 0;
        public float OfferPriceMultiplier = 1.0f;
        public float OrderPriceMultiplier = 1.0f;
        public float OfferAmountMultiplier = 1.0f;
        public float OrderAmountMultiplier = 1.0f;
        public string PlayerServiceName = string.Empty;
        public string PlayerRestrictionsSummary = string.Empty;
        public string PlayerDetailsDescription = string.Empty;
        public readonly HashSet<StoreItemCategory> AllowedCategories = new HashSet<StoreItemCategory>();
        public readonly HashSet<StoreItemCategory> OfferCategories = new HashSet<StoreItemCategory>();
        public readonly HashSet<StoreItemCategory> OrderCategories = new HashSet<StoreItemCategory>();
        public readonly HashSet<string> ForceOfferItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<string> ForceOrderItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<string> ForbiddenItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public static class StoreGenerationProfileCatalog
    {
        private const string RelativePath = "Data/StoreData/StoreGenerationConfig.xml";

        private static bool _loaded;
        private static StoreGenerationConfigDefinition _config;
        private static readonly Dictionary<string, string> _aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, StoreGenerationProfileDefinition> _profiles = new Dictionary<string, StoreGenerationProfileDefinition>(StringComparer.OrdinalIgnoreCase);

        public static void Invalidate()
        {
            _loaded = false;
            _config = null;
            _aliases.Clear();
            _profiles.Clear();
        }

        public static StoreResolvedGenerationProfile ResolveForBlock(IMyTerminalBlock block)
        {
            EnsureLoaded();

            if (_config == null || !_config.Enabled || _profiles.Count == 0)
                return null;

            string source = GetTagSource(block);
            string profileId = ResolveProfileId(source);
            if (string.IsNullOrWhiteSpace(profileId))
                profileId = _config.FallbackProfileId;

            StoreGenerationProfileDefinition profile;
            if (!_profiles.TryGetValue(profileId, out profile) || profile == null)
            {
                if (!_profiles.TryGetValue(_config.FallbackProfileId, out profile) || profile == null)
                    return null;

                profileId = profile.ProfileId;
            }

            StoreProfileVariantDefinition variant = SelectVariant(block, profile);
            if (variant == null)
                return null;

            return BuildResolvedProfile(profileId, variant);
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
                return;

            _loaded = true;
            _config = new StoreGenerationConfigDefinition();

            try
            {
                var session = MyAPIGateway.Session;
                if (session != null && session.Mods != null)
                {
                    foreach (var mod in session.Mods)
                    {
                        try
                        {
                            if (!MyAPIGateway.Utilities.FileExistsInModLocation(RelativePath, mod))
                                continue;

                            using (var reader = MyAPIGateway.Utilities.ReadFileInModLocation(RelativePath, mod))
                            {
                                MergeConfig(Deserialize(reader.ReadToEnd()));
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error("Failed to read StoreGenerationConfig from mod '" + mod.Name + "': " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("StoreGenerationProfileCatalog load failed: " + e);
            }

            BuildLookups();
        }

        private static StoreGenerationConfigDefinition Deserialize(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            try
            {
                return MyAPIGateway.Utilities.SerializeFromXML<StoreGenerationConfigDefinition>(xml);
            }
            catch (Exception e)
            {
                Log.Error("Failed to deserialize StoreGenerationConfig.xml: " + e.Message);
                return null;
            }
        }

        private static void MergeConfig(StoreGenerationConfigDefinition other)
        {
            if (other == null)
                return;

            _config.Enabled = other.Enabled;

            if (!string.IsNullOrWhiteSpace(other.FallbackProfileId))
                _config.FallbackProfileId = other.FallbackProfileId;

            _config.UseBuiltInProfilesAsFallback = other.UseBuiltInProfilesAsFallback;

            if (other.TagAliases != null)
                _config.TagAliases.AddRange(other.TagAliases);

            if (other.CustomProfiles != null)
                _config.CustomProfiles.AddRange(other.CustomProfiles);
        }

        private static void BuildLookups()
        {
            _aliases.Clear();
            _profiles.Clear();

            if (_config == null)
                _config = new StoreGenerationConfigDefinition();

            if (_config.UseBuiltInProfilesAsFallback || _config.CustomProfiles == null || _config.CustomProfiles.Count == 0)
            {
                foreach (var alias in CreateBuiltInAliases())
                {
                    if (alias != null && !string.IsNullOrWhiteSpace(alias.Tag) && !string.IsNullOrWhiteSpace(alias.ProfileId))
                        _aliases[alias.Tag] = alias.ProfileId;
                }

                foreach (var profile in CreateBuiltInProfiles())
                {
                    if (profile != null && !string.IsNullOrWhiteSpace(profile.ProfileId))
                        _profiles[profile.ProfileId] = profile;
                }
            }

            if (_config.TagAliases != null)
            {
                foreach (var alias in _config.TagAliases)
                {
                    if (alias != null && !string.IsNullOrWhiteSpace(alias.Tag) && !string.IsNullOrWhiteSpace(alias.ProfileId))
                        _aliases[alias.Tag] = alias.ProfileId;
                }
            }

            if (_config.CustomProfiles != null)
            {
                foreach (var profile in _config.CustomProfiles)
                {
                    if (profile != null && !string.IsNullOrWhiteSpace(profile.ProfileId))
                        _profiles[profile.ProfileId] = profile;
                }
            }

            if (string.IsNullOrWhiteSpace(_config.FallbackProfileId) || !_profiles.ContainsKey(_config.FallbackProfileId))
            {
                foreach (var kv in _profiles)
                {
                    _config.FallbackProfileId = kv.Key;
                    break;
                }
            }
        }

        private static string ResolveProfileId(string source)
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                foreach (var pair in _aliases)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Key)
                        && source.IndexOf(pair.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return pair.Value;
                    }
                }

                foreach (var pair in _profiles)
                {
                    string tag = "[" + pair.Key + "]";
                    if (source.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0)
                        return pair.Key;
                }
            }

            return _config != null ? _config.FallbackProfileId : string.Empty;
        }

        private static string GetTagSource(IMyTerminalBlock block)
        {
            if (block == null)
                return string.Empty;

            string blockName = block.CustomName ?? string.Empty;
            string gridName = block.CubeGrid != null ? (block.CubeGrid.DisplayName ?? string.Empty) : string.Empty;
            return blockName + " " + gridName;
        }

        private static StoreProfileVariantDefinition SelectVariant(IMyTerminalBlock block, StoreGenerationProfileDefinition profile)
        {
            if (profile == null || profile.Variants == null || profile.Variants.Count == 0)
                return null;

            if (profile.Variants.Count == 1)
                return profile.Variants[0];

            ulong seed = block != null ? (ulong)block.EntityId : 0UL;
            if (block != null && block.CubeGrid != null)
                seed ^= (ulong)block.CubeGrid.EntityId;

            int index = (int)(seed % (ulong)profile.Variants.Count);
            return profile.Variants[index];
        }

        private static StoreResolvedGenerationProfile BuildResolvedProfile(string profileId, StoreProfileVariantDefinition variant)
        {
            var resolved = new StoreResolvedGenerationProfile();
            resolved.ProfileId = profileId ?? string.Empty;
            resolved.VariantId = !string.IsNullOrWhiteSpace(variant.VariantId) ? variant.VariantId : "Default";
            resolved.TradeMode = ParseTradeMode(variant.TradeMode);
            resolved.RefreshIntervalSeconds = Math.Max(0, variant.RefreshIntervalSeconds);
            resolved.OfferPriceMultiplier = variant.OfferPriceMultiplier <= 0f ? 1f : variant.OfferPriceMultiplier;
            resolved.OrderPriceMultiplier = variant.OrderPriceMultiplier <= 0f ? 1f : variant.OrderPriceMultiplier;
            resolved.OfferAmountMultiplier = variant.OfferAmountMultiplier <= 0f ? 1f : variant.OfferAmountMultiplier;
            resolved.OrderAmountMultiplier = variant.OrderAmountMultiplier <= 0f ? 1f : variant.OrderAmountMultiplier;
            resolved.PlayerServiceName = variant.PlayerServiceName ?? string.Empty;
            resolved.PlayerRestrictionsSummary = variant.PlayerRestrictionsSummary ?? string.Empty;
            resolved.PlayerDetailsDescription = variant.PlayerDetailsDescription ?? string.Empty;

            AddCategories(resolved.AllowedCategories, variant.AllowedCategories);
            AddCategories(resolved.OfferCategories, variant.OfferCategories);
            AddCategories(resolved.OrderCategories, variant.OrderCategories);
            AddStrings(resolved.ForceOfferItems, variant.ForceOfferItems);
            AddStrings(resolved.ForceOrderItems, variant.ForceOrderItems);
            AddStrings(resolved.ForbiddenItemIds, variant.ForbiddenItemIds);

            return resolved;
        }

        private static void AddStrings(HashSet<string> target, List<string> values)
        {
            if (target == null || values == null)
                return;

            for (int i = 0; i < values.Count; i++)
            {
                string value = values[i];
                if (!string.IsNullOrWhiteSpace(value))
                    target.Add(value.Trim());
            }
        }

        private static void AddCategories(HashSet<StoreItemCategory> target, List<string> rawValues)
        {
            if (target == null || rawValues == null)
                return;

            for (int i = 0; i < rawValues.Count; i++)
            {
                StoreItemCategory category;
                if (TryParseCategory(rawValues[i], out category))
                    target.Add(category);
            }
        }

        private static bool TryParseCategory(string raw, out StoreItemCategory category)
        {
            category = StoreItemCategory.Unknown;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            switch (raw.Trim().ToLowerInvariant())
            {
                case "component":
                case "components":
                    category = StoreItemCategory.Component;
                    return true;
                case "ingot":
                case "ingots":
                    category = StoreItemCategory.Ingot;
                    return true;
                case "ore":
                case "ores":
                    category = StoreItemCategory.Ore;
                    return true;
                case "ammo":
                    category = StoreItemCategory.Ammo;
                    return true;
                case "tool":
                case "tools":
                    category = StoreItemCategory.Tool;
                    return true;
                case "bottle":
                case "bottles":
                    category = StoreItemCategory.Bottle;
                    return true;
                case "consumable":
                case "consumables":
                    category = StoreItemCategory.Consumable;
                    return true;
                case "power":
                    category = StoreItemCategory.Power;
                    return true;
                default:
                    return false;
            }
        }

        private static StoreTradeMode ParseTradeMode(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return StoreTradeMode.BuyAndSell;

            switch (raw.Trim())
            {
                case "BuyOnly":
                    return StoreTradeMode.BuyOnly;
                case "SellOnly":
                    return StoreTradeMode.SellOnly;
                default:
                    return StoreTradeMode.BuyAndSell;
            }
        }

        private static List<StoreGenerationTagAliasEntry> CreateBuiltInAliases()
        {
            return new List<StoreGenerationTagAliasEntry>
            {
                new StoreGenerationTagAliasEntry { Tag = "[NEUT]", ProfileId = "Neutral" },
                new StoreGenerationTagAliasEntry { Tag = "[NEUTRAL]", ProfileId = "Neutral" },
                new StoreGenerationTagAliasEntry { Tag = "[IND]", ProfileId = "Industrial" },
                new StoreGenerationTagAliasEntry { Tag = "[INDUSTRIAL]", ProfileId = "Industrial" },
                new StoreGenerationTagAliasEntry { Tag = "[YARD]", ProfileId = "Industrial" },
                new StoreGenerationTagAliasEntry { Tag = "[SHIPYARD]", ProfileId = "Industrial" },
                new StoreGenerationTagAliasEntry { Tag = "[CIV]", ProfileId = "Civilian" },
                new StoreGenerationTagAliasEntry { Tag = "[CIVIL]", ProfileId = "Civilian" },
                new StoreGenerationTagAliasEntry { Tag = "[TRADE]", ProfileId = "Civilian" },
                new StoreGenerationTagAliasEntry { Tag = "[SERVICE]", ProfileId = "Civilian" },
                new StoreGenerationTagAliasEntry { Tag = "[MIL]", ProfileId = "Military" },
                new StoreGenerationTagAliasEntry { Tag = "[MILITARY]", ProfileId = "Military" },
                new StoreGenerationTagAliasEntry { Tag = "[DEFENSE]", ProfileId = "Military" },
                new StoreGenerationTagAliasEntry { Tag = "[PREM]", ProfileId = "Premium" },
                new StoreGenerationTagAliasEntry { Tag = "[PREMIUM]", ProfileId = "Premium" },
                new StoreGenerationTagAliasEntry { Tag = "[LUX]", ProfileId = "Premium" }
            };
        }

        private static List<StoreGenerationProfileDefinition> CreateBuiltInProfiles()
        {
            return new List<StoreGenerationProfileDefinition>
            {
                new StoreGenerationProfileDefinition
                {
                    ProfileId = "Neutral",
                    Variants = new List<StoreProfileVariantDefinition>
                    {
                        new StoreProfileVariantDefinition
                        {
                            VariantId = "Default",
                            TradeMode = "BuyAndSell",
                            AllowedCategories = CreateCategories("Components", "Ingots", "Ores", "Ammo", "Tools", "Bottles", "Consumables", "Power"),
                            OfferCategories = CreateCategories("Components", "Tools", "Bottles", "Consumables", "Power"),
                            OrderCategories = CreateCategories("Components", "Ingots", "Ores"),
                            ForceOfferItems = CreateIds("MyObjectBuilder_Component/SteelPlate", "MyObjectBuilder_ConsumableItem/ClangCola"),
                            ForceOrderItems = CreateIds("MyObjectBuilder_Component/Construction"),
                            PlayerServiceName = "General Store",
                            PlayerRestrictionsSummary = "Balanced civilian supply",
                            PlayerDetailsDescription = "Balanced mixed inventory for general-purpose neutral stations.",
                            OfferAmountMultiplier = 1.0f,
                            OrderAmountMultiplier = 1.0f
                        }
                    }
                },
                new StoreGenerationProfileDefinition
                {
                    ProfileId = "Industrial",
                    Variants = new List<StoreProfileVariantDefinition>
                    {
                        new StoreProfileVariantDefinition
                        {
                            VariantId = "Yard",
                            TradeMode = "BuyAndSell",
                            AllowedCategories = CreateCategories("Components", "Ingots", "Ores", "Tools", "Bottles", "Power"),
                            OfferCategories = CreateCategories("Components", "Ingots", "Tools", "Bottles", "Power"),
                            OrderCategories = CreateCategories("Components", "Ingots", "Ores"),
                            ForceOfferItems = CreateIds("MyObjectBuilder_Component/SteelPlate", "MyObjectBuilder_Component/Construction"),
                            ForceOrderItems = CreateIds("MyObjectBuilder_Ore/Stone", "MyObjectBuilder_Ore/Iron"),
                            PlayerServiceName = "Industrial Yard",
                            PlayerRestrictionsSummary = "Heavy materials and practical utility stock",
                            PlayerDetailsDescription = "Industrial stations focus on construction, raw materials and service tools.",
                            OfferAmountMultiplier = 1.35f,
                            OrderAmountMultiplier = 1.25f
                        }
                    }
                },
                new StoreGenerationProfileDefinition
                {
                    ProfileId = "Civilian",
                    Variants = new List<StoreProfileVariantDefinition>
                    {
                        new StoreProfileVariantDefinition
                        {
                            VariantId = "Market",
                            TradeMode = "BuyAndSell",
                            AllowedCategories = CreateCategories("Components", "Ingots", "Ores", "Tools", "Bottles", "Consumables", "Power"),
                            OfferCategories = CreateCategories("Components", "Tools", "Bottles", "Consumables", "Power"),
                            OrderCategories = CreateCategories("Ingots", "Ores", "Components"),
                            ForceOfferItems = CreateIds("MyObjectBuilder_ConsumableItem/ClangCola", "MyObjectBuilder_ConsumableItem/CosmicCoffee", "MyObjectBuilder_ConsumableItem/Medkit"),
                            PlayerServiceName = "Civilian Market",
                            PlayerRestrictionsSummary = "No dedicated munitions catalogue",
                            PlayerDetailsDescription = "Civilian stations emphasize life-support, tools and common maintenance goods.",
                            OfferAmountMultiplier = 1.1f,
                            OrderAmountMultiplier = 0.9f
                        }
                    }
                },
                new StoreGenerationProfileDefinition
                {
                    ProfileId = "Military",
                    Variants = new List<StoreProfileVariantDefinition>
                    {
                        new StoreProfileVariantDefinition
                        {
                            VariantId = "Armory",
                            TradeMode = "SellOnly",
                            AllowedCategories = CreateCategories("Components", "Ammo", "Tools", "Bottles", "Power", "Consumables"),
                            OfferCategories = CreateCategories("Components", "Ammo", "Tools", "Bottles", "Power", "Consumables"),
                            ForceOfferItems = CreateIds("MyObjectBuilder_AmmoMagazine/NATO_25x184mm", "MyObjectBuilder_ConsumableItem/Medkit"),
                            PlayerServiceName = "Military Supply",
                            PlayerRestrictionsSummary = "Focused on combat readiness and field sustainment",
                            PlayerDetailsDescription = "Military stations stock ammunition, combat support gear and critical replacement parts.",
                            OfferPriceMultiplier = 1.15f,
                            OfferAmountMultiplier = 0.85f
                        }
                    }
                },
                new StoreGenerationProfileDefinition
                {
                    ProfileId = "Premium",
                    Variants = new List<StoreProfileVariantDefinition>
                    {
                        new StoreProfileVariantDefinition
                        {
                            VariantId = "FullService",
                            TradeMode = "BuyAndSell",
                            AllowedCategories = CreateCategories("Components", "Ingots", "Ores", "Ammo", "Tools", "Bottles", "Consumables", "Power"),
                            OfferCategories = CreateCategories("Components", "Ingots", "Ammo", "Tools", "Bottles", "Consumables", "Power"),
                            OrderCategories = CreateCategories("Components", "Ingots", "Ores", "Ammo", "Consumables", "Power"),
                            ForceOfferItems = CreateIds("MyObjectBuilder_ConsumableItem/Medkit", "MyObjectBuilder_ConsumableItem/Powerkit"),
                            PlayerServiceName = "Premium Exchange",
                            PlayerRestrictionsSummary = "Wide catalogue with premium pricing",
                            PlayerDetailsDescription = "Premium stations expose a broad stock list with higher throughput and pricing.",
                            OfferPriceMultiplier = 1.2f,
                            OrderPriceMultiplier = 1.05f,
                            OfferAmountMultiplier = 1.25f,
                            OrderAmountMultiplier = 1.1f
                        }
                    }
                }
            };
        }

        private static List<string> CreateCategories(params string[] values)
        {
            return new List<string>(values ?? new string[0]);
        }

        private static List<string> CreateIds(params string[] values)
        {
            return new List<string>(values ?? new string[0]);
        }
    }
}
