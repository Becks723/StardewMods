using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal class ModConfig
    {
        public List<FontConfig> Fonts { get; set; } = new();

        public string ExampleText { get; set; } = "AaBbYyZz\n测试用例";

        public FontConfig GetOrCreateFontConfig(StardewValley.LocalizedContentManager.LanguageCode code, string locale, GameFontType inGameType)
        {
            return (from font in this.Fonts
                    where font.Lang == code && font.Locale == locale && font.InGameType == inGameType
                    select font)
                    .FirstOrDefault() ?? new FontConfig
                    {
                        Enabled = false,
                        Lang = code,
                        Locale = locale,
                        InGameType = inGameType,
                        FontSize = 24,
                        Spacing = 0,
                        LineSpacing = 24
                    };
        }
    }
}
