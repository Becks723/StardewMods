﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching.Invalidators
{
    internal abstract class BaseFontPatchInvalidator : IFontPatchInvalidator
    {
        private static readonly object _lock = new();

        public void InvalidateAndPropagate(FontContext context)
        {
            lock (_lock)
                this.InvalidateCore(context);
        }

        protected abstract void InvalidateCore(FontContext context);

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
