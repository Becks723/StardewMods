using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Enabled"></param>
    /// <param name="FontFilePath">Full path to the font file.</param>
    /// <param name="FontIndex">When the font file is a TTC or OTC, the index of the font in this file. Otherwise, keep 0.</param>
    /// <param name="FontSize"></param>
    /// <param name="Spacing"></param>
    /// <param name="LineSpacing"></param>
    /// <param name="CharOffsetX"></param>
    /// <param name="CharOffsetY"></param>
    /// <param name="CharacterRanges">Must not be null.</param>
    internal record FontConfig(
        bool Enabled,
        string FontFilePath,
        int FontIndex,
        float FontSize,
        float Spacing,
        float LineSpacing,
        float CharOffsetX,
        float CharOffsetY,
        IEnumerable<CharacterRange> CharacterRanges) : IExtensible
    {
        public virtual T GetInstance<T>() where T : class => this as T;

        public virtual bool Supports<T>() where T : class => this is T;
    }
}
