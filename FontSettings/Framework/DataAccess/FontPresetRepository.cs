using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Parsing;
using FontSettings.Framework.Models;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    internal partial class FontPresetRepository
    {
        private readonly IMonitor _monitor;
        private readonly FontPresetParser _parser;

        public FontPresetRepository(string presetsDir, IMonitor monitor)
            : this(presetsDir)
        {
            this._monitor = monitor;
            this._parser = new FontPresetParser();
        }

        public IEnumerable<FontPresetModel> ReadPresets(FontConfigKey key)
        {
            var rawPresets = this.ReadAllPresets();
            var parsedPresets = this._parser.ParseCollection(rawPresets.Values, key.Language, key.FontType);
            this._monitor.Log($"Loaded presets in {key}:"
                + $"\n{string.Join('\n', parsedPresets.Select(preset => preset.Settings?.ToString()))}");
            return parsedPresets;
        }

        public IEnumerable<FontPresetModel> ReadPresets(LanguageInfo language)
        {
            var rawPresets = this.ReadAllPresets();
            var parsedPresets = this._parser.ParseCollection(rawPresets.Values, language);
            this._monitor.Log($"Loaded presets in {language}:"
                + $"\n{string.Join('\n', parsedPresets.Select(preset => preset.Settings?.ToString()))}");
            return parsedPresets;
        }

        public void WritePreset(string name, FontPresetModel? preset)
        {
            this._monitor.Log($"Writing preset. Name: {name} Value: {preset?.Settings?.ToString()}");

            var rawPreset = preset == null
                ? null
                : this._parser.ParseBack(preset);

            this.WritePreset(name, rawPreset);
        }
    }
}
