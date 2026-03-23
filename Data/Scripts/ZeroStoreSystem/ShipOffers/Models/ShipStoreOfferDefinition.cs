using VRage.Game;
using VRageMath;

namespace ZeroStoreSystem.ShipOffers.Models
{
    public class ShipStoreOfferDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public string PrefabSubtypeId = string.Empty;
        public string TokenItemId = string.Empty;
        public string Icon = string.Empty;
        public int Price = 0;
        public int Stock = 1;
        public bool IsVanilla = false;
        public ShipSpawnMode SpawnMode = ShipSpawnMode.VanillaLike;
        public string FactionTag = string.Empty;
        public Vector3D SpawnOffset = new Vector3D(0d, 0d, 0d);
        public Vector3D SpawnCheckHalfExtents = new Vector3D(40d, 20d, 40d);

        public MyDefinitionId GetTokenDefinitionId()
        {
            if (string.IsNullOrWhiteSpace(TokenItemId))
                return MyDefinitionId.Parse("MyObjectBuilder_Component/" + PrefabSubtypeId);

            return MyDefinitionId.Parse(TokenItemId);
        }
    }
}
