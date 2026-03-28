using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.ShipOffers;
using ZeroStoreSystem.ShipOffers.Models;

namespace ZeroStoreSystem.Core
{
    public enum StoreItemCategory
    {
        Unknown = 0,
        Component = 1,
        Ingot = 2,
        Ore = 3,
        Ammo = 4,
        Tool = 5,
        Bottle = 6,
        Consumable = 7,
        Power = 8,
        Ships = 9
    }

    public class StoreCatalogItem
    {
        public string Id;
        public StoreItemCategory Category;
        public bool IsVanilla;
    }

    public static class StoreItemCatalog
    {
        public static readonly string[] VanillaComponentIds =
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

        public static readonly string[] VanillaIngotIds =
        {
            "MyObjectBuilder_Ingot/Iron",
            "MyObjectBuilder_Ingot/Nickel",
            "MyObjectBuilder_Ingot/Cobalt",
            "MyObjectBuilder_Ingot/Silicon",
            "MyObjectBuilder_Ingot/Magnesium",
            "MyObjectBuilder_Ingot/Silver",
            "MyObjectBuilder_Ingot/Gold",
            "MyObjectBuilder_Ingot/Platinum",
            "MyObjectBuilder_Ingot/Uranium",
            "MyObjectBuilder_Ingot/Stone",
            "MyObjectBuilder_Ingot/Scrap"
        };

        public static readonly string[] VanillaOreIds =
        {
            "MyObjectBuilder_Ore/Stone",
            "MyObjectBuilder_Ore/Iron",
            "MyObjectBuilder_Ore/Nickel",
            "MyObjectBuilder_Ore/Cobalt",
            "MyObjectBuilder_Ore/Silicon",
            "MyObjectBuilder_Ore/Magnesium",
            "MyObjectBuilder_Ore/Silver",
            "MyObjectBuilder_Ore/Gold",
            "MyObjectBuilder_Ore/Platinum",
            "MyObjectBuilder_Ore/Uranium",
            "MyObjectBuilder_Ore/Ice",
            "MyObjectBuilder_Ore/Scrap"
        };

        public static readonly string[] VanillaAmmoIds =
        {
            "MyObjectBuilder_AmmoMagazine/NATO_5p56x45mm",
            "MyObjectBuilder_AmmoMagazine/NATO_25x184mm",
            "MyObjectBuilder_AmmoMagazine/Missile200mm",
            "MyObjectBuilder_AmmoMagazine/AutocannonClip",
            "MyObjectBuilder_AmmoMagazine/SmallRailgunAmmoContainer",
            "MyObjectBuilder_AmmoMagazine/LargeRailgunAmmoContainer",
            "MyObjectBuilder_AmmoMagazine/AutomaticRifleGun_Mag_20rd",
            "MyObjectBuilder_AmmoMagazine/PreciseAutomaticRifleGun_Mag_5rd",
            "MyObjectBuilder_AmmoMagazine/RapidFireAutomaticRifleGun_Mag_50rd",
            "MyObjectBuilder_AmmoMagazine/UltimateAutomaticRifleGun_Mag_30rd",
            "MyObjectBuilder_AmmoMagazine/SemiAutoPistolMagazine",
            "MyObjectBuilder_AmmoMagazine/FullAutoPistolMagazine",
            "MyObjectBuilder_AmmoMagazine/ElitePistolMagazine"
        };

        public static readonly string[] VanillaToolIds =
        {
            "MyObjectBuilder_PhysicalGunObject/WelderItem",
            "MyObjectBuilder_PhysicalGunObject/Welder2Item",
            "MyObjectBuilder_PhysicalGunObject/Welder3Item",
            "MyObjectBuilder_PhysicalGunObject/Welder4Item",
            "MyObjectBuilder_PhysicalGunObject/AngleGrinderItem",
            "MyObjectBuilder_PhysicalGunObject/AngleGrinder2Item",
            "MyObjectBuilder_PhysicalGunObject/AngleGrinder3Item",
            "MyObjectBuilder_PhysicalGunObject/AngleGrinder4Item",
            "MyObjectBuilder_PhysicalGunObject/HandDrillItem",
            "MyObjectBuilder_PhysicalGunObject/HandDrill2Item",
            "MyObjectBuilder_PhysicalGunObject/HandDrill3Item",
            "MyObjectBuilder_PhysicalGunObject/HandDrill4Item"
        };

        public static readonly string[] VanillaBottleIds =
        {
            "MyObjectBuilder_OxygenContainerObject/OxygenBottle",
            "MyObjectBuilder_GasContainerObject/HydrogenBottle"
        };

        public static readonly string[] VanillaConsumableIds =
        {
            "MyObjectBuilder_ConsumableItem/Medkit",
            "MyObjectBuilder_ConsumableItem/ClangCola",
            "MyObjectBuilder_ConsumableItem/CosmicCoffee",
            "MyObjectBuilder_Package/Package"
        };

