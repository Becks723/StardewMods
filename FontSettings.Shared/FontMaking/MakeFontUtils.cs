using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FontSettings.Framework
{
    internal static class MakeFontUtils
    {
        public static Texture2D GenerateTexture2D(byte[] pixels, int width, int height, GraphicsDevice? graphicsDevice = null)
        {
            if (graphicsDevice == null)
            {
                var game1Device = Game1.graphics?.GraphicsDevice;
                if (game1Device == null)
                    throw new InvalidOperationException($"The game is not running! Needs 'Game1.graphics.GraphicsDevice' but it's null.");

                graphicsDevice = game1Device;
            }

            Texture2D result = new Texture2D(graphicsDevice, width, height);

            Color[] colorData = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                byte b = pixels[i];
                colorData[i].R = b;
                colorData[i].G = b;
                colorData[i].B = b;
                colorData[i].A = b;
            }

            result.SetData(colorData);
            return result;
        }

        public static XmlSource SerializeFontFile(FontFile fontFile)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(FontFile));
            using var writer = new StringWriter();
            xmlSerializer.Serialize(writer, fontFile);

            string xml = writer.ToString();
            return new XmlSource(xml);
        }
    }
}
