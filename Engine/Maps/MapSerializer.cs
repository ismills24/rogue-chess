using System;
using System.IO;
using Newtonsoft.Json;

namespace RogueChess.Engine.Maps
{
    /// <summary>
    /// Handles JSON serialization/deserialization of MapDefinition using Newtonsoft.Json.
    /// </summary>
    public static class MapSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented, // pretty-print
        };

        /// <summary>
        /// Save a MapDefinition to a JSON file.
        /// </summary>
        public static void Save(MapDefinition map, string filePath)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var json = JsonConvert.SerializeObject(map, Settings);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Load a MapDefinition from a JSON file.
        /// </summary>
        public static MapDefinition Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Map file not found", filePath);

            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<MapDefinition>(json)
                ?? throw new InvalidOperationException("Failed to deserialize map file.");
        }

        /// <summary>
        /// Load a MapDefinition from a raw JSON string.
        /// </summary>
        public static MapDefinition FromJson(string json)
        {
            return JsonConvert.DeserializeObject<MapDefinition>(json)
                ?? throw new InvalidOperationException("Failed to deserialize map JSON.");
        }

        /// <summary>
        /// Convert a MapDefinition to a JSON string.
        /// </summary>
        public static string ToJson(MapDefinition map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            return JsonConvert.SerializeObject(map, Settings);
        }
    }
}
