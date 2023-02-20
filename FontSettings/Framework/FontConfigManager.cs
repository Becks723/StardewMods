using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using StardewValleyUI;

namespace FontSettings.Framework
{
    internal class FontConfigManager : IFontConfigManager
    {
        private readonly object _lock = new object();

        private readonly IDictionary<FontConfigKey, FontConfig> _fontConfigs = new Dictionary<FontConfigKey, FontConfig>();

        public event EventHandler ConfigUpdated;

        public FontConfigManager()
        {
        }

        public FontConfigManager(IDictionary<FontConfigKey, FontConfig> fontConfigs)
        {
            foreach (var pair in fontConfigs)
                this._fontConfigs.Add(pair);
        }

        /// <summary>Won't raise <see cref="ConfigUpdated"/>.</summary>
        public void AddFontConfig(KeyValuePair<FontConfigKey, FontConfig> config)
        {
            this.UpdateFontConfig(config.Key.Language, config.Key.FontType, config.Value, raiseConfigUpdated: false);
        }

        public void UpdateFontConfig(LanguageInfo language, GameFontType fontType, FontConfig? config)
        {
            this.UpdateFontConfig(language, fontType, config, raiseConfigUpdated: true);
        }

        private void UpdateFontConfig(LanguageInfo language, GameFontType fontType, FontConfig? config, bool raiseConfigUpdated)
        {
            var key = new FontConfigKey(language, fontType);

            lock (this._lock)
            {
                if (!this.TryGetFontConfig(language, fontType, out _))
                {
                    if (config != null)
                    {
                        this._fontConfigs.Add(key, config);
                        if (raiseConfigUpdated)
                            this.RaiseConfigUpdated(EventArgs.Empty);
                    }
                }
                else
                {
                    if (config != null)
                        this._fontConfigs[key] = config;
                    else
                        this._fontConfigs.Remove(key);

                    if (raiseConfigUpdated)
                        this.RaiseConfigUpdated(EventArgs.Empty);
                }
            }
        }
        public bool TryGetFontConfig(LanguageInfo language, GameFontType fontType, out FontConfig? fontConfig)
        {
            FontConfig? got = this.GetFontConfig(language, fontType);

            if (got != null)
            {
                fontConfig = got;
                return true;
            }
            else
            {
                fontConfig = null;
                return false;
            }
        }

        public IDictionary<FontConfigKey, FontConfig> GetAllFontConfigs()
        {
            lock (this._lock)
            {
                return this._fontConfigs;
            }
        }

        public FontConfig? GetFontConfig(LanguageInfo language, GameFontType fontType)
        {
            lock (this._lock)
            {
                var key = new FontConfigKey(language, fontType);
                if (this._fontConfigs.TryGetValue(key, out FontConfig value))
                    return value;

                return null;
            }
        }

        protected virtual void RaiseConfigUpdated(EventArgs e)
        {
            ConfigUpdated?.Invoke(this, e);
        }
    }
}
