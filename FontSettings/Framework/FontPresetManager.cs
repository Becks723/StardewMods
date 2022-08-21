using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FontSettings.Framework
{
    internal class FontPresetManager
    {
        private readonly IList<FontPreset> _builtInPresets = new List<FontPreset>();
        private readonly IList<FontPreset> _userDefinedPresets = new List<FontPreset>();
        private readonly string _rootDir;
        private readonly string _builtInDir;
        private readonly FontPresetComparer _comparer = new();

        public FontPresetManager(string presetsDir, string builtInFolderName, IEnumerable<FontPreset> builtInPresets = null)
        {
            this._rootDir = presetsDir;
            this._builtInDir = Path.Combine(this._rootDir, builtInFolderName);

            Directory.CreateDirectory(this._rootDir);
            Directory.CreateDirectory(this._builtInDir);

            if (builtInPresets != null)
                foreach (FontPreset item in builtInPresets)
                    this._builtInPresets.Add(item);

            this.Load();
        }

        public IEnumerable<FontPreset> GetAll(bool skipBuiltIn = false, bool skipUserDefined = false)
        {
            switch (skipBuiltIn, skipUserDefined)
            {
                case (true, false):
                    return this._userDefinedPresets;
                case (false, true):
                    return this._builtInPresets;
                case (false, false):
                    return this._builtInPresets.Concat(this._userDefinedPresets);
                case (true, true):
                    return Enumerable.Empty<FontPreset>();
                default:
                    throw new NotSupportedException();
            }
        }

        public IEnumerable<FontPreset> GetAllUnder(FontPresetFontType fontType, bool includeGeneral = true, bool skipBuiltIn = false, bool skipUserDefined = false)
        {
            var all = this.GetAll(skipBuiltIn, skipUserDefined);
            var result = all.Where(p => p.FontType == fontType);
            if (includeGeneral)
                result = result.Concat(all.Where(p => p.FontType is FontPresetFontType.Any));
            return result;
        }

        public bool IsBuiltInPreset(FontPreset preset)
        {
            foreach (FontPreset item in this._builtInPresets)
            {
                if (this._comparer.Equals(item, preset))
                    return true;
            }

            return false;
        }

        public bool TryFindPreset(string name, out FontPreset preset)
        {
            var allPresets = this.GetAll();
            foreach (FontPreset item in allPresets)
            {
                if (this._comparer.Equals(item.Name, name))
                {
                    preset = item;
                    return true;
                }
            }

            preset = null;
            return false;
        }

        public void AddPreset(FontPreset preset)
        {
            this.AssertNotNull(preset, nameof(preset));
            this.AssertValidName(preset.Name);

            this._userDefinedPresets.Add(preset);
            this.WriteToFile(preset);
        }

        public bool TryAddPreset(FontPreset preset)
        {
            try
            {
                this.AddPreset(preset);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }


        public bool RemovePreset(string name, bool canRemoveBuiltIn = false)
        {
            if (!this.TryFindPreset(name, out FontPreset preset))
                return false;

            if (this._userDefinedPresets.Remove(preset))
            {
                DeleteFile(this._rootDir, $"{name}.json");
                return true;
            }

            if (canRemoveBuiltIn
                && this._builtInPresets.Remove(preset))
            {
                DeleteFile(this._builtInDir, $"{name}.json");
                return true;
            }

            return false;
        }

        public void EditPreset<TField>(string name, string fieldName, TField newValue)
        {
            if (!this.TryFindPreset(name, out FontPreset preset))
                throw new KeyNotFoundException($"未找到名字为{name}的预设。");

            static void EnsureTypeMatch(Type expectedType)
            {
                if (expectedType != typeof(TField))
                    throw new ArgumentException($"预期输入一个{expectedType}类型，但实际上输入了{typeof(TField)}类型。");
            }
            static T ParseValue<T>(TField value) => (T)(object)value;

            switch (fieldName)
            {
                case nameof(FontPreset.FontType):
                    EnsureTypeMatch(typeof(FontPresetFontType));
                    preset.FontType = ParseValue<FontPresetFontType>(newValue);
                    break;
                case nameof(FontPreset.FontIndex):
                    EnsureTypeMatch(typeof(int));
                    preset.FontIndex = ParseValue<int>(newValue);
                    break;
                case nameof(FontPreset.Lang):
                    EnsureTypeMatch(typeof(StardewValley.LocalizedContentManager.LanguageCode));
                    preset.Lang = ParseValue<StardewValley.LocalizedContentManager.LanguageCode>(newValue);
                    break;
                case nameof(FontPreset.Locale):
                    EnsureTypeMatch(typeof(string));
                    preset.Locale = ParseValue<string>(newValue);
                    break;
                case nameof(FontPreset.FontSize):
                    EnsureTypeMatch(typeof(float));
                    preset.FontSize = ParseValue<float>(newValue);
                    break;
                case nameof(FontPreset.Spacing):
                    EnsureTypeMatch(typeof(float));
                    preset.Spacing = ParseValue<float>(newValue);
                    break;
                case nameof(FontPreset.LineSpacing):
                    EnsureTypeMatch(typeof(int));
                    preset.LineSpacing = ParseValue<int>(newValue);
                    break;
                case nameof(FontPreset.CharOffsetX):
                    EnsureTypeMatch(typeof(float));
                    preset.CharOffsetX = ParseValue<float>(newValue);
                    break;
                case nameof(FontPreset.CharOffsetY):
                    EnsureTypeMatch(typeof(float));
                    preset.CharOffsetY = ParseValue<float>(newValue);
                    break;
            }
        }

        public void EditPreset(FontPreset preset,
            string fontFileName, int fontIndex, float fontSize, float spacing, int lineSpacing, float offsetX, float offsetY)
        {
            this.AssertNotNull(preset, nameof(preset));
            this.AssertNoInvalidChar(preset.Name, nameof(preset));

            preset.Requires.FontFileName = fontFileName;
            preset.FontIndex = fontIndex;
            preset.FontSize = fontSize;
            preset.Spacing = spacing;
            preset.LineSpacing = lineSpacing;
            preset.CharOffsetX = offsetX;
            preset.CharOffsetY = offsetY;

            this.WriteToFile(preset);
        }

        public bool IsValidPresetName(string? name, out string invalidMessage)
        {
            string reason = null;
            if (string.IsNullOrWhiteSpace(name))
                reason = "不可为空。";

            if (this.ContainsInvalidChar(name))
                reason = "不可包含非法字符，如：<、>、? 等。";

            if (this.DuplicatePresetName(name))
                reason = "该名称已被占用。";

            invalidMessage = reason;
            return reason == null;
        }

        private void WriteToFile(FontPreset preset)  // 名称必须合法。
        {
            string json = JsonConvert.SerializeObject(preset, GetJsonSerializeSettings());

            string destPath = Path.Combine(
                this.IsBuiltInPreset(preset) ? this._builtInDir : this._rootDir,
                $"{preset.Name}.json");
            File.WriteAllText(destPath, json);
        }

        private void Load()
        {
            this.EnsureBuiltInPresets();

            foreach (FontPreset builtInPreset in this.LoadFromDir(this._builtInDir))
                this._builtInPresets.Add(builtInPreset);
            foreach (FontPreset preset in this.LoadFromDir(this._rootDir))
                this._userDefinedPresets.Add(preset);
        }

        private void EnsureBuiltInPresets()
        {
            string[] potentialPresetFiles = Directory.GetFiles(this._builtInDir, "*.json", SearchOption.TopDirectoryOnly);
            foreach (FontPreset item in this._builtInPresets)
            {

            }
        }

        private FontPreset[] LoadFromDir(string directory)
        {
            var result = new List<FontPreset>();
            string[] potentialPresetFiles = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string file in potentialPresetFiles)
            {
                if (TryLoadPreset(file, out FontPreset preset))
                    result.Add(preset);
            }

            return result.ToArray();
        }

        private static bool TryLoadPreset(string fullPath, out FontPreset preset)
        {
            string str = File.ReadAllText(fullPath);
            try
            {
                preset = JsonConvert.DeserializeObject<FontPreset>(str, GetJsonDeserializeSettings());
                preset.Name = Path.GetFileNameWithoutExtension(fullPath);
                return true;
            }
            catch (JsonSerializationException)
            {
                ILog.Trace($"Not a preset file: {fullPath}");
                preset = null;
                return false;
            }
        }

        private static void DeleteFile(string directory, string fileName)
        {
            string fullPath = Path.Combine(directory, fileName);
            File.Delete(fullPath);
        }

        private void AssertNotNull<T>(T? value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }

        private void AssertNoInvalidChar(string value, string paramName)
        {
            if (this.ContainsInvalidChar(value))
                throw new ArgumentException(paramName);
        }

        private void AssertValidName(string? name)
        {
            if (!this.IsValidPresetName(name, out string message))
                throw new ArgumentException(message, nameof(name));
        }

        private bool ContainsInvalidChar(string name)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in name)
            {
                if (invalidChars.Contains(c))
                    return true;
            }

            return false;
        }

        private bool DuplicatePresetName(string name)
        {
            var all = this.GetAll();
            return all.Any(p => this._comparer.Equals(p.Name, name));
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
