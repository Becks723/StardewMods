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
        private readonly FontConfigParser _parser = new();

        public FontConfigModel? ReadVanillaFontConfig(FontContext context)
        {
            var rawConfigs = this.ReadVanillaFontData().Fonts;

            var parsedConfigs = this._parser.ParseCollection(rawConfigs, context.Language, context.FontType);
            var parsedConfig = parsedConfigs.ContainsKey(context)
                ? parsedConfigs[context]
                : null;

            this._monitor.Log($"Loaded vanilla font config for {context}: {parsedConfig}");
            return parsedConfig;
        }
    }
}
