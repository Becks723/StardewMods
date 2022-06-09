using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;  // Game1.graphics.GraphicsDevice
using static StbTrueTypeSharp.StbTrueType;

namespace BmFontCS
{
    public unsafe class BmFont
    {
        private static stbtt_fontinfo _fontInfo;
        private static float _scale;
        private static float _ascender, _descender, _lineHeight;

        public static void GenerateIntoMemory(string fontFilePath, out FontFile fontFile, out Texture2D[] pages, BmFontSettings settings)
        {
            byte[] ttf = File.ReadAllBytes(fontFilePath);

            _fontInfo = new stbtt_fontinfo();

            fixed (byte* ttfPtr = ttf)
            {
                int offset = stbtt_GetFontOffsetForIndex(ttfPtr, settings.FontIndex);
                if (offset == -1)
                    throw new IndexOutOfRangeException($"字体索引超出范围。索引值：{settings.FontIndex}");
                if (stbtt_InitFont(_fontInfo, ttfPtr, offset) == 0)
                    throw new Exception("无法初始化字体。");
            }

            InitFields(settings);
            var codepoints = GetChars(settings.Chars, settings.CharsFiles);
            Glyph[] glyphs = CollectGlyphs(codepoints).ToArray();
            Size[] pageSizes = ArrangeGlyphs(glyphs, settings);
            Page[] pgs = RenderGlyphsToPages(glyphs, pageSizes, settings);

            fontFile = GetFontInfo(fontFilePath, settings, pgs.Length, glyphs);
            pages = pgs.Select(pg =>
            {
                Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, pg.Width, pg.Height);

                Color[] data = new Color[pg.Width * pg.Height];
                for (int i = 0; i < pg.Buffer.Length; i++)
                {
                    byte b = pg.Buffer[i];
                    data[i].R = b;
                    data[i].G = b;
                    data[i].B = b;
                    data[i].A = b;
                }
                texture.SetData(data);
                return texture;
            }).ToArray();
        }

        public static void Generate(string fontFile, string output, BmFontSettings settings)
        {
            byte[] ttf = File.ReadAllBytes(fontFile);

            _fontInfo = new stbtt_fontinfo();

            fixed (byte* ttfPtr = ttf)
            {
                int offset = stbtt_GetFontOffsetForIndex(ttfPtr, settings.FontIndex);
                if (offset == -1)
                    throw new IndexOutOfRangeException($"字体索引超出范围。索引值：{settings.FontIndex}");
                if (stbtt_InitFont(_fontInfo, ttfPtr, offset) == 0)
                    throw new Exception("无法初始化字体。");
            }

            InitFields(settings);
            var codepoints = GetChars(settings.Chars, settings.CharsFiles);
            Glyph[] glyphs = CollectGlyphs(codepoints).ToArray();
            Size[] pageSizes = ArrangeGlyphs(glyphs, settings);
            Page[] pages = RenderGlyphsToPages(glyphs, pageSizes, settings);
            SaveToFiles(output, pages);
        }

        private static void SaveToFiles(string output, Page[] pages)
        {

        }

