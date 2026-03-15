using System.Collections.Generic;

namespace ZeroStoreSystem.Core
{
    public static class VanillaComponentCatalog
    {
        public static readonly string[] ComponentIds = new[]
        {
            "MyObjectBuilder_Component/BulletproofGlass",
            "MyObjectBuilder_Component/Canvas",
            "MyObjectBuilder_Component/Computer",
            "MyObjectBuilder_Component/Construction",
            "MyObjectBuilder_Component/Detector",
            "MyObjectBuilder_Component/Display",
            "MyObjectBuilder_Component/Engine",
            "MyObjectBuilder_Component/Explosives",
            "MyObjectBuilder_Component/Girder",
            "MyObjectBuilder_Component/GravityGenerator",
            "MyObjectBuilder_Component/InteriorPlate",
            "MyObjectBuilder_Component/LargeTube",
            "MyObjectBuilder_Component/Medical",
            "MyObjectBuilder_Component/MetalGrid",
            "MyObjectBuilder_Component/Motor",
            "MyObjectBuilder_Component/PowerCell",
            "MyObjectBuilder_Component/RadioCommunication",
            "MyObjectBuilder_Component/Reactor",
            "MyObjectBuilder_Component/SmallTube",
            "MyObjectBuilder_Component/SolarCell",
            "MyObjectBuilder_Component/SteelPlate",
            "MyObjectBuilder_Component/Superconductor",
            "MyObjectBuilder_Component/Thrust",
            "MyObjectBuilder_Component/ZoneChip"
        };

        public static void EnsurePresent(ICollection<Config.Models.StoreItemRule> itemRules)
        {
            if (itemRules == null)
                return;

            var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var rule in itemRules)
            {
                if (rule != null && !string.IsNullOrWhiteSpace(rule.Id))
                    seen.Add(rule.Id);
            }

            foreach (var id in ComponentIds)
            {
                if (seen.Contains(id))
                    continue;

                itemRules.Add(new Config.Models.StoreItemRule
                {
                    Id = id,
                    Allowed = true,
                    ForceInclude = false,
                    Offer = new Config.Models.StoreOfferRule
                    {
                        Enabled = false,
                        PriceMod = 1f,
                        Amount = 0
                    },
                    Order = new Config.Models.StoreOrderRule
                    {
                        Enabled = false,
                        PriceMod = 1f,
                        Amount = 0
                    }
                });
            }
        }
    }
}
