using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.Exporting.Writers
{
    [ObjectWriter(typeof(Texture2D))]
    internal class Texture2DWriter : BaseObjectWriter<Texture2D>
    {
        protected override void Write(XnbWriter writer, Texture2D texture)
        {
            var format = (SurfaceFormat)typeof(Texture2D).GetField("_format", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(texture);
            int levelCount = (int)typeof(Texture2D).GetField("_levelCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(texture);

            writer.Write((int)format);
            writer.Write(texture.Width);
            writer.Write(texture.Height);
            writer.Write(levelCount);

            byte[] data = new byte[texture.Width * texture.Height * 4];
            for (int i = 0; i < levelCount; i++)
            {
                FontHelpers.BlockOnUIThread(() => texture.GetData(i, null, data, 0, data.Length));
                //texture.GetData(i, null, data, 0, data.Length);
                writer.Write(data.Length);
                writer.Write(data);
            }
        }

        protected override string GetTypeReaderName()
        {
            return "Microsoft.Xna.Framework.Content.Texture2DReader, Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";
        }
    }
}
