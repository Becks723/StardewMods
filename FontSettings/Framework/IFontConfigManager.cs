using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal interface IFontConfigManager
    {
        /// <summary>Three in one: add, edit, delete.</summary>
        void UpdateFontConfig(LanguageInfo language, GameFontType fontType, FontConfig_? config);

        bool TryGetFontConfig(LanguageInfo language, GameFontType fontType, out FontConfig_? config);

        IDictionary<FontConfigKey, FontConfig_> GetAllFontConfigs();
    }
}
