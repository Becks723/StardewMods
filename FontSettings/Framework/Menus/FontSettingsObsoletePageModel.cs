using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValleyUI.Mvvm;

namespace FontSettings.Framework.Menus
{
    internal class FontSettingsObsoletePageModel : MenuModelBase
    {
        private readonly ModConfig _config;
        private readonly Action<ModConfig> _saveConfig;

        private string _hotkeyString;
        public string HotkeyString
        {
            get => this._hotkeyString;
            set => this.SetField(ref this._hotkeyString, value);
        }

        private bool _hideThisTab;
        public bool HideThisTab
        {
            get => this._hideThisTab;
            set
            {
                this.SetField(ref this._hideThisTab, value);
                this.SetConfig(!value);
            }
        }

        public FontSettingsObsoletePageModel(ModConfig config, Action<ModConfig> saveConfig)
        {
            this._config = config;
            this._saveConfig = saveConfig;

            this.HotkeyString = config.OpenFontSettingsMenu.ToString();
        }

        private void SetConfig(bool fontSettingsInGameMenu)
        {
            this._config.FontSettingsInGameMenu = fontSettingsInGameMenu;
            this._saveConfig(this._config);
        }
    }
}
