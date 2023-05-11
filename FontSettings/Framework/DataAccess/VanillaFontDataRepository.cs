using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Parsing;
using FontSettings.Framework.Models;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    internal partial class VanillaFontDataRepository
    {
        private readonly IMonitor _monitor;
        private readonly FontConfigParser _parser;

        public VanillaFontDataRepository(IModHelper helper, IMonitor monitor, FontConfigParser parser)
            : this(helper, monitor)
        {
            this._monitor = monitor;
            this._parser = parser;
        }

        public FontConfig? ReadVanillaFontConfig(FontConfigKey key)
        {
            var rawConfigs = this.ReadVanillaFontData().Fonts;

            var parsedConfigs = this._parser.ParseCollection(rawConfigs, key.Language, key.FontType);
            var parsedConfig = parsedConfigs.ContainsKey(key)
                ? parsedConfigs[key]
                : null;

            this._monitor.Log($"Loaded vanilla font config for {key}: {parsedConfig}");
            return parsedConfig;
        }
    }
}
