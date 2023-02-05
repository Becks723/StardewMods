using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontPatching.Editors
{
    internal class SpriteFontEditor : BaseFontEditor<SpriteFont>
    {
        public SpriteFontEditor(FontConfig config)
            : base(config)
        {
        }

        protected override void Edit(SpriteFont font, FontConfig config)
        {
            SpriteFontGenerator.EditExisting(
                existingFont: font,
                overridePixelHeight: config.FontSize,
                overrideCharRange: config.CharacterRanges,
                overrideSpacing: config.Spacing,
                overrideLineSpacing: (int)config.LineSpacing,
                extraCharOffsetX: config.CharOffsetX,
                extraCharOffsetY: config.CharOffsetY);
        }
    }
}
