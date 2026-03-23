using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Sandbox.ModAPI;
using VRage.Game;
using VRageMath;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.ShipOffers.Models;

namespace ZeroStoreSystem.ShipOffers
{
    public static class ShipStoreOfferCatalog
    {
        public const string RelativeFilePath = "Data/StoreData/ShipStoreOffers.xml";

        private static readonly List<ShipStoreOfferDefinition> _offers = new List<ShipStoreOfferDefinition>();
        private static bool _loaded;

        public sealed class Accessor
        {
            public IEnumerable<ShipStoreOfferDefinition> GetOffers()
            {
                return ShipStoreOfferCatalog.GetOffers();
            }

            public bool TryGetById(string id, out ShipStoreOfferDefinition offer)
            {
                return ShipStoreOfferCatalog.TryGetById(id, out offer);
            }

            public bool TryGetByItemId(MyDefinitionId itemId, out ShipStoreOfferDefinition offer)
            {
                return ShipStoreOfferCatalog.TryGetByItemId(itemId, out offer);
            }

            public void Invalidate()
            {
                ShipStoreOfferCatalog.Invalidate();
            }
        }

        public static readonly Accessor Instance = new Accessor();

        public static void Invalidate()
        {
            _offers.Clear();
            _loaded = false;
        }

        public static IEnumerable<ShipStoreOfferDefinition> GetOffers()
        {
            EnsureLoaded();
            return _offers;
        }

