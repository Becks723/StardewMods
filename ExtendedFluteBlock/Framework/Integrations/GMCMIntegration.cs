using System;
using CodeShared.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using static FluteBlockExtension.Framework.Constants;

namespace FluteBlockExtension.Framework.Integrations
{
    internal class GMCMIntegration : GenericModConfigMenuIntegrationBase
    {
        private readonly ModConfig _config;

        public GMCMIntegration(ModConfig config, Action reset, Action save, IModRegistry modRegistry, IMonitor monitor, IManifest manifest)
            : base(reset, save, modRegistry, monitor, manifest)
        {
            this._config = config;
        }

        protected override void IntegrateOverride(GenericModConfigMenuFluentHelper helper)
        {
            helper.Register()

                .AddCheckbox(
                    name: I18n.Config_EnableMod,
                    get: () => this._config.EnableMod,
                    set: val => this._config.EnableMod = val
                )
                .AddSlider(
                    name: I18n.Config_MinPitch,
                    get: () => ToMidiNote(this._config.MinAccessiblePitch),
                    set: val => this._config.MinAccessiblePitch = FromMidiNote(val),
                    min: ToMidiNote(MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE),
                    max: ToMidiNote(MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE),
                    tooltip: I18n.Config_MinPitch_Tooltip
                )
                .AddSlider(
                    name: I18n.Config_MaxPitch,
                    get: () => ToMidiNote(this._config.MaxAccessiblePitch),
                    set: val => this._config.MaxAccessiblePitch = FromMidiNote(val),
                    min: ToMidiNote(MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE),
                    max: ToMidiNote(MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE),
                    tooltip: I18n.Config_MaxPitch_Tooltip
                );
        }
    }
}
