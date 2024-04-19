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

        /// <summary>Remove duplicated item of same <see cref="FontConfigData.Lang"/>, <see cref="FontConfigData.Locale"/> and <see cref="FontConfigData.InGameType"/>, then return the distincted collection.</summary>
        public FontConfigs Distinct()
        {
            for (int i = 0; i < this.Count; i++)
            {
                FontConfigData current = this[i];
                for (int j = i + 1; j < this.Count; j++)
                {
                    if (current.IsSameContextWith(this[j]))
                    {
                        this.RemoveAt(j);
                        --j;
                    }
                }
            }
            return this;
        }
    }
}
