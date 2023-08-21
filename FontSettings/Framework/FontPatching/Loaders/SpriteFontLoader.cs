using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontPatching.Loaders
{
    internal class SpriteFontLoader : BaseFontLoader<SpriteFont>
    {
        public SpriteFontLoader(FontConfig config, int priority) 
            : base(config, priority)
        {
        }

        protected override SpriteFont Load(FontConfig config)
        {
            return SpriteFontGenerator.FromTtf(  // TODO: processing
                ttfPath: config.FontFilePath,
                fontIndex: config.FontIndex,
                fontPixelHeight: config.FontSize,
                characterRanges: config.CharacterRanges,
                spacing: config.Spacing,
                lineSpacing: (int)config.LineSpacing,
                charOffsetX: config.CharOffsetX,
                charOffsetY: config.CharOffsetY);
        }
    }
}
