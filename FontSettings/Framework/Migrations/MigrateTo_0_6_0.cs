using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess;
using FontSettings.Framework.DataAccess.Models;
using StardewModdingAPI;

namespace FontSettings.Framework.Migrations
{
    /// <summary>
    /// 迁移至0.6.0<br/>
    /// · 在<see cref="FontConfig"/>中添加了一个属性<see cref="FontConfig.PixelZoom"/>，会更改SpriteText的缩放比例。默认值为0，我们需要将原有设置的该项设为1。<br/>
    /// · 将LanguageCode.en时的Locale由 "en" 改成 string.Empty。
    /// · <see cref="ModConfig.ExampleText"/>一般情况不需要再设置，因此改成空字符串。
    /// </summary>
    internal class MigrateTo_0_6_0
    {
        private readonly string _migrationDataKey = "migration";
        private readonly ISemanticVersion _targetVersion = new SemanticVersion(0, 6, 0);
        private readonly IModHelper _helper;
        private readonly IManifest _manifest;

        public MigrateTo_0_6_0(IModHelper helper, IManifest manifest)
        {
            this._helper = helper;
            this._manifest = manifest;
        }

        public void ApplyDatabaseChanges(FontConfigRepository fontConfigRepository, FontPresetRepository fontPresetRepository, 
            ModConfig modConfig, Action<ModConfig> writeModConfig)
        {
            if (this._manifest.Version.IsNewerThan(this._targetVersion)
             || this._manifest.Version.Equals(this._targetVersion))
            {
                var data = this.ReadOrCreateMigrationData(this._helper);
                if (!data.HasMigratedTo_0_6_0)
                {
                    // 一次性修改：

                    // -（必须一次性）将现存的对话字体缩放改成1f。
                    // -（未必一次性）将本地存档（包括字体设置、预设）中LanguageCode.en时Locale由"en"改为string.Empty。
                    {
                        var fontSettings = fontConfigRepository.ReadAllConfigs();
                        foreach (FontConfig config in fontSettings)
                        {
                            if (config.InGameType == GameFontType.SpriteText)
                                config.PixelZoom = 1f;

                            if (config.Lang == StardewValley.LocalizedContentManager.LanguageCode.en)
                                config.Locale = string.Empty;
                        }
                        fontConfigRepository.WriteAllConfigs(fontSettings);

                        var presets = fontPresetRepository.ReadAllPresets();
                        foreach (var pair in presets)
                        {
                            string name = pair.Key;
                            FontPreset preset = pair.Value;

                            if (preset.FontType is FontPresetFontType.Any or FontPresetFontType.Dialogue)
                                preset.PixelZoom = 1f;

                            if (preset.Lang == StardewValley.LocalizedContentManager.LanguageCode.en)
                                preset.Locale = string.Empty;

                            fontPresetRepository.WritePreset(name, preset);
                        }

                        this.SetMigrationProgress(MigrationFlag.ResetPixelZoom);
                        this.SetMigrationProgress(MigrationFlag.CorrectEnglishLocale);
                    }

                    // -（必须一次性）将ExampleText改成空字符串。
                    {
                        modConfig.ExampleText = string.Empty;
                        writeModConfig(modConfig);

                        this.SetMigrationProgress(MigrationFlag.EmptyExampleText);
                    }
                }
            }
        }

        [Obsolete("Use ApplyDatabaseChanges instead.")]
        public void Apply(IModHelper helper, IManifest manifest, FontConfigs fontSettings, Action<FontConfigs> writeFontSettings, ModConfig modConfig, Action<ModConfig> writeModConfig, FontPresetManager presetManager)
        {
            if (manifest.Version.IsNewerThan(this._targetVersion)
             || manifest.Version.Equals(this._targetVersion))
            {
                var data = this.ReadOrCreateMigrationData(helper);
                if (!data.HasMigratedTo_0_6_0)
                {
                    // 一次性修改：
                    // - 将现存的对话字体缩放改成1f。
                    foreach (FontConfig config in fontSettings)
                    {
                        if (config.InGameType == GameFontType.SpriteText)
                        {
                            config.PixelZoom = 1f;
                        }
                    }
                    writeFontSettings(fontSettings);

                    // - 将ExampleText改成空字符串。
                    modConfig.ExampleText = string.Empty;
                    writeModConfig(modConfig);

                    data.HasMigratedTo_0_6_0 = true;
                    this.WriteMigrationData(helper, data);
                }

                // 将本地存档（包括字体设置、预设）中LanguageCode.en时Locale由"en"改为string.Empty。
                {
                    foreach (FontConfig config in fontSettings)
                    {
                        if (config.Lang == StardewValley.LocalizedContentManager.LanguageCode.en)
                        {
                            config.Locale = string.Empty;
                        }
                    }
                    writeFontSettings(fontSettings);

                    var allPresets = presetManager.GetAll();
                    foreach (FontPreset preset in allPresets)
                    {
                        if (preset.Lang == StardewValley.LocalizedContentManager.LanguageCode.en
                            && preset.Locale != string.Empty)
                        {
                            presetManager.EditPreset(preset,
                                preset => preset.Locale = string.Empty);
                        }
                    }
                }
            }
        }

        private MigrationFlag _migrationProgress;
        private void SetMigrationProgress(MigrationFlag flag)
        {
            this._migrationProgress |= flag;

            if (Enum.GetValues<MigrationFlag>()
                .All(flag => this._migrationProgress.HasFlag(flag)))
            {
                var data = this.ReadOrCreateMigrationData(this._helper);
                data.HasMigratedTo_0_6_0 = true;
                this.WriteMigrationData(this._helper, data);
            }
        }

        [Flags]
        private enum MigrationFlag
        {
            ResetPixelZoom = 1,
            CorrectEnglishLocale = 2,
            EmptyExampleText = 4
        }

        private MigrationData ReadOrCreateMigrationData(IModHelper helper)
        {
            return this.ReadOrCreateMigrationData(helper, this._migrationDataKey);
        }

        private void WriteMigrationData(IModHelper helper, MigrationData value)
        {
            this.WriteMigrationData(helper, this._migrationDataKey, value);
        }

        private MigrationData ReadOrCreateMigrationData(IModHelper helper, string key)
        {
            var data = helper.Data.ReadGlobalData<MigrationData>(key);
            if (data == null)
            {
                data = new MigrationData();
                helper.Data.WriteGlobalData(key, data);
            }
            return data;
        }

        private void WriteMigrationData(IModHelper helper, string key, MigrationData value)
        {
            helper.Data.WriteGlobalData(key, value);
        }
    }
}
