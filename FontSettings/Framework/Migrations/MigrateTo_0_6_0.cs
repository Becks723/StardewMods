using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.Migrations
{
    /// <summary>
    /// 0.6.0在<see cref="FontConfig"/>中添加了一个属性<see cref="FontConfig.PixelZoom"/>，会更改SpriteText的缩放比例。默认值为0，我们需要将原有设置的该项设为1。
    /// </summary>
    internal class MigrateTo_0_6_0
    {
        private readonly ISemanticVersion _targetVersion = new SemanticVersion(0, 6, 0);

        public void Apply(IModHelper helper, IManifest manifest, FontConfigs fontSettings, Action<FontConfigs> writeFontSettings)
        {
            if (manifest.Version.IsNewerThan(this._targetVersion)
             || manifest.Version.Equals(this._targetVersion))
            {
                var data = this.ReadOrCreateMigrationData(helper, "migration");
                if (!data.HasMigratedTo_0_6_0)
                {
                    foreach (FontConfig config in fontSettings)
                    {
                        if (config.InGameType == GameFontType.SpriteText)
                        {
                            config.PixelZoom = 1f;
                        }
                    }
                    writeFontSettings(fontSettings);

                    data.HasMigratedTo_0_6_0 = true;
                }
            }
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
    }
}
