using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal interface IFontGenerator
    {
        ISpriteFont GenerateFont(FontGeneratorParameter param);
    }

    internal interface IAsyncFontGenerator
    {
        Task<ISpriteFont> GenerateFontAsync(FontGeneratorParameter param);

        Task<ISpriteFont> GenerateFontAsync(FontGeneratorParameter param, CancellationToken cancellationToken);
    }

    internal abstract record FontGeneratorParameter(
        string FontFilePath,
        float FontSize,
        float Spacing,
        float LineSpacing,
        IEnumerable<CharacterRange> CharacterRanges,

        int FontIndex = 0,
        float CharOffsetX = 0,
        float CharOffsetY = 0);
}
