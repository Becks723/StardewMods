using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using static StbTrueTypeSharp.StbTrueType;

namespace FontSettings.Framework
{
    internal class SpriteFontGenerator
    {
        // TODO: 支持size和charRange
        public static SpriteFont FromExisting(SpriteFont existingFont,
            float? overridePixelHeight = null,
            IEnumerable<CharacterRange>? overrideCharRange = null,
            float? overrideSpacing = null,
            int? overrideLineSpacing = null,
            float extraCharOffsetX = 0,
            float extraCharOffsetY = 0)
        {
            if (existingFont is null) throw new ArgumentNullException(nameof(existingFont));

            Texture2D existingTexture = existingFont.Texture;
            //Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice,
            //    existingTexture.Width, existingTexture.Height, false, existingTexture.Format);  // TODO: existingTexture.Format 是 Dxt3，不支持GetData<Color>
            //{
            //    Color[] data = new Color[texture.Width * texture.Height];
            //    existingTexture.GetData(data);  
            //    texture.SetData(data);
            //}

            Rectangle CharOffset(Rectangle value)
            {
                value.Offset(
                    (int)Math.Round(extraCharOffsetX),
                    (int)Math.Round(extraCharOffsetY));
                return value;
            }

            return new SpriteFont(
                existingTexture,
                existingFont.Glyphs.Select(g => g.BoundsInTexture).ToList(),
                existingFont.Glyphs.Select(g => CharOffset(g.Cropping)).ToList(),
                existingFont.Characters.ToList(),
                overrideLineSpacing ?? existingFont.LineSpacing,
                overrideSpacing ?? existingFont.Spacing,
                existingFont.Glyphs.Select(g => new Vector3(g.LeftSideBearing, g.Width, g.RightSideBearing)).ToList(),
                existingFont.DefaultCharacter
            );
        }

        public static void EditExisting(SpriteFont existingFont,
            float? overridePixelHeight = null,
            IEnumerable<CharacterRange>? overrideCharRange = null,
            float? overrideSpacing = null,
            int? overrideLineSpacing = null,
            float extraCharOffsetX = 0,
            float extraCharOffsetY = 0)
        {
            if (existingFont is null) throw new ArgumentNullException(nameof(existingFont));

            // spacing
            if (overrideSpacing != null)
                existingFont.Spacing = overrideSpacing.Value;

            // line spacing
            if (overrideLineSpacing != null)
                existingFont.LineSpacing = overrideLineSpacing.Value;

            // char offset
            foreach (SpriteFont.Glyph glyph in existingFont.Glyphs)
            {
                glyph.Cropping.Offset(
                    (int)Math.Round(extraCharOffsetX),
                    (int)Math.Round(extraCharOffsetY));
            }
        }

        public static SpriteFont FromTtf(
            string ttfPath,
            int fontIndex,
            float fontPixelHeight,
            IEnumerable<CharacterRange> characterRanges,
            int? bitmapWidth = null,
            int? bitmapHeight = null,
            char? defaultCharacter = '*',
            float spacing = 0,
            int? lineSpacing = null,
            float charOffsetX = 0,
            float charOffsetY = 0)
        {
            var data = GenerateMetadata(ttfPath, fontIndex, fontPixelHeight, characterRanges, bitmapWidth, bitmapHeight, defaultCharacter, spacing, lineSpacing, charOffsetX, charOffsetY);

            Texture2D texture = MakeFontUtils.GenerateTexture2D(data.Pixels, data.Width, data.Height);
            return new SpriteFont(
                texture: texture,
                glyphBounds: data.Bounds,
                cropping: data.Cropping,
                characters: data.Characters,
                lineSpacing: data.LineSpacing,
                spacing: data.Spacing,
                kerning: data.Kerning,
                defaultCharacter: data.DefaultCharacter);
        }

