using System;
using System.Collections.Generic;
using ZeroStoreSystem.Config.Models;

namespace ZeroStoreSystem.Core
{
    public enum StoreItemCategory
    {
        Unknown = 0,
        Component = 1,
        Ingot = 2,
        Ore = 3,
        Ammo = 4
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

        public static readonly string[] KnownShipOfferIds =
        {
            "MyObjectBuilder_Component/SCC Zeus MKI"
        };

        public static IEnumerable<string> EnumerateKnownVanillaIds()
        {
            for (int i = 0; i < VanillaComponentIds.Length; i++)
                yield return VanillaComponentIds[i];

            for (int i = 0; i < VanillaIngotIds.Length; i++)
                yield return VanillaIngotIds[i];

            for (int i = 0; i < VanillaOreIds.Length; i++)
                yield return VanillaOreIds[i];

            for (int i = 0; i < VanillaAmmoIds.Length; i++)
                yield return VanillaAmmoIds[i];

            for (int i = 0; i < KnownShipOfferIds.Length; i++)
                yield return KnownShipOfferIds[i];
        }

        public static StoreItemCategory GetCategory(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return StoreItemCategory.Unknown;

            if (Contains(KnownShipOfferIds, id))
                return StoreItemCategory.Component;

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

        public static bool IsVanilla(string id)
        {
            return Contains(VanillaComponentIds, id)
                || Contains(VanillaIngotIds, id)
                || Contains(VanillaOreIds, id)
                || Contains(VanillaAmmoIds, id);
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
                default:
                    return 4;
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

            for (int i = 0; i < ids.Length; i++)
            {
                if (string.Equals(ids[i], id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
