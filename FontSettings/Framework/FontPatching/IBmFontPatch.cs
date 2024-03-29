﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching
{
    internal interface IBmFontPatch : IFontPatch
    {
        IDictionary<string, IFontLoader>? PageLoaders { get; }  // null if not supported.

        float FontPixelZoom { get; }
    }
}
