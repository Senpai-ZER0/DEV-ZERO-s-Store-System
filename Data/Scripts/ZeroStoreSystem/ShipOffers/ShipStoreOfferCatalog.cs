using System;
using System.Collections.Generic;
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

            string root = ExtractTag(xml, "ShipStoreOffers");
            if (string.IsNullOrWhiteSpace(root))
                return;

            List<string> offerBlocks = ExtractBlocks(root, "Offer");
            for (int i = 0; i < offerBlocks.Count; i++)
            {
                string block = offerBlocks[i];

                var offer = new ShipStoreOfferDefinition
                {
                    Id = ReadString(block, "Id"),
                    DisplayName = ReadString(block, "DisplayName"),
                    Description = ReadString(block, "Description"),
                    PrefabSubtypeId = ReadString(block, "PrefabSubtypeId"),
                    Icon = ReadString(block, "Icon"),
                    Price = ReadLong(block, "Price", 0),
                    Stock = ReadInt(block, "Stock", -1),
                    SpawnMode = ReadSpawnMode(block, "SpawnMode", ShipSpawnMode.Auto),
                    ConnectorName = ReadString(block, "ConnectorName"),
                    ConnectorTag = ReadString(block, "ConnectorTag"),
                    SpawnOffset = ReadVector(block, "SpawnOffset", Vector3D.Zero),
                    SpawnCheckHalfExtents = ReadVector(block, "SpawnCheckHalfExtents", new Vector3D(10, 10, 10)),
                    PlanetAllowed = ReadBool(block, "PlanetAllowed", true),
                    SpaceAllowed = ReadBool(block, "SpaceAllowed", true),
                    FactionTag = ReadString(block, "FactionTag"),
                    SourceModName = mod.Name,
                    IsVanilla = string.Equals(mod.Name, "DEV ZERO's Store System", StringComparison.OrdinalIgnoreCase)
                };

                if (string.IsNullOrWhiteSpace(offer.Id) || string.IsNullOrWhiteSpace(offer.PrefabSubtypeId))
                    continue;

                if (!seen.Add(offer.Id))
                    continue;

                if (string.IsNullOrWhiteSpace(offer.DisplayName))
                    offer.DisplayName = offer.PrefabSubtypeId;

                target.Add(offer);
            }
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

        private static string ReadString(string block, string tag)
        {
            string value = ExtractTag(block, tag);
            return string.IsNullOrWhiteSpace(value) ? string.Empty : DecodeXml(value.Trim());
        }

        private static int ReadInt(string block, string tag, int fallback)
        {
            int value;
            return int.TryParse(ReadString(block, tag), out value) ? value : fallback;
        }

        private static long ReadLong(string block, string tag, long fallback)
        {
            long value;
            return long.TryParse(ReadString(block, tag), out value) ? value : fallback;
        }

        private static bool ReadBool(string block, string tag, bool fallback)
        {
            bool value;
            return bool.TryParse(ReadString(block, tag), out value) ? value : fallback;
        }

        private static ShipSpawnMode ReadSpawnMode(string block, string tag, ShipSpawnMode fallback)
        {
            string text = ReadString(block, tag);
            ShipSpawnMode mode;
            return Enum.TryParse(text, true, out mode) ? mode : fallback;
        }

        private static Vector3D ReadVector(string block, string tag, Vector3D fallback)
        {
            string vectorBlock = ExtractTag(block, tag);
            if (string.IsNullOrWhiteSpace(vectorBlock))
                return fallback;

            double x, y, z;
            if (!double.TryParse(ReadString(vectorBlock, "X"), out x)) x = fallback.X;
            if (!double.TryParse(ReadString(vectorBlock, "Y"), out y)) y = fallback.Y;
            if (!double.TryParse(ReadString(vectorBlock, "Z"), out z)) z = fallback.Z;
            return new Vector3D(x, y, z);
        }

        private static List<string> ExtractBlocks(string xml, string tag)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(tag))
                return list;

            string openTag = "<" + tag + ">";
            string closeTag = "</" + tag + ">";
            int searchIndex = 0;

            while (searchIndex < xml.Length)
            {
                int start = xml.IndexOf(openTag, searchIndex, StringComparison.OrdinalIgnoreCase);
                if (start < 0)
                    break;

                start += openTag.Length;
                int end = xml.IndexOf(closeTag, start, StringComparison.OrdinalIgnoreCase);
                if (end < 0)
                    break;

                list.Add(xml.Substring(start, end - start));
                searchIndex = end + closeTag.Length;
            }

            return list;
        }

        private static string ExtractTag(string xml, string tag)
        {
            if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(tag))
                return string.Empty;

            string openTag = "<" + tag + ">";
            string closeTag = "</" + tag + ">";

            int start = xml.IndexOf(openTag, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                return string.Empty;

            start += openTag.Length;
            int end = xml.IndexOf(closeTag, start, StringComparison.OrdinalIgnoreCase);
            if (end < 0 || end < start)
                return string.Empty;

            return xml.Substring(start, end - start);
        }

        private static string DecodeXml(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&apos;", "'")
                .Replace("&amp;", "&");
        }
    }
}
