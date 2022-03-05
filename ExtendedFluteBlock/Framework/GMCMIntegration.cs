using System;
using CodeShared.Integrations;
using CodeShared.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using static FluteBlockExtension.Framework.Constants;

namespace FluteBlockExtension.Framework
{
    internal class GMCMIntegration : ModIntegration<IGenericModConfigMenuApi>
    {
        private readonly ModConfig _config;
        private readonly Action _reset;
        private readonly Action _save;
        private readonly IManifest _manifest;

        public GMCMIntegration(ModConfig config, Action reset, Action save, IModRegistry modRegistry, IMonitor monitor, IManifest manifest)
            : base(modID: "spacechase0.GenericModConfigMenu", modRegistry: modRegistry, monitor: monitor)
        {
            this._config = config;
            this._reset = reset;
            this._save = save;
            this._manifest = manifest;
        }

        protected override void IntegrateOverride(IGenericModConfigMenuApi api)
        {
            new GenericModConfigMenuFluentHelper(api, this._manifest)
                .Register(this._reset, this._save)

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
