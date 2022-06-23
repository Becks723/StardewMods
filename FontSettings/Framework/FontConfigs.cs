using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal class FontConfigs : List<FontConfig>
    {
        public FontConfig GetOrCreateFontConfig(StardewValley.LocalizedContentManager.LanguageCode code, string locale, GameFontType inGameType)
        {
            if (code is StardewValley.LocalizedContentManager.LanguageCode.en && string.IsNullOrEmpty(locale))
                locale = "en";

            FontConfig result = (from font in this
                                 where font.Lang == code && font.Locale == locale && font.InGameType == inGameType
                                 select font)
                                 .FirstOrDefault();
            if (result == null)
            {
                result = new FontConfig
                {
                    Enabled = false,
                    Lang = code,
                    Locale = locale,
                    InGameType = inGameType,
                    FontSize = 24,
                    Spacing = 0,
                    LineSpacing = 24
                };
                this.Add(result);
            }

            return result;
        }
    }
}
