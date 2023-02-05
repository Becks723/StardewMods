using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.DataAccess.Models
{
    internal class FontConfigs : List<FontConfig>
    {
        public FontConfig GetOrCreateFontConfig(StardewValley.LocalizedContentManager.LanguageCode code, string locale, GameFontType inGameType, Func<FontConfig> createFontConfig)
        {
            if (this.TryGetFontConfig(code, locale, inGameType, out var got))
                return got;

            got = createFontConfig();
            this.Add(got);
            return got;
        }

        public FontConfig GetOrCreateFontConfig(StardewValley.LocalizedContentManager.LanguageCode code, string locale, GameFontType inGameType)
        {
            return this.GetOrCreateFontConfig(code, locale, inGameType, () =>
            {
                return new FontConfig
                {
                    Lang = code,
                    Locale = locale,
                    InGameType = inGameType
                };
            });
        }

        public FontConfig GetFontConfig(StardewValley.LocalizedContentManager.LanguageCode code, string locale, GameFontType inGameType)
        {
            return (from font in this
                    where font.Lang == code && font.Locale == locale && font.InGameType == inGameType
                    select font)
                    .FirstOrDefault();
        }

        public bool TryGetFontConfig(StardewValley.LocalizedContentManager.LanguageCode code, string locale, GameFontType inGameType, out FontConfig fontConfig)
        {
            FontConfig got = this.GetFontConfig(code, locale, inGameType);

            if (got != null)
            {
                fontConfig = got;
                return true;
            }
            else
            {
                fontConfig = null;
                return false;
            }
        }
    }
}