        public static readonly string[] VanillaPowerIds =
        {
            "MyObjectBuilder_ConsumableItem/Powerkit"
        };

        public static readonly string[] KnownShipOfferIds =
        {
            "MyObjectBuilder_Component/SCC Zeus MKI",
            "MyObjectBuilder_Component/ATV-Survivor"
        };

        public static IEnumerable<string> EnumerateKnownVanillaIds()
        {
            int i;
            for (i = 0; i < VanillaComponentIds.Length; i++)
                yield return VanillaComponentIds[i];
            for (i = 0; i < VanillaIngotIds.Length; i++)
                yield return VanillaIngotIds[i];
            for (i = 0; i < VanillaOreIds.Length; i++)
                yield return VanillaOreIds[i];
            for (i = 0; i < VanillaAmmoIds.Length; i++)
                yield return VanillaAmmoIds[i];
            for (i = 0; i < VanillaToolIds.Length; i++)
                yield return VanillaToolIds[i];
            for (i = 0; i < VanillaBottleIds.Length; i++)
                yield return VanillaBottleIds[i];
            for (i = 0; i < VanillaConsumableIds.Length; i++)
                yield return VanillaConsumableIds[i];
            for (i = 0; i < VanillaPowerIds.Length; i++)
                yield return VanillaPowerIds[i];
        }

        public static IEnumerable<StoreCatalogItem> EnumerateCatalogItems()
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string id in EnumerateKnownVanillaIds())
            {
                if (seen.Add(id))
                {
                    yield return new StoreCatalogItem
                    {
                        Id = id,
                        Category = GetCategory(id),
                        IsVanilla = true
                    };
                }
            }

            foreach (string id in EnumerateRuntimeDefinitionIds())
            {
                if (!seen.Add(id))
                    continue;

                StoreItemCategory category = GetCategory(id);
                if (category == StoreItemCategory.Unknown || category == StoreItemCategory.Ships)
                    continue;

                yield return new StoreCatalogItem
                {
                    Id = id,
                    Category = category,
                    IsVanilla = IsVanilla(id)
                };
            }

