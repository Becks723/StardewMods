using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontChangers
{
    internal abstract class BaseGameFontChanger : IGameFontChanger, IAsyncGameFontChanger
    {
        public abstract IGameFontChangeResult ChangeGameFont(FontConfig font);
        public abstract Task<IGameFontChangeResult> ChangeGameFontAsync(FontConfig font);

        protected string LocalizeBaseAssetName(string baseName)
        {
            string code = string.Empty;
            string locale = FontHelpers.GetCurrentLocale();
            if (locale != string.Empty)
                code = $".{locale}";

            return baseName + code;
        }

        protected IGameFontChangeResult GetSuccessResult()
        {
            return new GameFontChangeResult(true, null);
        }

        protected IGameFontChangeResult GetErrorResult(Exception exception)
        {
            return new GameFontChangeResult(false, exception);
        }
    }
}
