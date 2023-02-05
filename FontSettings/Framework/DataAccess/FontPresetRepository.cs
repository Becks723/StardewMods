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
    internal class FontPresetRepository
    {
        private readonly string _rootDir;

        public FontPresetRepository(string presetsDir)
        {
            this._rootDir = presetsDir;
        }

        public IDictionary<string, FontPreset> ReadAllPresets()
        {
            var result = new Dictionary<string, FontPreset>();

            string[] potentialPresetFiles = Directory.GetFiles(this._rootDir, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string file in potentialPresetFiles)
            {
                if (TryLoadPreset(file, out var pair))
                    result.Add(pair.Key, pair.Value);
            }

            return result;
        }

        public void WritePreset(string name, FontPreset? preset)
        {
            // when null, delete the preset if exists.
            if (preset == null)
            {
                DeleteFile(this._rootDir, name);
            }

            else
            {
                string json = JsonConvert.SerializeObject(preset, GetJsonSerializeSettings());

                string destPath = Path.Combine(this._rootDir, $"{name}.json");
                File.WriteAllText(destPath, json);
            }
        }

        private static bool TryLoadPreset(string fullPath, out KeyValuePair<string, FontPreset> preset)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(fullPath);

                string json = File.ReadAllText(fullPath);
                FontPreset value = JsonConvert.DeserializeObject<FontPreset>(json, GetJsonDeserializeSettings());
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

        private static void DeleteFile(string directory, string fileName)
        {
            string fullPath = Path.Combine(directory, fileName);
            File.Delete(fullPath);
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
