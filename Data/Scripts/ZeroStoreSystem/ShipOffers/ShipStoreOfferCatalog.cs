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
                if (string.Equals(_offers[i].Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    offer = _offers[i];
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

            var mods = MyAPIGateway.Session.Mods;
            if (mods == null)
                return;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < mods.Count; i++)
            {
                var mod = mods[i];
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

            if (xml.IndexOf("<ShipStoreOffers", StringComparison.OrdinalIgnoreCase) < 0)
                return;

            int searchIndex = 0;
            while (true)
            {
                int offerStart = IndexOfTag(xml, "Offer", searchIndex);
                if (offerStart < 0)
                    break;

                int contentStart = IndexOfTagEnd(xml, offerStart);
                if (contentStart < 0)
                    break;

                int offerEnd = IndexOfClosingTag(xml, "Offer", contentStart);
                if (offerEnd < 0)
                    break;

                string offerXml = xml.Substring(contentStart, offerEnd - contentStart);
                searchIndex = offerEnd + "</Offer>".Length;

                var offer = ParseSingleOffer(offerXml, mod);
                if (offer == null)
                    continue;

                if (string.IsNullOrWhiteSpace(offer.Id) || string.IsNullOrWhiteSpace(offer.PrefabSubtypeId))
                    continue;

                if (!seen.Add(offer.Id))
                    continue;

                if (string.IsNullOrWhiteSpace(offer.DisplayName))
                    offer.DisplayName = offer.PrefabSubtypeId;

                target.Add(offer);
            }
        }

        private static ShipStoreOfferDefinition ParseSingleOffer(string offerXml, MyObjectBuilder_Checkpoint.ModItem mod)
        {
            var offer = new ShipStoreOfferDefinition
            {
                Id = ReadTagValue(offerXml, "Id"),
                DisplayName = ReadTagValue(offerXml, "DisplayName"),
                Description = ReadTagValue(offerXml, "Description"),
                PrefabSubtypeId = ReadTagValue(offerXml, "PrefabSubtypeId"),
                Icon = ReadTagValue(offerXml, "Icon"),
                Price = ReadLongTag(offerXml, "Price", 0L),
                Stock = ReadIntTag(offerXml, "Stock", -1),
                SpawnMode = ReadSpawnModeTag(offerXml, "SpawnMode", ShipSpawnMode.Auto),
                ConnectorName = ReadTagValue(offerXml, "ConnectorName"),
                ConnectorTag = ReadTagValue(offerXml, "ConnectorTag"),
                SpawnOffset = ReadVectorBlock(offerXml, "SpawnOffset", Vector3D.Zero),
                SpawnCheckHalfExtents = ReadVectorBlock(offerXml, "SpawnCheckHalfExtents", new Vector3D(10d, 10d, 10d)),
                PlanetAllowed = ReadBoolTag(offerXml, "PlanetAllowed", true),
                SpaceAllowed = ReadBoolTag(offerXml, "SpaceAllowed", true),
                FactionTag = ReadTagValue(offerXml, "FactionTag"),
                SourceModName = mod.Name,
                IsVanilla = string.Equals(mod.Name, "DEV ZERO's Store System", StringComparison.OrdinalIgnoreCase)
            };

            return offer;
        }

        private static int CompareOffers(ShipStoreOfferDefinition a, ShipStoreOfferDefinition b)
        {
            int vanillaA = a != null && a.IsVanilla ? 0 : 1;
            int vanillaB = b != null && b.IsVanilla ? 0 : 1;
            int cmp = vanillaA.CompareTo(vanillaB);
            if (cmp != 0)
                return cmp;

            string nameA = a != null ? a.DisplayName ?? a.Id : string.Empty;
            string nameB = b != null ? b.DisplayName ?? b.Id : string.Empty;
            cmp = string.Compare(nameA, nameB, StringComparison.OrdinalIgnoreCase);
            if (cmp != 0)
                return cmp;

            string idA = a != null ? a.Id ?? string.Empty : string.Empty;
            string idB = b != null ? b.Id ?? string.Empty : string.Empty;
            return string.Compare(idA, idB, StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadTagValue(string source, string tagName)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(tagName))
                return string.Empty;

            int start = IndexOfTag(source, tagName, 0);
            if (start < 0)
                return string.Empty;

            int contentStart = IndexOfTagEnd(source, start);
            if (contentStart < 0)
                return string.Empty;

            int end = IndexOfClosingTag(source, tagName, contentStart);
            if (end < 0)
                return string.Empty;

            return source.Substring(contentStart, end - contentStart).Trim();
        }

        private static int ReadIntTag(string source, string tagName, int fallback)
        {
            int value;
            return int.TryParse(ReadTagValue(source, tagName), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                ? value
                : fallback;
        }

        private static long ReadLongTag(string source, string tagName, long fallback)
        {
            long value;
            return long.TryParse(ReadTagValue(source, tagName), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                ? value
                : fallback;
        }

        private static bool ReadBoolTag(string source, string tagName, bool fallback)
        {
            bool value;
            return bool.TryParse(ReadTagValue(source, tagName), out value)
                ? value
                : fallback;
        }

        private static ShipSpawnMode ReadSpawnModeTag(string source, string tagName, ShipSpawnMode fallback)
        {
            string text = ReadTagValue(source, tagName);
            ShipSpawnMode mode;
            return Enum.TryParse(text, true, out mode) ? mode : fallback;
        }

        private static Vector3D ReadVectorBlock(string source, string tagName, Vector3D fallback)
        {
            string block = ReadTagValue(source, tagName);
            if (string.IsNullOrWhiteSpace(block))
                return fallback;

            double x = ReadDoubleTag(block, "X", fallback.X);
            double y = ReadDoubleTag(block, "Y", fallback.Y);
            double z = ReadDoubleTag(block, "Z", fallback.Z);
            return new Vector3D(x, y, z);
        }

        private static double ReadDoubleTag(string source, string tagName, double fallback)
        {
            double value;
            return double.TryParse(ReadTagValue(source, tagName), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value)
                ? value
                : fallback;
        }

        private static int IndexOfTag(string source, string tagName, int startIndex)
        {
            return source.IndexOf("<" + tagName, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        private static int IndexOfTagEnd(string source, int tagStartIndex)
        {
            if (tagStartIndex < 0)
                return -1;

            int end = source.IndexOf('>', tagStartIndex);
            if (end < 0)
                return -1;

            return end + 1;
        }

        private static int IndexOfClosingTag(string source, string tagName, int startIndex)
        {
            return source.IndexOf("</" + tagName + ">", startIndex, StringComparison.OrdinalIgnoreCase);
        }
    }
}
