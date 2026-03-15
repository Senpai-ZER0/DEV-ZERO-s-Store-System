using System.Collections.Generic;
using ZeroStoreSystem.Config.Models;

namespace ZeroStoreSystem.Config
{
    public static class GlobalStoreConfigManager
    {
        private static readonly string[] VanillaComponentIds =
        {
            "MyObjectBuilder_Component/SteelPlate",
            "MyObjectBuilder_Component/Construction",
            "MyObjectBuilder_Component/InteriorPlate",
            "MyObjectBuilder_Component/Girder",
            "MyObjectBuilder_Component/SmallTube",
            "MyObjectBuilder_Component/LargeTube",
            "MyObjectBuilder_Component/MetalGrid",
            "MyObjectBuilder_Component/BulletproofGlass",
            "MyObjectBuilder_Component/Computer",
            "MyObjectBuilder_Component/Display",
            "MyObjectBuilder_Component/Detector",
            "MyObjectBuilder_Component/RadioCommunication",
            "MyObjectBuilder_Component/Motor",
            "MyObjectBuilder_Component/Reactor",
            "MyObjectBuilder_Component/Thrust",
            "MyObjectBuilder_Component/Superconductor",
            "MyObjectBuilder_Component/GravityGenerator",
            "MyObjectBuilder_Component/Explosives",
            "MyObjectBuilder_Component/Medical",
            "MyObjectBuilder_Component/PowerCell",
            "MyObjectBuilder_Component/SolarCell"
        };

        private static GlobalStoreConfig _cachedDefault;

        public static GlobalStoreConfig GetDefaultConfig()
        {
            if (_cachedDefault != null)
                return _cachedDefault;

            var config = new GlobalStoreConfig
            {
                AllowModdedItems = true,
                UseGlobalRules = true
            };

            foreach (var id in VanillaComponentIds)
            {
                config.GlobalItemRules.Add(new StoreItemRule
                {
                    Id = id,
                    Allowed = true,
                    ForceInclude = false,
                    Offer = new StoreOfferRule
                    {
                        Enabled = false,
                        PriceMod = 1f,
                        Amount = 0
                    },
                    Order = new StoreOrderRule
                    {
                        Enabled = false,
                        PriceMod = 1f,
                        Amount = 0
                    }
                });
            }

            SetOrReplace(config.GlobalItemRules, new StoreItemRule
            {
                Id = "MyObjectBuilder_Component/SteelPlate",
                Allowed = true,
                ForceInclude = false,
                Offer = new StoreOfferRule
                {
                    Enabled = true,
                    PriceMod = 1.15f,
                    Amount = 250
                },
                Order = new StoreOrderRule
                {
                    Enabled = false,
                    PriceMod = 1f,
                    Amount = 0
                }
            });

            SetOrReplace(config.GlobalItemRules, new StoreItemRule
            {
                Id = "MyObjectBuilder_Component/Construction",
                Allowed = true,
                ForceInclude = false,
                Offer = new StoreOfferRule
                {
                    Enabled = false,
                    PriceMod = 1f,
                    Amount = 0
                },
                Order = new StoreOrderRule
                {
                    Enabled = true,
                    PriceMod = 0.9f,
                    Amount = 150
                }
            });

            _cachedDefault = config;
            return _cachedDefault;
        }

        public static bool IsVanillaComponentId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            for (int i = 0; i < VanillaComponentIds.Length; i++)
            {
                if (VanillaComponentIds[i] == id)
                    return true;
            }

            return false;
        }

        private static void SetOrReplace(List<StoreItemRule> list, StoreItemRule rule)
        {
            if (list == null || rule == null || string.IsNullOrWhiteSpace(rule.Id))
                return;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].Id == rule.Id)
                {
                    list[i] = rule;
                    return;
                }
            }

            list.Add(rule);
        }
    }
}
