using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal class FontConfigManager : IFontConfigManager
    {
        private readonly object _lock = new object();

        private readonly IDictionary<FontConfigKey, FontConfig_> _fontConfigs;

        public event EventHandler ConfigUpdated;

        public FontConfigManager(IDictionary<FontConfigKey, FontConfig_> fontConfigs)
        {
            this._fontConfigs = fontConfigs;
        }

        public void UpdateFontConfig(LanguageInfo language, GameFontType fontType, FontConfig_? config)
        {
            var key = new FontConfigKey(language, fontType);

            lock (this._lock)
            {
                if (!this.TryGetFontConfig(language, fontType, out _))
                {
                    if (config != null)
                    {
                        this._fontConfigs.Add(key, config);
                        this.RaiseConfigUpdated(EventArgs.Empty);
                    }
                }
                else
                {
                    if (config != null)
                        this._fontConfigs[key] = config;
                    else
                        this._fontConfigs.Remove(key);

                    this.RaiseConfigUpdated(EventArgs.Empty);
                }
            }
        }

        public bool TryGetFontConfig(LanguageInfo language, GameFontType fontType, out FontConfig_? fontConfig)
        {
            FontConfig_? got = this.GetFontConfig(language, fontType);

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

        public IDictionary<FontConfigKey, FontConfig_> GetAllFontConfigs()
        {
            lock (this._lock)
            {
                return this._fontConfigs;
            }
        }

        public FontConfig_? GetFontConfig(LanguageInfo language, GameFontType fontType)
        {
            lock (this._lock)
            {
                var key = new FontConfigKey(language, fontType);
                if (this._fontConfigs.TryGetValue(key, out FontConfig_ value))
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
