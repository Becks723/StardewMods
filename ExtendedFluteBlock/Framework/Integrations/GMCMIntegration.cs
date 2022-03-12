using System;
using CodeShared.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using static FluteBlockExtension.Framework.Constants;

namespace FluteBlockExtension.Framework.Integrations
{
    internal class GMCMIntegration : GenericModConfigMenuIntegrationBase
    {
        private readonly ModConfig _config;
        private readonly SoundsConfig _soundsConfig;

        public GMCMIntegration(ModConfig config, SoundsConfig soundsConfig, Action reset, Action save, IModRegistry modRegistry, IMonitor monitor, IManifest manifest)
            : base(reset, save, modRegistry, monitor, manifest)
        {
            this._config = config;
            this._soundsConfig = soundsConfig;
        }

        protected override void IntegrateOverride(GenericModConfigMenuFluentHelper helper)
        {
            helper.Register()

                .AddCheckbox(
                    name: I18n.Config_EnableMod,
                    get: () => this._config.EnableExtraPitch,
                    set: val => this._config.EnableExtraPitch = val
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
                )

                .AddSectionTitle(I18n.Config_SectionTitle_Sounds)
                //.AddTextBox(
                //    name: I18n.Config_Sounds_FolderPath,
                //    get: () => this._soundsConfig.SoundsFolderPath,
                //    set: val => this._soundsConfig.SoundsFolderPath = val,
                //    tooltip: I18n.Config_Sounds_FolderPath_Tooltip
                //)
                .AddFilePathPicker(
                    name: I18n.Config_Sounds_FolderPath,
                    tooltip: I18n.Config_Sounds_FolderPath_Tooltip
                );
        }
    }
}
