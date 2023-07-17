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
    internal partial class FontConfigRepository
    {
        private readonly IMonitor _monitor;
        private readonly FontConfigParser _parser;

        public FontConfigRepository(IModHelper helper, IMonitor monitor)
            : this(helper)
        {
            this._monitor = monitor;
            this._parser = new FontConfigParser();
        }

        public FontConfigModel? ReadConfig(FontContext context)
        {
            var rawConfigs = this.ReadAllConfigs();
            var parsedConfigs = this._parser.ParseCollection(rawConfigs, context.Language, context.FontType);
            var parsedConfig = parsedConfigs.ContainsKey(context)
                ? parsedConfigs[context]
                : null;

            this._monitor.Log($"Loaded font config for {context}: {parsedConfig}");
            return parsedConfig;
        }

        public void WriteConfig(FontContext context, FontConfigModel? config)
        {
            this._monitor.Log($"Saving font config for {context}: {config}");

            var allConfigs = this.ReadAllConfigs();
            allConfigs.RemoveAll(config => config?.Lang == context.Language.Code
                                        && config?.Locale == context.Language.Locale
                                        && config?.InGameType == context.FontType);
            if (config != null)
            {
                var rawConfig = this._parser.ParseBack(new(context, config));
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