        private static FontFile GetFontInfo(string fontFilePath, BmFontSettings settings, int pages, Glyph[] glyphs)
        {
            return new FontFile
            {
                Info = new FontInfo
                {
                    Face = Path.GetFileNameWithoutExtension(fontFilePath),
                    Size = settings.FontSize,
                    Bold = 0,             // TODO
                    Italic = 0,           // TODO
                    CharSet = null,       // TODO
                    Unicode = 1,          // TODO
                    StretchHeight = 100,  // TODO
                    Smooth = 0,           // TODO
                    SuperSampling = 1, // aa   // TODO
                    Padding = "0,0,0,0", //按上右下左 // TODO 
                    Spacing = "0,0",     //按左右    // TODO
                    OutLine = 0           // TODO
                },
                Common = new FontCommon
                {
                    LineHeight = (int)Math.Round(_lineHeight),
                    Base = (int)Math.Round(_ascender),
                    ScaleW = settings.TextureSize.Width,
                    ScaleH = settings.TextureSize.Height,
                    Pages = pages,
                    Packed = 0,         // TODO
                    AlphaChannel = 0,   // TODO
                    RedChannel = 4,     // TODO
                    GreenChannel = 4,   // TODO
                    BlueChannel = 4,    // TODO
                },
                Pages = new List<FontPage>(pages),
                Chars = new List<FontChar>(glyphs.Select(glyph => new FontChar
                {
                    ID = glyph.Id,
                    X = glyph.X,
                    Y = glyph.Y,
                    Width = glyph.Width,
                    Height = glyph.Height,
                    XOffset = glyph.XOffset,
                    YOffset = glyph.YOffset,
                    XAdvance = glyph.XAdvance,
                    Page = glyph.Page,
                    Channel = 15
                })),
                Kernings = new List<FontKerning>() // TODO
            };
        }

        private static Page[] RenderGlyphsToPages(Glyph[] glyphs, Size[] pages, BmFontSettings settings)
        {
            List<Page> result = new(pages.Length);

            for (int i = 0; i < pages.Length; i++)
            {
                byte[] buffer = new byte[pages[i].Width * pages[i].Height];
                foreach (Glyph glyph in glyphs)
                {
                    if (glyph.Page != i)
                        continue;

                    fixed (byte* bufferPtr = buffer)
                        stbtt_MakeGlyphBitmapSubpixel(_fontInfo,
                            bufferPtr + glyph.X + glyph.Y * pages[i].Width,
                            glyph.Width, glyph.Height, pages[i].Width,
                            _scale, _scale,
                            0, 0,
                            glyph.GlyphIndex);
                }

                result.Add(new Page
                {
                    Width = pages[i].Width,
                    Height = pages[i].Height,
                    Id = i,
                    Buffer = buffer
                });
            }

            return result.ToArray();
        }

        private static Size[] ArrangeGlyphs(Glyph[] glyphs, BmFontSettings settings)
        {
            List<Size> pages = new();

            int x = 0,
                y = 0,
                width = settings.TextureSize.Width,
                height = settings.TextureSize.Height,
                bottomY = 0;
            pages.Add(new Size(width, height));
            int currentPage = 0;
            foreach (Glyph glyph in glyphs)
            {
                // 换行
                if (x + glyph.Width > width)
                {
                    x = 0;
                    y = bottomY;
                }

                // 下一页。
                if (y + glyph.Height > height)
                {
                    x = 0;
                    y = 0;
                    width = settings.TextureSize.Width;
                    height = settings.TextureSize.Height;
                    bottomY = 0;
                    pages.Add(new Size(width, height));
                    currentPage++;
                }

                glyph.X = x;
                glyph.Y = y;
                glyph.Page = currentPage;
                x += glyph.Width;
                if (bottomY < y + glyph.Height)
                    bottomY = y + glyph.Height;
            }

            return pages.ToArray();
        }

        private static void InitFields(BmFontSettings settings)
        {
            _scale = stbtt_ScaleForPixelHeight(_fontInfo, settings.FontSize);

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(_fontInfo, &ascent, &descent, &lineGap);
            if (ascent == 0 && descent == 0)
                stbtt_GetFontVMetricsOS2(_fontInfo, &ascent, &descent, &lineGap);
            _ascender = ascent * _scale;
            _descender = descent * _scale;
            _lineHeight = (ascent - descent + lineGap) * _scale;
        }

        private static HashSet<int> GetChars(UnicodeRange[] chars, string[] charsFiles)
        {
            HashSet<int> result = new();

            if (charsFiles != null)
                foreach (string file in charsFiles)
                {
                    using StreamReader reader = File.OpenText(file);
                    int c;
                    while ((c = reader.Read()) != -1)
                        result.Add(c);
                }

            if (chars != null)
                foreach (UnicodeRange range in chars)
                    for (int c = range.Start; c <= range.End; c++)
                        result.Add(c);

            return result;
        }

