using System;
using Sandbox.Definitions;
using VRage.Game;

namespace ZeroStoreSystem.Pricing
{
    public static class BasePriceCalculator
    {
        public static int GetBasePrice(MyDefinitionId itemId)
        {
            try
            {
                MyPhysicalItemDefinition def;
                if (MyDefinitionManager.Static.TryGetDefinition(itemId, out def) && def != null)
                {
                    if (def.MinimalPricePerUnit > 0)
                        return Math.Max(1, (int)Math.Ceiling((double)def.MinimalPricePerUnit));

                    if (def.Mass > 0f)
                        return Math.Max(1, (int)Math.Ceiling(def.Mass * 100f));
                }
            }
            catch
            {
            }

            return 100;
        }

        public static int ApplyPriceModifier(int basePrice, float mod)
        {
            if (basePrice < 1)
                basePrice = 1;

            if (mod <= 0f)
                mod = 1f;

            return Math.Max(1, (int)Math.Ceiling((double)(basePrice * mod)));
        }
    }
}
