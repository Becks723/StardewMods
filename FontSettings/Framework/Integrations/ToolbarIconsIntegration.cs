using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeShared.Integrations.ToolbarIcons;
using StardewModdingAPI;

namespace FontSettings.Framework.Integrations
{
    internal class ToolbarIconsIntegration : ToolbarIconsIntegrationBase
    {
        private readonly string _uniqueId;
        private readonly Action _openFontSettingsMenu;

        public ToolbarIconsIntegration(IModRegistry modRegistry, IMonitor monitor, string uniqueId, Action openFontSettingsMenu)
            : base(modRegistry, monitor)
        {
            this._uniqueId = uniqueId;
            this._openFontSettingsMenu = openFontSettingsMenu;
        }

        protected override void IntegrateOverride(IToolbarIconsApi api)
        {
            api.AddToolbarIcon(
                id: this._uniqueId,
                texturePath: Textures.FontMenuIconKey,
                sourceRect: null,
                hoverText: "Font Settings");
            api.Subscribe(this.OnToolbarIconPressed);
        }

        private void OnToolbarIconPressed(IIconPressedEventArgs e)
        {
            if (e.Id == this._uniqueId)
                this._openFontSettingsMenu();
        }
    }
}
