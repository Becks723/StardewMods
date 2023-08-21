using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.Exporting.Writers
{
    [ObjectWriter(typeof(SpriteFont))]
    internal class SpriteFontWriter : BaseObjectWriter<SpriteFont>
    {
        protected override void Write(XnbWriter writer, SpriteFont font)
        {
            writer.WriteObject<Texture2D>(font.Texture);
            writer.WriteObject<List<Rectangle>>(font.Glyphs.Select(g => g.BoundsInTexture).ToList());
            writer.WriteObject<List<Rectangle>>(font.Glyphs.Select(g => g.Cropping).ToList());
            writer.WriteObject<List<char>>(font.Glyphs.Select(g => g.Character).ToList());
            writer.Write(font.LineSpacing);
            writer.Write(font.Spacing);
            writer.WriteObject<List<Vector3>>(font.Glyphs.Select(g => new Vector3(g.LeftSideBearing, g.Width, g.RightSideBearing)).ToList());
            writer.Write(font.DefaultCharacter != null);
            if (font.DefaultCharacter != null)
                writer.Write(font.DefaultCharacter.Value);
        }

        protected override string GetTypeReaderName()
        {
            return "Microsoft.Xna.Framework.Content.SpriteFontReader, Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";
        }
    }
}