        public static bool TryGetById(string id, out ShipStoreOfferDefinition offer)
        {
            EnsureLoaded();

            for (int i = 0; i < _offers.Count; i++)
            {
                ShipStoreOfferDefinition current = _offers[i];
                if (current != null && string.Equals(current.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    offer = current;
                    return true;
                }
            }

            offer = null;
            return false;
        }

        public static bool TryGetByItemId(MyDefinitionId itemId, out ShipStoreOfferDefinition offer)
        {
            EnsureLoaded();

            string subtype = itemId.SubtypeName ?? string.Empty;

            for (int i = 0; i < _offers.Count; i++)
            {
                ShipStoreOfferDefinition current = _offers[i];
                if (current == null)
                    continue;

                if (string.Equals(current.Id, subtype, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(current.PrefabSubtypeId, subtype, StringComparison.OrdinalIgnoreCase))
                {
                    offer = current;
                    return true;
                }
            }

            offer = null;
            return false;
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
                return;

            _loaded = true;
            _offers.Clear();

            if (MyAPIGateway.Session == null || MyAPIGateway.Utilities == null)
                return;

            List<MyObjectBuilder_Checkpoint.ModItem> mods = MyAPIGateway.Session.Mods;
            if (mods == null)
                return;

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < mods.Count; i++)
            {
                MyObjectBuilder_Checkpoint.ModItem mod = mods[i];
                try
                {
                    if (!MyAPIGateway.Utilities.FileExistsInModLocation(RelativeFilePath, mod))
                        continue;

                    using (TextReader reader = MyAPIGateway.Utilities.ReadFileInModLocation(RelativeFilePath, mod))
                    {
                        string xml = reader.ReadToEnd();
                        ParseOffers(xml, mod, seen, _offers);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Failed to load ship offers from mod '" + mod.Name + "': " + e.Message);
                }
            }

            _offers.Sort(CompareOffers);
            Log.Info("ShipStoreOfferCatalog loaded offers=" + _offers.Count);
        }

        private static void ParseOffers(string xml, MyObjectBuilder_Checkpoint.ModItem mod, HashSet<string> seen, List<ShipStoreOfferDefinition> target)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return;

            int position = 0;
            while (true)
            {
                int offerStart = xml.IndexOf("<Offer>", position, StringComparison.OrdinalIgnoreCase);
                if (offerStart < 0)
                    break;

                int contentStart = offerStart + 7;
                int offerEnd = xml.IndexOf("</Offer>", contentStart, StringComparison.OrdinalIgnoreCase);
                if (offerEnd < 0)
                    break;

                string offerXml = xml.Substring(contentStart, offerEnd - contentStart);
                ShipStoreOfferDefinition offer = ParseOfferBlock(offerXml, mod);
                if (offer != null && !string.IsNullOrWhiteSpace(offer.Id) && seen.Add(offer.Id))
                    target.Add(offer);

                position = offerEnd + 8;
            }
        }

        private static ShipStoreOfferDefinition ParseOfferBlock(string offerXml, MyObjectBuilder_Checkpoint.ModItem mod)
        {
            if (string.IsNullOrWhiteSpace(offerXml))
                return null;

            ShipStoreOfferDefinition offer = new ShipStoreOfferDefinition();
            offer.Id = ReadTag(offerXml, "Id");
            offer.DisplayName = ReadTag(offerXml, "DisplayName");
            offer.Description = ReadTag(offerXml, "Description");
            offer.PrefabSubtypeId = ReadTag(offerXml, "PrefabSubtypeId");
            offer.Icon = ReadTag(offerXml, "Icon");
            offer.Price = ReadInt(ReadTag(offerXml, "Price"), 0);
            offer.Stock = ReadInt(ReadTag(offerXml, "Stock"), -1);
            offer.SpawnMode = ReadSpawnMode(ReadTag(offerXml, "SpawnMode"), ShipSpawnMode.Auto);
            offer.ConnectorName = ReadTag(offerXml, "ConnectorName");
            offer.ConnectorTag = ReadTag(offerXml, "ConnectorTag");
            offer.SpawnOffset = ReadVector3D(ReadTag(offerXml, "SpawnOffset"), Vector3D.Zero);
            offer.SpawnCheckHalfExtents = ReadVector3D(ReadTag(offerXml, "SpawnCheckHalfExtents"), new Vector3D(10d, 10d, 10d));
            offer.PlanetAllowed = ReadBool(ReadTag(offerXml, "PlanetAllowed"), true);
            offer.SpaceAllowed = ReadBool(ReadTag(offerXml, "SpaceAllowed"), true);
            offer.FactionTag = ReadTag(offerXml, "FactionTag");
            offer.SourceModName = mod.Name;
            offer.IsVanilla = string.Equals(mod.Name, "DEV ZERO's Store System", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(offer.DisplayName))
                offer.DisplayName = offer.PrefabSubtypeId;

            if (string.IsNullOrWhiteSpace(offer.Id) || string.IsNullOrWhiteSpace(offer.PrefabSubtypeId))
                return null;

            return offer;
        }

        private static int CompareOffers(ShipStoreOfferDefinition a, ShipStoreOfferDefinition b)
        {
            int vanillaA = (a != null && a.IsVanilla) ? 0 : 1;
            int vanillaB = (b != null && b.IsVanilla) ? 0 : 1;
            int cmp = vanillaA.CompareTo(vanillaB);
            if (cmp != 0)
                return cmp;

            string nameA = a != null ? (a.DisplayName ?? a.Id ?? string.Empty) : string.Empty;
            string nameB = b != null ? (b.DisplayName ?? b.Id ?? string.Empty) : string.Empty;
            cmp = string.Compare(nameA, nameB, StringComparison.OrdinalIgnoreCase);
            if (cmp != 0)
                return cmp;

            string idA = a != null ? (a.Id ?? string.Empty) : string.Empty;
            string idB = b != null ? (b.Id ?? string.Empty) : string.Empty;
            return string.Compare(idA, idB, StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadTag(string xml, string tagName)
        {
            if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(tagName))
                return string.Empty;

            string openTag = "<" + tagName + ">";
            string closeTag = "</" + tagName + ">";

            int start = xml.IndexOf(openTag, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                return string.Empty;

            start += openTag.Length;
            int end = xml.IndexOf(closeTag, start, StringComparison.OrdinalIgnoreCase);
            if (end < 0 || end < start)
                return string.Empty;

            return xml.Substring(start, end - start).Trim();
        }

        private static bool ReadBool(string text, bool fallback)
        {
            bool value;
            return bool.TryParse(text, out value) ? value : fallback;
        }

        private static int ReadInt(string text, int fallback)
        {
            int value;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : fallback;
        }

        private static ShipSpawnMode ReadSpawnMode(string text, ShipSpawnMode fallback)
        {
            if (string.IsNullOrWhiteSpace(text))
                return fallback;

            ShipSpawnMode mode;
            return Enum.TryParse<ShipSpawnMode>(text, true, out mode) ? mode : fallback;
        }

        private static Vector3D ReadVector3D(string xml, Vector3D fallback)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return fallback;

            double x = fallback.X;
            double y = fallback.Y;
            double z = fallback.Z;

            string xs = ReadTag(xml, "X");
            string ys = ReadTag(xml, "Y");
            string zs = ReadTag(xml, "Z");

            if (!string.IsNullOrWhiteSpace(xs))
                double.TryParse(xs, NumberStyles.Float, CultureInfo.InvariantCulture, out x);
            if (!string.IsNullOrWhiteSpace(ys))
                double.TryParse(ys, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
            if (!string.IsNullOrWhiteSpace(zs))
                double.TryParse(zs, NumberStyles.Float, CultureInfo.InvariantCulture, out z);

            return new Vector3D(x, y, z);
        }
    }
}
