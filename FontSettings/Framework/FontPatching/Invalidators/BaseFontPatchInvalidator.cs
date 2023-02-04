using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching.Invalidators
{
    internal abstract class BaseFontPatchInvalidator : IFontPatchInvalidator
    {
        IFontPatch IFontPatchInvalidator.Patch { get; set; }

        public bool IsInProgress { get; private set; }

        public void InvalidateAndPropagate()
        {
            if (this.IsInProgress) return;

            try
            {
                this.IsInProgress = true;

                this.InvalidateCore();
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        protected abstract void InvalidateCore();

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
