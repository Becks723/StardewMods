using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching
{
    internal interface IFontPatchInvalidator
    {
        IFontPatch? Patch { get; set; }

        bool IsInProgress { get; }

        void InvalidateAndPropagate();
    }

    internal interface ISpriteTextPatchInvalidator : IFontPatchInvalidator
    {
        void UpdateFontFile(BmFont.FontFile fontFile);
    }
}