        internal static unsafe SpriteFontMetadata GenerateMetadata(
            string ttfPath,
            int fontIndex,
            float fontPixelHeight,
            IEnumerable<CharacterRange> characterRanges,
            int? bitmapWidth = null,
            int? bitmapHeight = null,
            char? defaultCharacter = '*',
            float spacing = 0,
            int? lineSpacing = null,
            float charOffsetX = 0,
            float charOffsetY = 0)
        {
            byte[] ttf = File.ReadAllBytes(ttfPath);

            int offset;
            fixed (byte* ttfPtr = ttf)
            {
                offset = stbtt_GetFontOffsetForIndex(ttfPtr, fontIndex);
                if (offset == -1)
                    throw new IndexOutOfRangeException($"字体索引超出范围。索引值：{fontIndex}");
            }

            stbtt_fontinfo fontInfo = CreateFont(ttf, offset);
            if (fontInfo == null)
                throw new Exception("无法初始化字体。");
            try
            {
                float scale = stbtt_ScaleForPixelHeight(fontInfo, fontPixelHeight);

                const int padding = 1;
                int finalTexWidth, finalTexHeight;
                if (bitmapWidth is null || bitmapHeight is null)
                    EstimateTextureSize(fontInfo, characterRanges, scale, out finalTexWidth, out finalTexHeight, padding);
                else
                {
                    finalTexWidth = bitmapWidth.Value;
                    finalTexHeight = bitmapHeight.Value;
                }

                int ascent, descent, lineGap;
                stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);
                if (ascent == 0 && descent == 0)
                    stbtt_GetFontVMetricsOS2(fontInfo, &ascent, &descent, &lineGap);
                int lineHeight = (int)((ascent - descent + lineGap) * scale);

                var bounds = new List<Rectangle>();
                var cropping = new List<Rectangle>();
                var chars = new List<char>();
                var kerning = new List<Vector3>();

                byte[] pixels = new byte[finalTexWidth * finalTexHeight];
                stbtt_pack_context ctx = new stbtt_pack_context();
                fixed (byte* pxPtr = pixels)
                    stbtt_PackBegin(ctx, pxPtr, finalTexWidth, finalTexHeight, finalTexWidth, padding, null);

                foreach (CharacterRange range in characterRanges)
                {
                    stbtt_packedchar[] arr = new stbtt_packedchar[range.End - range.Start + 1];
                    fixed (stbtt_packedchar* cPtr = arr)
                        stbtt_PackFontRange(ctx, fontInfo.data, fontIndex, fontPixelHeight, range.Start, arr.Length, cPtr);

                    for (int i = 0; i < arr.Length; i++)
                    {
                        var pc = arr[i];

                        float yOff = pc.yoff;
                        yOff += ascent * scale;

                        int width = pc.x1 - pc.x0;
                        int height = pc.y1 - pc.y0;
                        chars.Add((char)(range.Start + i));
                        bounds.Add(new Rectangle(pc.x0, pc.y0, width, height));
                        cropping.Add(new Rectangle((int)Math.Round(charOffsetX), (int)Math.Round(yOff + charOffsetY), width, height));
                        kerning.Add(new Vector3(pc.xoff, width, pc.xadvance - pc.xoff - width));
                    }
                }

                return new SpriteFontMetadata(
                    Pixels: pixels,
                    Width: finalTexWidth,
                    Height: finalTexHeight,
                    Bounds: bounds,
                    Cropping: cropping,
                    Characters: chars,
                    LineSpacing: lineSpacing ?? lineHeight,
                    Spacing: spacing,
                    Kerning: kerning,
                    DefaultCharacter: defaultCharacter);
            }
            finally
            {
                fontInfo.Dispose();
            }
        }

        private static Texture2D GenerateTexture(byte[] pixels, int width, int height, GraphicsDevice? graphicsDevice = null)
        {
            if (graphicsDevice == null)
            {
                var game1Device = Game1.graphics?.GraphicsDevice;
                if (game1Device == null)
                    throw new InvalidOperationException($"The game is not running! Needs 'Game1.graphics?.GraphicsDevice' but it's null.");

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

        private static void EstimateTextureSize(stbtt_fontinfo fontInfo, IEnumerable<CharacterRange> ranges, float scale,
            out int width, out int height, int padding = 1, bool requirePowerOfTwo = true)
        {
            if (padding < 0) padding = 0;

            var glyphSizes = GetGlyphSizes(fontInfo, ranges, scale)
                .Select(size => size + new Point(padding));

            width = GuessWidth(glyphSizes.ToArray(), requirePowerOfTwo);
            int bottomY = 0;
            int x = 0, y = 0;
            foreach (Point size in glyphSizes)
            {
                // 需换行
                if (x + size.X > width)
                {
                    x = 0;        // 重置X坐标。
                    y = bottomY;  // 更新Y坐标。
                }

                x += size.X;

                // 更新最底y值。
                if (bottomY < y + size.Y)
                    bottomY = y + size.Y;
            }

            height = MakeValidTextureSize(bottomY, requirePowerOfTwo);
        }

        private static unsafe IEnumerable<Point> GetGlyphSizes(stbtt_fontinfo fontInfo, IEnumerable<CharacterRange> ranges, float scale)
        {
            List<Point> result = new();
            foreach (CharacterRange range in ranges)
            {
                for (int c = range.Start; c <= range.End; c++)
                {
                    int x0, y0, x1, y1;
                    stbtt_GetCodepointBitmapBox(fontInfo, c, scale, scale, &x0, &y0, &x1, &y1);
                    result.Add(new Point(x1 - x0, y1 - y0));
                }
            }
            return result;
        }

        private static int GuessWidth(Point[] glyphSizes, bool requirePowerOfTwo)
        {
            int totalSize = 0;
            int maxWidth = 0;

            foreach (Point size in glyphSizes)
            {
                maxWidth = Math.Max(maxWidth, size.X);
                totalSize += size.X * size.Y;
            }

            int finalWidth = Math.Max((int)Math.Sqrt(totalSize), maxWidth);
            return MakeValidTextureSize(finalWidth, requirePowerOfTwo);
        }

        // From Microsoft.Xna.Framework.Content.Pipeline.Graphics.GlyphPacker
        // Rounds a value up to the next larger valid texture size.
        private static int MakeValidTextureSize(int value, bool requirePowerOfTwo)
        {
            // In case we want to compress the texture, make sure the size is a multiple of 4.
            const int blockSize = 4;

            if (requirePowerOfTwo)
            {
                // Round up to a power of two.
                int powerOfTwo = blockSize;

                while (powerOfTwo < value)
                    powerOfTwo <<= 1;

                return powerOfTwo;
            }
            else
            {
                // Round up to the specified block size.
                return (value + blockSize - 1) & ~(blockSize - 1);
            }
        }

    }

    internal record SpriteFontMetadata(
        byte[] Pixels,
        int Width,
        int Height,
        List<Rectangle> Bounds,
        List<Rectangle> Cropping,
        List<char> Characters,
        float Spacing,
        int LineSpacing,
        List<Vector3> Kerning,
        char? DefaultCharacter);
}
