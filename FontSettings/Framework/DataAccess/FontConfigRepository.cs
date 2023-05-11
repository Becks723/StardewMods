using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;
using FontSettings.Framework.DataAccess.Parsing;
using FontSettings.Framework.Models;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    internal partial class FontConfigRepository : IFontConfigRepository
    {
        private readonly IMonitor _monitor;
        private readonly FontConfigParser _parser;

        public FontConfigRepository(IModHelper helper, IMonitor monitor, FontConfigParser parser)
            : this(helper)
        {
            this._monitor = monitor;
            this._parser = parser;
        }

        public FontConfig? ReadConfig(FontConfigKey key)
        {
            var rawConfigs = this.ReadAllConfigs();
            var parsedConfigs = this._parser.ParseCollection(rawConfigs, key.Language, key.FontType);
            var parsedConfig = parsedConfigs.ContainsKey(key) 
                ? parsedConfigs[key] 
                : null;

            this._monitor.Log($"Loaded font config for {key}: {parsedConfig}");
            return parsedConfig;
        }

        public void WriteConfig(FontConfigKey key, FontConfig? config)
        {
            this._monitor.Log($"Saving font config for {key}: {config}");

            var allConfigs = this.ReadAllConfigs();
            allConfigs.RemoveAll(config => config.Lang == key.Language.Code
                                        && config.Locale == key.Language.Locale
                                        && config.InGameType == key.FontType);
            if (config != null)
            {
                var rawConfig = this._parser.ParseBack(new(key, config));
                allConfigs.Add(rawConfig);
            }

            var orderedData = new FontConfigs();
            orderedData.AddRange(allConfigs
                .OrderBy(data => data.Lang)
                .ThenBy(data => data.InGameType));

            this.WriteAllConfigs(orderedData);
        }
    }
}
