using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace FontSettings.Framework.DataAccess
{
    partial class FontPresetRepository
    {
        private readonly string _rootDir;

        public FontPresetRepository(string presetsDir)
        {
            this._rootDir = presetsDir;

            Directory.CreateDirectory(presetsDir);
        }

        public IDictionary<string, FontPresetData> ReadAllPresets()
        {
            var result = new Dictionary<string, FontPresetData>();

            string[] potentialPresetFiles = Directory.GetFiles(this._rootDir, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string file in potentialPresetFiles)
            {
                if (TryLoadPreset(file, out var pair))
                    result.Add(pair.Key, pair.Value);
            }

            return result;
        }

        public void WritePreset(string name, FontPresetData? preset)
        {
            // when null, delete the preset if exists.
            if (preset == null)
            {
                string destPath = Path.Combine(this._rootDir, $"{name}.json");
                File.Delete(destPath);
            }

            else
            {
                string json = JsonConvert.SerializeObject(preset, GetJsonSerializeSettings());

                string destPath = Path.Combine(this._rootDir, $"{name}.json");
                File.WriteAllText(destPath, json);
            }
        }

        private static bool TryLoadPreset(string fullPath, out KeyValuePair<string, FontPresetData> preset)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(fullPath);

                string json = File.ReadAllText(fullPath);
                FontPresetData value = JsonConvert.DeserializeObject<FontPresetData>(json, GetJsonDeserializeSettings());
                value.Name = name;

                preset = KeyValuePair.Create(name, value);
                return true;
            }
            catch (Exception ex)
            {
                if (ex is JsonSerializationException)
                    ILog.Trace($"Not a preset file: {fullPath}");
                ILog.Error($"Failed to load preset: {fullPath}. {ex.Message}\n{ex.StackTrace}");

                preset = default;
                return false;
            }
        }

        private static JsonSerializerSettings GetJsonSerializeSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            };
        }

        private static JsonSerializerSettings GetJsonDeserializeSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                MissingMemberHandling = MissingMemberHandling.Error,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            };
        }
    }
}
