using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching.Invalidators
{
    internal abstract class BaseFontPatchInvalidator : IFontPatchInvalidator
    {
        public void InvalidateAndPropagate(FontPatchContext context)
        {
            this.InvalidateCore(context);
        }

        protected abstract void InvalidateCore(FontPatchContext context);

        protected string LocalizeBaseAssetName(string baseName)
        {
            string locale = FontHelpers.GetCurrentLocale();

            string code = locale != string.Empty
                ? $".{locale}"
                : string.Empty;
            return baseName + code;
        }
    }
}