            List<ShipStoreOfferDefinition> offers = ShipStoreOfferCatalog.GetOffers();
            if (offers != null)
            {
                int i;
                for (i = 0; i < offers.Count; i++)
                {
                    ShipStoreOfferDefinition offer = offers[i];
                    if (offer == null)
                        continue;

                    string tokenId = offer.TokenItemId;
                    if (string.IsNullOrWhiteSpace(tokenId) && !string.IsNullOrWhiteSpace(offer.PrefabSubtypeId))
                        tokenId = "MyObjectBuilder_Component/" + offer.PrefabSubtypeId;

                    if (string.IsNullOrWhiteSpace(tokenId) || !seen.Add(tokenId))
                        continue;

                    yield return new StoreCatalogItem
                    {
                        Id = tokenId,
                        Category = StoreItemCategory.Ships,
                        IsVanilla = offer.IsVanilla
                    };
                }
            }
        }

        public static StoreItemCategory GetCategory(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return StoreItemCategory.Unknown;

            if (Contains(KnownShipOfferIds, id))
                return StoreItemCategory.Ships;
            if (Contains(VanillaToolIds, id) || id.StartsWith("MyObjectBuilder_PhysicalGunObject/", StringComparison.OrdinalIgnoreCase))
                return StoreItemCategory.Tool;
            if (Contains(VanillaBottleIds, id)
                || id.StartsWith("MyObjectBuilder_OxygenContainerObject/", StringComparison.OrdinalIgnoreCase)
                || id.StartsWith("MyObjectBuilder_GasContainerObject/", StringComparison.OrdinalIgnoreCase))
                return StoreItemCategory.Bottle;
            if (Contains(VanillaPowerIds, id)
                || EndsWithAny(id, "/Powerkit", "/PowerKit")
                || ContainsAny(id, "battery", "cell", "powerkit", "energykit"))
                return StoreItemCategory.Power;
            if (Contains(VanillaConsumableIds, id)
                || id.StartsWith("MyObjectBuilder_ConsumableItem/", StringComparison.OrdinalIgnoreCase)
                || id.StartsWith("MyObjectBuilder_Package/", StringComparison.OrdinalIgnoreCase)
                || ContainsAny(id, "medkit", "cola", "coffee", "ration", "meal", "food", "drink"))
                return StoreItemCategory.Consumable;
            if (id.StartsWith("MyObjectBuilder_Component/", StringComparison.OrdinalIgnoreCase))
                return StoreItemCategory.Component;
            if (id.StartsWith("MyObjectBuilder_Ingot/", StringComparison.OrdinalIgnoreCase))
                return StoreItemCategory.Ingot;
            if (id.StartsWith("MyObjectBuilder_Ore/", StringComparison.OrdinalIgnoreCase))
                return StoreItemCategory.Ore;
            if (id.StartsWith("MyObjectBuilder_AmmoMagazine/", StringComparison.OrdinalIgnoreCase))
                return StoreItemCategory.Ammo;

            return StoreItemCategory.Unknown;
        }


        private static IEnumerable<string> EnumerateRuntimeDefinitionIds()
        {
            MyDefinitionManager manager = MyDefinitionManager.Static;
            if (manager == null)
                yield break;

            foreach (var definition in manager.GetAllDefinitions())
            {
                if (definition == null)
                    continue;

                string id = ToCatalogId(definition.Id);
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (!IsSupportedCatalogType(id))
                    continue;

                yield return id;
            }
        }

        private static bool IsSupportedCatalogType(string id)
        {
            return id.StartsWith("MyObjectBuilder_Component/", StringComparison.OrdinalIgnoreCase)
                || id.StartsWith("MyObjectBuilder_Ingot/", StringComparison.OrdinalIgnoreCase)
                || id.StartsWith("MyObjectBuilder_Ore/", StringComparison.OrdinalIgnoreCase)
                || id.StartsWith("MyObjectBuilder_AmmoMagazine/", StringComparison.OrdinalIgnoreCase)
                || id.StartsWith("MyObjectBuilder_PhysicalGunObject/", StringComparison.OrdinalIgnoreCase)
                || id.StartsWith("MyObjectBuilder_OxygenContainerObject/", StringComparison.OrdinalIgnoreCase)
                || id.StartsWith("MyObjectBuilder_GasContainerObject/", StringComparison.OrdinalIgnoreCase)
                || id.StartsWith("MyObjectBuilder_ConsumableItem/", StringComparison.OrdinalIgnoreCase)
                || id.StartsWith("MyObjectBuilder_Package/", StringComparison.OrdinalIgnoreCase);
        }

        private static string ToCatalogId(MyDefinitionId definitionId)
        {
            string typeId = definitionId.TypeId.ToString();
            string subtypeId = definitionId.SubtypeName;
            if (string.IsNullOrWhiteSpace(typeId) || string.IsNullOrWhiteSpace(subtypeId))
                return string.Empty;

            return typeId + "/" + subtypeId;
        }

        public static bool IsVanilla(string id)
        {
            return Contains(VanillaComponentIds, id)
                || Contains(VanillaIngotIds, id)
                || Contains(VanillaOreIds, id)
                || Contains(VanillaAmmoIds, id)
                || Contains(VanillaToolIds, id)
                || Contains(VanillaBottleIds, id)
                || Contains(VanillaConsumableIds, id)
                || Contains(VanillaPowerIds, id)
                || Contains(KnownShipOfferIds, id);
        }

        public static int GetCategorySortOrder(StoreItemCategory category)
        {
            switch (category)
            {
                case StoreItemCategory.Component: return 0;
                case StoreItemCategory.Ingot: return 1;
                case StoreItemCategory.Ore: return 2;
                case StoreItemCategory.Ammo: return 3;
                case StoreItemCategory.Tool: return 4;
                case StoreItemCategory.Bottle: return 5;
                case StoreItemCategory.Consumable: return 6;
                case StoreItemCategory.Power: return 7;
                case StoreItemCategory.Ships: return 8;
                default: return 9;
            }
        }


        public static int GetSuggestedAmount(string id)
        {
            switch (GetCategory(id))
            {
                case StoreItemCategory.Component: return 100;
                case StoreItemCategory.Ingot: return 250;
                case StoreItemCategory.Ore: return 500;
                case StoreItemCategory.Ammo: return 50;
                case StoreItemCategory.Tool: return 2;
                case StoreItemCategory.Bottle: return 8;
                case StoreItemCategory.Consumable: return 15;
                case StoreItemCategory.Power: return 10;
                default: return 10;
            }
        }

        public static StoreItemRule CreateDefaultRule(string id)
        {
            return new StoreItemRule
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
            };
        }

        private static bool Contains(string[] ids, string id)
        {
            if (ids == null || string.IsNullOrWhiteSpace(id))
                return false;

            int i;
            for (i = 0; i < ids.Length; i++)
            {
                if (string.Equals(ids[i], id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool ContainsAny(string id, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(id) || tokens == null)
                return false;

            int i;
            for (i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (!string.IsNullOrWhiteSpace(token) && id.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static bool EndsWithAny(string id, params string[] suffixes)
        {
            if (string.IsNullOrWhiteSpace(id) || suffixes == null)
                return false;

            int i;
            for (i = 0; i < suffixes.Length; i++)
            {
                string suffix = suffixes[i];
                if (!string.IsNullOrWhiteSpace(suffix) && id.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
