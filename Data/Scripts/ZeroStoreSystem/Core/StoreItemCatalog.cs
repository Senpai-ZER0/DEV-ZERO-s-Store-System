using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game.Definitions;
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
        Ship = 5
    }

    public struct StoreCatalogItem
    {
        public string Id;
        public string ShortName;
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

        private static readonly List<StoreCatalogItem> _cachedItems = new List<StoreCatalogItem>();
        private static bool _isBuilt;

        public static IEnumerable<string> KnownShipOfferIds
        {
            get
            {
                foreach (ShipStoreOfferDefinition offer in ShipStoreOfferCatalog.GetOffers())
                {
                    if (offer != null && !string.IsNullOrWhiteSpace(offer.Id))
                        yield return offer.Id;
                }
            }
        }

        public static void Invalidate()
        {
            _cachedItems.Clear();
            _isBuilt = false;
        }

        public static IEnumerable<StoreCatalogItem> EnumerateCatalogItems()
        {
            EnsureBuilt();
            return _cachedItems;
        }

        public static IEnumerable<string> EnumerateKnownVanillaIds()
        {
            foreach (var item in EnumerateCatalogItems())
            {
                if (item.IsVanilla && item.Category != StoreItemCategory.Ship)
                    yield return item.Id;
            }
        }

        public static StoreItemCategory GetCategory(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return StoreItemCategory.Unknown;

            if (id.StartsWith("MyObjectBuilder_Component/", StringComparison.OrdinalIgnoreCase))
                return StoreItemCategory.Component;

            if (id.StartsWith("MyObjectBuilder_Ingot/", StringComparison.OrdinalIgnoreCase))
                return StoreItemCategory.Ingot;

            if (id.StartsWith("MyObjectBuilder_Ore/", StringComparison.OrdinalIgnoreCase))
                return StoreItemCategory.Ore;

            if (id.StartsWith("MyObjectBuilder_AmmoMagazine/", StringComparison.OrdinalIgnoreCase))
                return StoreItemCategory.Ammo;

            ShipStoreOfferDefinition offer;
            if (ShipStoreOfferCatalog.TryGetById(id, out offer))
                return StoreItemCategory.Ship;

            return StoreItemCategory.Unknown;
        }

        public static bool IsVanilla(string id)
        {
            if (Contains(VanillaComponentIds, id)
                || Contains(VanillaIngotIds, id)
                || Contains(VanillaOreIds, id)
                || Contains(VanillaAmmoIds, id))
                return true;

            ShipStoreOfferDefinition offer;
            if (ShipStoreOfferCatalog.TryGetById(id, out offer))
                return offer != null && offer.IsVanilla;

            return false;
        }

        public static int GetCategorySortOrder(StoreItemCategory category)
        {
            switch (category)
            {
                case StoreItemCategory.Component:
                    return 0;
                case StoreItemCategory.Ingot:
                    return 1;
                case StoreItemCategory.Ore:
                    return 2;
                case StoreItemCategory.Ammo:
                    return 3;
                case StoreItemCategory.Ship:
                    return 4;
                default:
                    return 5;
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

        private static void EnsureBuilt()
        {
            if (_isBuilt)
                return;

            _cachedItems.Clear();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AddKnownVanilla(_cachedItems, seen, VanillaComponentIds, StoreItemCategory.Component);
            AddKnownVanilla(_cachedItems, seen, VanillaIngotIds, StoreItemCategory.Ingot);
            AddKnownVanilla(_cachedItems, seen, VanillaOreIds, StoreItemCategory.Ore);
            AddKnownVanilla(_cachedItems, seen, VanillaAmmoIds, StoreItemCategory.Ammo);

            try
            {
                if (MyDefinitionManager.Static != null)
                {
                    foreach (var def in MyDefinitionManager.Static.GetAllDefinitions())
                    {
                        var physical = def as MyPhysicalItemDefinition;
                        if (physical == null)
                            continue;

                        string id = BuildId(physical);
                        var category = GetCategory(id);
                        if (category == StoreItemCategory.Unknown)
                            continue;

                        if (seen.Add(id))
                        {
                            _cachedItems.Add(new StoreCatalogItem
                            {
                                Id = id,
                                ShortName = GetShortName(id),
                                Category = category,
                                IsVanilla = false
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("StoreItemCatalog dynamic scan failed: " + e);
            }

            try
            {
                foreach (ShipStoreOfferDefinition offer in ShipStoreOfferCatalog.GetOffers())
                {
                    if (offer == null || string.IsNullOrWhiteSpace(offer.Id) || !seen.Add(offer.Id))
                        continue;

                    _cachedItems.Add(new StoreCatalogItem
                    {
                        Id = offer.Id,
                        ShortName = string.IsNullOrWhiteSpace(offer.DisplayName) ? offer.PrefabSubtypeId : offer.DisplayName,
                        Category = StoreItemCategory.Ship,
                        IsVanilla = offer.IsVanilla
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error("StoreItemCatalog ship scan failed: " + e);
            }

            _isBuilt = true;
        }

        private static string BuildId(MyPhysicalItemDefinition def)
        {
            return def.Id.TypeId.ToString() + "/" + def.Id.SubtypeName;
        }

        private static void AddKnownVanilla(List<StoreCatalogItem> target, HashSet<string> seen, string[] ids, StoreItemCategory category)
        {
            if (ids == null)
                return;

            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i];
                if (string.IsNullOrWhiteSpace(id) || !seen.Add(id))
                    continue;

                target.Add(new StoreCatalogItem
                {
                    Id = id,
                    ShortName = GetShortName(id),
                    Category = category,
                    IsVanilla = true
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

        private static bool Contains(string[] ids, string id)
        {
            if (ids == null || string.IsNullOrWhiteSpace(id))
                return false;

            for (int i = 0; i < ids.Length; i++)
            {
                if (string.Equals(ids[i], id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
