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
        private const string RelativePath = "Data/StoreData/ShipStoreOffers.xml";
        private static bool _loaded;
        private static readonly List<ShipStoreOfferDefinition> _offers = new List<ShipStoreOfferDefinition>();
        private static readonly Dictionary<string, ShipStoreOfferDefinition> _byId = new Dictionary<string, ShipStoreOfferDefinition>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, ShipStoreOfferDefinition> _byTokenId = new Dictionary<string, ShipStoreOfferDefinition>(StringComparer.OrdinalIgnoreCase);

        public static void Invalidate()
        {
            _loaded = false;
            _offers.Clear();
            _byId.Clear();
            _byTokenId.Clear();
        }

        public static List<ShipStoreOfferDefinition> GetOffers()
        {
            EnsureLoaded();
            return _offers;
        }

        public static bool TryGetById(string id, out ShipStoreOfferDefinition offer)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(id))
            {
                offer = null;
                return false;
            }
            return _byId.TryGetValue(id, out offer);
        }

        public static bool TryGetByItemId(MyDefinitionId itemId, out ShipStoreOfferDefinition offer)
        {
            EnsureLoaded();
            return _byTokenId.TryGetValue(itemId.ToString(), out offer);
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
                return;

            Invalidate();
            _loaded = true;

            try
            {
                var session = MyAPIGateway.Session;
                if (session == null || session.Mods == null)
                    return;

                foreach (var mod in session.Mods)
                {
                    try
                    {
                        if (!MyAPIGateway.Utilities.FileExistsInModLocation(RelativePath, mod))
                            continue;

                        using (var reader = MyAPIGateway.Utilities.ReadFileInModLocation(RelativePath, mod))
                        {
                            Parse(reader.ReadToEnd());
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("Failed to read ShipStoreOffers from mod '" + mod.Name + "': " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("ShipStoreOfferCatalog load failed: " + e);
            }
        }

        private static void Parse(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return;

            int cursor = 0;
            while (true)
            {
                int open = xml.IndexOf("<Offer>", cursor, StringComparison.OrdinalIgnoreCase);
                if (open < 0)
                    break;

                int contentStart = open + "<Offer>".Length;
                int close = xml.IndexOf("</Offer>", contentStart, StringComparison.OrdinalIgnoreCase);
                if (close < 0)
                    break;

                string block = xml.Substring(contentStart, close - contentStart);
                cursor = close + "</Offer>".Length;

                ShipStoreOfferDefinition offer = ParseOffer(block);
                if (offer == null || string.IsNullOrWhiteSpace(offer.Id) || string.IsNullOrWhiteSpace(offer.PrefabSubtypeId))
                    continue;

                string tokenKey;
                try
                {
                    tokenKey = offer.GetTokenDefinitionId().ToString();
                }
                catch
                {
                    continue;
                }

                _offers.Add(offer);
                _byId[offer.Id] = offer;
                _byTokenId[tokenKey] = offer;
            }
        }

        private static ShipStoreOfferDefinition ParseOffer(string block)
        {
            var offer = new ShipStoreOfferDefinition();
            offer.Id = ReadTag(block, "Id");
            offer.DisplayName = ReadTag(block, "DisplayName");
            offer.Description = ReadTag(block, "Description");
            offer.PrefabSubtypeId = ReadTag(block, "PrefabSubtypeId");
            offer.TokenItemId = ReadTag(block, "TokenItemId");
            offer.Icon = ReadTag(block, "Icon");
            offer.FactionTag = ReadTag(block, "FactionTag");
            offer.Price = ReadInt(block, "Price", 0);
            offer.Stock = ReadInt(block, "Stock", 1);

            string spawnMode = ReadTag(block, "SpawnMode");
            if (!string.IsNullOrWhiteSpace(spawnMode) && string.Equals(spawnMode, "Auto", StringComparison.OrdinalIgnoreCase))
                offer.SpawnMode = ShipSpawnMode.Auto;
            else
                offer.SpawnMode = ShipSpawnMode.VanillaLike;

            offer.SpawnOffset = ReadVector3D(block, "SpawnOffset", new Vector3D(0d, 0d, 0d));
            offer.SpawnCheckHalfExtents = ReadVector3D(block, "SpawnCheckHalfExtents", new Vector3D(40d, 20d, 40d));

            return offer;
        }

        private static string ReadTag(string text, string tag)
        {
            string open = "<" + tag + ">";
            string close = "</" + tag + ">";
            int start = text.IndexOf(open, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                return string.Empty;
            start += open.Length;
            int end = text.IndexOf(close, start, StringComparison.OrdinalIgnoreCase);
            if (end < 0)
                return string.Empty;
            return text.Substring(start, end - start).Trim();
        }

        private static int ReadInt(string text, string tag, int fallback)
        {
            string raw = ReadTag(text, tag);
            int value;
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : fallback;
        }

        private static Vector3D ReadVector3D(string text, string tag, Vector3D fallback)
        {
            string raw = ReadTag(text, tag);
            if (string.IsNullOrWhiteSpace(raw))
                return fallback;

            char[] seps = new []{',',';',' '};
            string[] parts = raw.Split(seps, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return fallback;

            double x, y, z;
            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x))
                return fallback;
            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y))
                return fallback;
            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                return fallback;
            return new Vector3D(x, y, z);
        }
    }
}
