using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess;
using FontSettings.Framework.DataAccess.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace FontSettings.Framework.Migrations
{
    /// <summary>
    /// 迁移至0.12.0<br/>
    /// · 在<see cref="FontConfigData"/>中添加了一个属性<see cref="FontConfigData.Mask"/>。需要给原有设置添加该字段，并设置为<see cref="Color.White"/>。<br/>
    /// · 在<see cref="FontConfigData"/>中添加了一个属性<see cref="FontConfigData.DefaultCharacter"/>。需要给原有设置添加该字段，并设置为<see langword="*"/>。<br/>
    /// · <see cref="FontPresetData"/>中同样上述两项更改。
    /// </summary>
    internal class MigrateTo_0_12_0
    {
        private readonly string _migrationDataKey = "migration";
        private readonly IModHelper _helper;

        public MigrateTo_0_12_0(IModHelper helper)
        {
            this._helper = helper;
        }

        public void ApplyDatabaseChanges(FontConfigRepository fontConfigRepository, FontPresetRepository presetRepository)
        {
            var data = this.ReadOrCreateMigrationData(this._helper);
            if (!data.HasMigratedTo_0_12_0)
            {
                // 一次性修改：
                // 添加Mask字段，并赋值为Color.White。
                // 添加DefaultCharacter字段，并赋值为*。
                {
                    // 数据
                    var fontSettings = fontConfigRepository.ReadAllConfigs();
                    foreach (FontConfigData config in fontSettings)
                    {
                        config.Mask = Color.White;
                        config.DefaultCharacter = '*';
                    }
                    fontConfigRepository.WriteAllConfigs(fontSettings);

                    // 预设
                    var presets = presetRepository.ReadAllPresets();
                    foreach (var pair in presets)
                    {
                        string key = pair.Key;
                        FontPresetData preset = pair.Value;

                        preset.Mask = Color.White;
                        preset.DefaultCharacter = '*';

                        presetRepository.WritePreset(key, preset);
                    }
                }

                data.HasMigratedTo_0_12_0 = true;
                this.WriteMigrationData(this._helper, data);
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