        private static HashSet<Glyph> CollectGlyphs(HashSet<int> codepoints)
        {
            HashSet<Glyph> result = new();

            int x0 = 0,
                y0 = 0,
                x1 = 0,
                y1 = 0,
                advanceWidth = 0;
            foreach (int codepoint in codepoints)
            {
                int glyphIndex = stbtt_FindGlyphIndex(_fontInfo, codepoint);
                if (glyphIndex != 0)  // 未定义 TODO: 警告
                {
                    Glyph glyph = new Glyph();
                    stbtt_GetGlyphBox(_fontInfo, glyphIndex, &x0, &y0, &x1, &y1);
                    stbtt_GetGlyphHMetrics(_fontInfo, glyphIndex, &advanceWidth, null);

                    glyph.Id = codepoint;
                    glyph.GlyphIndex = glyphIndex;
                    glyph.Width = (int)((x1 - x0) * _scale);
                    glyph.Height = (int)((y1 - y0) * _scale);
                    glyph.XAdvance = (int)(advanceWidth * _scale);
                    glyph.XOffset = (int)(x0 * _scale);
                    glyph.YOffset = (int)(_ascender - y1 * _scale);
                    result.Add(glyph);
                }
            }
            return result;
        }

        //private class FontInfo
        //{
        //    public class Info
        //    {
        //        public string Face;
        //        public int Size;
        //        public bool Bold;
        //        public bool Italic;
        //        public int Charset;
        //        public bool Unicode;
        //        public int stretchH;
        //        public bool Smooth;
        //        public int Aa;
        //        public Padding Padding;
        //        public Spacing Spacing;
        //        public int Outline;
        //    }

        //    public class Common
        //    {
        //        public int LineHeight;
        //        public int Base;
        //        public int ScaleW;
        //        public int ScaleH;
        //        public int Pages;
        //        public bool Packed;
        //        public int AlphaChnl;
        //        public int RedChnl;
        //        public int BlueChnl;
        //        public int GreenChnl;
        //    }
        //}

        private class Glyph
        {
            public int Id;
            public int GlyphIndex;
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public int XOffset;
            public int YOffset;
            public int XAdvance;
            public int Page;
        }

        private class Page
        {
            public int Width;
            public int Height;
            public byte[] Buffer;
            public int Id;
        }
    }

    public class BmFontSettings
    {
        public static BmFontSettings Default { get; } = new BmFontSettings();

        /// <summary>单位为像素px。</summary>
        public int FontSize { get; set; } = 24;
        public int FontIndex { get; set; } = 0;
        public UnicodeRange[] Chars { get; set; } = new[] { new UnicodeRange() { Start = (char)32, End = (char)126 } };
        public string[] CharsFiles { get; set; }
        public Size TextureSize { get; set; } = new Size() { Width = 1024, Height = 1024 };
        public DataFormat DataFormat { get; set; } = DataFormat.Xml;
        public Padding Padding { get; set; } = new Padding(0, 0, 0, 0);
        public Spacing Spacing { get; set; } = new Spacing(1, 1);
    }

    public struct UnicodeRange
    {
        public char Start;
        public char End;
    }

    public struct Size
    {
        public int Width;
        public int Height;

        public Size(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }

    public enum DataFormat
    {
        Text,
        Xml,
        Binary
    }

    public struct Padding
    {
        public int Up;
        public int Right;
        public int Down;
        public int Left;

        public Padding(int up, int right, int down, int left)
        {
            this.Up = up;
            this.Right = right;
            this.Down = down;
            this.Left = left;
        }
    }

    public struct Spacing
    {
        public int Horizontal;
        public int Vertical;

        public Spacing(int horizontal, int vertical)
        {
            this.Horizontal = horizontal;
            this.Vertical = vertical;
        }
    }
}
