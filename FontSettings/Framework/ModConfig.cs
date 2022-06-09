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

        public FontConfig GetFontConfig(LanguageCode currentLang, GameFontType inGameType)
        {
            return (from font in this.Fonts
                    where font.Lang == currentLang && font.InGameType == inGameType
                    select font)
                   .FirstOrDefault();
        }

        public void SetFontConfig(LanguageCode currentLang, GameFontType inGameType, string fontFilePath = null, bool? enabled = null)  // TODO: 如果想赋null值怎么办
        {
            FontConfig font = this.GetFontConfig(currentLang, inGameType);
            if (fontFilePath != null)
                font.FontFilePath = fontFilePath;
            if (enabled != null)
                font.Enabled = enabled.Value;
        }
    }
}
