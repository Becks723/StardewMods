using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.DataAccess.Models
{
    internal class FontConfigs : List<FontConfigData>
    {
        public FontConfigData GetOrCreateFontConfig(StardewValley.LocalizedContentManager.LanguageCode code, string locale, GameFontType inGameType, Func<FontConfigData> createFontConfig)
        {
            if (this.TryGetFontConfig(code, locale, inGameType, out var got))
                return got;

            got = createFontConfig();
            this.Add(got);
            return got;
        }

        public FontConfigData GetOrCreateFontConfig(StardewValley.LocalizedContentManager.LanguageCode code, string locale, GameFontType inGameType)
        {
            return this.GetOrCreateFontConfig(code, locale, inGameType, () =>
            {
                return new FontConfigData
                {
                    Lang = code,
                    Locale = locale,
                    InGameType = inGameType
                };
            });
        }

        public FontConfigData GetFontConfig(StardewValley.LocalizedContentManager.LanguageCode code, string locale, GameFontType inGameType)
        {
            return (from font in this
                    where font.Lang == code && font.Locale == locale && font.InGameType == inGameType
                    select font)
                    .FirstOrDefault();
        }

        public bool TryGetFontConfig(StardewValley.LocalizedContentManager.LanguageCode code, string locale, GameFontType inGameType, out FontConfigData fontConfig)
        {
            FontConfigData got = this.GetFontConfig(code, locale, inGameType);

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
