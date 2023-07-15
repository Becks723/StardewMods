using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models
{
    internal record FontConfigModel(
        bool Enabled,
        string FontFile,
        int FontIndex,
        float FontSize,
        float Spacing,
        float LineSpacing,
        float CharOffsetX,
        float CharOffsetY,
        float PixelZoom,
        CharacterPatchMode CharacterPatchMode,
        IEnumerable<CharacterRange>? CharacterOverride,
        IEnumerable<CharacterRange>? CharacterAdd,
        IEnumerable<CharacterRange>? CharacterRemove,
        char? DefaultCharacter);
}
