using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontChangers
{
    internal abstract class BaseGameFontChanger : IGameFontChanger, IAsyncGameFontChanger
    {
        public abstract bool ChangeGameFont(FontConfig font);
        public abstract Task<bool> ChangeGameFontAsync(FontConfig font);

        protected string LocalizeBaseAssetName(string baseName)
        {
            string code = string.Empty;
            string locale = FontHelpers.GetCurrentLocale();
            if (locale != "en")
                code = $".{locale}";

            return baseName + code;
        }
    }
}
