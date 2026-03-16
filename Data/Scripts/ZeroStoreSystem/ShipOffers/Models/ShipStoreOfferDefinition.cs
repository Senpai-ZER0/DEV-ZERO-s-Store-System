using VRageMath;

namespace ZeroStoreSystem.ShipOffers.Models
{
    public class ShipStoreOfferDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public string PrefabSubtypeId = string.Empty;
        public string Icon = string.Empty;
        public long Price = 0;
        public int Stock = -1;
        public ShipSpawnMode SpawnMode = ShipSpawnMode.Auto;
        public string ConnectorName = string.Empty;
        public string ConnectorTag = string.Empty;
        public Vector3D SpawnOffset = Vector3D.Zero;
        public Vector3D SpawnCheckHalfExtents = new Vector3D(10, 10, 10);
        public bool PlanetAllowed = true;
        public bool SpaceAllowed = true;
        public string FactionTag = string.Empty;
        public string SourceModName = string.Empty;
        public bool IsVanilla = false;
    }
}
