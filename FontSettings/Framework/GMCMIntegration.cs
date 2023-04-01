using System;
using CodeShared.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace FontSettings.Framework
{
    internal class GMCMIntegration : GenericModConfigMenuIntegrationBase
    {
        private ModConfig Config { get; }

        public GMCMIntegration(ModConfig config, Action reset, Action save, IModRegistry modRegistry, IMonitor monitor, IManifest manifest)
            : base(reset, save, modRegistry, monitor, manifest)
        {
            this.Config = config;
        }

        protected override void IntegrateOverride(GenericModConfigMenuFluentHelper helper)
        {
            helper.Register()
                .AddKeyBindList(
                    name: I18n.Config_FontMenuKey,
                    tooltip: I18n.Config_FontMenuKey_Description,
                    get: () => this.Config.OpenFontSettingsMenu,
                    set: val => this.Config.OpenFontSettingsMenu = val
                )

                // diable font shadow
                .AddCheckbox(
                    name: I18n.Config_DisableTextShadow,
                    tooltip: I18n.Config_DisableTextShadow_Description,
                    get: () => this.Config.DisableTextShadow,
                    set: val => this.Config.DisableTextShadow = val
                )

                .AddTextBox(
                    name: I18n.Config_ExampleText,
                    tooltip: I18n.Config_ExampleText_Description,
                    get: () => NormalizeExampleText(this.Config.ExampleText),
                    set: val => this.Config.ExampleText = ParseBackExampleText(val)
                )

                // min font size
                .AddSlider(
                    name: I18n.Config_MinFontSize,
                    tooltip: I18n.Config_MinFontSize_Description,
                    get: () => this.Config.MinFontSize,
                    set: val => this.Config.MinFontSize = val,
                    max: 30,
                    min: 1,
                    interval: 1
                )

                // max font size
                .AddSlider(
                    name: I18n.Config_MaxFontSize,
                    tooltip: I18n.Config_MaxFontSize_Description,
                    get: () => this.Config.MaxFontSize,
                    set: val => this.Config.MaxFontSize = val,
                    max: 150,
                    min: 50,
                    interval: 1
                )

                // min spacing
                .AddSlider(
                    name: I18n.Config_MinSpacing,
                    tooltip: I18n.Config_MinSpacing_Description,
                    get: () => this.Config.MinSpacing,
                    set: val => this.Config.MinSpacing = val,
                    max: -5,
                    min: -20,
                    interval: 1
                )

                // max spacing
                .AddSlider(
                    name: I18n.Config_MaxSpacing,
                    tooltip: I18n.Config_MaxSpacing_Description,
                    get: () => this.Config.MaxSpacing,
                    set: val => this.Config.MaxSpacing = val,
                    max: 20,
                    min: 5,
                    interval: 1
                )

                // min line spacing
                .AddSlider(
                    name: I18n.Config_MinLineSpacing,
                    tooltip: I18n.Config_MinLineSpacing_Description,
                    get: () => this.Config.MinLineSpacing,
                    set: val => this.Config.MinLineSpacing = val,
                    max: 30,
                    min: 1,
                    interval: 1
                )

                // max line spacing
                .AddSlider(
                    name: I18n.Config_MaxLineSpacing,
                    tooltip: I18n.Config_MaxLineSpacing_Description,
                    get: () => this.Config.MaxLineSpacing,
                    set: val => this.Config.MaxLineSpacing = val,
                    max: 150,
                    min: 50,
                    interval: 1
                )

                // min x offset
                .AddSlider(
                    name: I18n.Config_MinOffsetX,
                    tooltip: I18n.Config_MinOffsetX_Description,
                    get: () => this.Config.MinCharOffsetX,
                    set: val => this.Config.MinCharOffsetX = val,
                    max: -5,
                    min: -25,
                    interval: 1
                )

                // max x offset
                .AddSlider(
                    name: I18n.Config_MaxOffsetX,
                    tooltip: I18n.Config_MaxOffsetX_Description,
                    get: () => this.Config.MaxCharOffsetX,
                    set: val => this.Config.MaxCharOffsetX = val,
                    max: 25,
                    min: 5,
                    interval: 1
                )

                // min y offset
                .AddSlider(
                    name: I18n.Config_MinOffsetY,
                    tooltip: I18n.Config_MinOffsetY_Description,
                    get: () => this.Config.MinCharOffsetY,
                    set: val => this.Config.MinCharOffsetY = val,
                    max: -5,
                    min: -25,
                    interval: 1
                )

                // max y offset
                .AddSlider(
                    name: I18n.Config_MaxOffsetY,
                    tooltip: I18n.Config_MaxOffsetY_Description,
                    get: () => this.Config.MaxCharOffsetY,
                    set: val => this.Config.MaxCharOffsetY = val,
                    max: 25,
                    min: 5,
                    interval: 1
                )

                // min pixel zoom
                .AddSlider(
                    name: I18n.Config_MinPixelZoom,
                    tooltip: I18n.Config_MinPixelZoom_Description,
                    get: () => this.Config.MinPixelZoom,
                    set: val => this.Config.MinPixelZoom = val,
                    max: 3,
                    min: 0.1f,
                    interval: 0.1f
                )

                // max pixel zoom
                .AddSlider(
                    name: I18n.Config_MaxPixelZoom,
                    tooltip: I18n.Config_MaxPixelZoom_Description,
                    get: () => this.Config.MaxPixelZoom,
                    set: val => this.Config.MaxPixelZoom = val,
                    max: 10,
                    min: 3,
                    interval: 1
                )

                // rich drop down
                .AddCheckbox(
                    name: I18n.Config_SimplifiedDropDown,
                    tooltip: I18n.Config_SimplifiedDropDown_Description,
                    get: () => this.Config.SimplifiedDropDown,
                    set: val => this.Config.SimplifiedDropDown = val
                );
        }

        private static string NormalizeExampleText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("\n", "\\n");
        }

        private static string ParseBackExampleText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("\\n", "\n");
        }
    }
}
