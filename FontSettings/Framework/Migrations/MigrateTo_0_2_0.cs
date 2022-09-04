using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Legacy;
using StardewModdingAPI;

namespace FontSettings.Framework.Migrations
{
    /// <summary>0.1.0到0.2.0版本迁移：
    /// <list type="bullet">
    /// <item>字体设置数据储存位置：config.json -> 本地（游戏存档文件夹）。</item>
    /// <item>配置模型改了：旧（<see cref="ModConfig_0_1_0"/>）-> 新（<see cref="ModConfig"/>）。</item>
    /// <item>字体数据添加了一个属性<see cref="FontConfig.Locale"/>。</item>
    /// </list>
    /// </summary>
    internal class MigrateTo_0_2_0
    {
        public bool NeedMigrate(IModHelper helper)
        {
            string configPath = Path.Combine(helper.DirectoryPath, "config.json");
            if (!File.Exists(configPath))  // 如果没有找到config.json，说明根本没有进过游戏，不需要迁移。
                return false;

            // 我们创建一个临时的配置文件拷贝。
            string copiedConfigPath = Path.Combine(helper.DirectoryPath, "config-migrating-0.1-to-0.2.json");
            File.Copy(configPath, copiedConfigPath);

            try
            {
                ModConfig_0_1_0 config = helper.Data.ReadJsonFile<ModConfig_0_1_0>(Path.GetFileName(copiedConfigPath));
                if (config.Fonts.Count > 0)
                    return true;
                return false;
            }
            catch  // 要是有什么乱七八糟的错误，一律视作需要迁移。
            {
                return true;
            }
            finally
            {
                File.Delete(copiedConfigPath);  // 用完删除临时的配置文件。
            }
        }

        public void Apply(IModHelper helper, out ModConfig config)
        {
            ILog.Trace($"检测到需要迁移至0.2.0版本，正在迁移……");
            try
            {
                ModConfig_0_1_0 legacyConfig = helper.ReadConfig<ModConfig_0_1_0>();

                config = new ModConfig();
                config.ExampleText = legacyConfig.ExampleText;
                config.Fonts = new FontConfigs();
                foreach (FontConfig_0_1_0 legacyFont in legacyConfig.Fonts)
                {
                    if (legacyFont.Lang is StardewValley.LocalizedContentManager.LanguageCode.mod)  // 如果mod语言，直接丢弃该项，因为还不兼容，而且也无法推测具体是哪种语言。
                        continue;

                    FontConfig font = new FontConfig
                    {
                        Enabled = legacyFont.Enabled,
                        Lang = legacyFont.Lang,
                        Locale = FontHelpers.GetLocale(legacyFont.Lang),  // 补充字体的Locale属性。
                        InGameType = legacyFont.InGameType,
                        ExistingFontPath = legacyFont.ExistingFontPath,
                        FontFilePath = legacyFont.FontFilePath,
                        FontIndex = legacyFont.FontIndex,
                        FontSize = legacyFont.FontSize,
                        Spacing = legacyFont.Spacing,
                        LineSpacing = legacyFont.LineSpacing,
                        TextureWidth = legacyFont.TextureWidth,
                        TextureHeight = legacyFont.TextureHeight,
                        CharacterRanges = legacyFont.CharacterRanges,
                    };
                    config.Fonts.Add(font);
                }
                ILog.Trace($"已迁移至0.2.0版本。");
            }
            catch (Exception ex)
            {
                ILog.Error($"在迁移至0.2.0版本时遇到问题，数据丢失。{ex.Message}\n{ex.StackTrace}");
                config = new ModConfig();  // 重置
            }
        }
    }
}
