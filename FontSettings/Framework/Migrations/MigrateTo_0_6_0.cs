using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
