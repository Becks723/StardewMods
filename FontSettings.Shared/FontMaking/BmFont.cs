using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BmFont;
using FontSettings.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;  // Game1.graphics.GraphicsDevice
using static StbTrueTypeSharp.StbTrueType;

namespace BmFontCS
{
    public unsafe class BmFont
    {
        private stbtt_fontinfo _fontInfo;
        private float _scale;
        private float _ascender, _descender, _lineHeight;

        public void GenerateIntoMemory(string fontFilePath, out FontFile fontFile, out Texture2D[] pages, BmFontSettings settings)
        {
            this.GenerateIntoMemory(fontFilePath, out fontFile, out Page[] pgs, settings);

            pages = pgs
                .Select(p => MakeFontUtils.GenerateTexture2D(p.Buffer, p.Width, p.Height))
                .ToArray();
        }

        public void GenerateIntoMemory(string fontFilePath, out FontFile fontFile, out byte[][] pages, BmFontSettings settings)
        {
            this.GenerateIntoMemory(fontFilePath, out fontFile, out Page[] pgs, settings);

            pages = pgs.Select(p => p.Buffer).ToArray();
        }

        private void GenerateIntoMemory(string fontFilePath, out FontFile fontFile, out Page[] pages, BmFontSettings settings)
        {
            byte[] ttf = File.ReadAllBytes(fontFilePath);

            int offset;
            fixed (byte* ttfPtr = ttf)
            {
                offset = stbtt_GetFontOffsetForIndex(ttfPtr, settings.FontIndex);
                if (offset == -1)
                    throw new IndexOutOfRangeException($"字体索引超出范围。索引值：{settings.FontIndex}");
            }

            this._fontInfo = CreateFont(ttf, offset);
            if (this._fontInfo == null)
                throw new Exception("无法初始化字体。");
            try
            {
                this.InitFields(settings);
                var codepoints = GetChars(settings.Chars, settings.CharsFiles);
                Glyph[] glyphs = this.CollectGlyphs(codepoints).ToArray();
                Size[] pageSizes = ArrangeGlyphs(glyphs, settings);
                Page[] pgs = this.RenderGlyphsToPages(glyphs, pageSizes, settings);

                fontFile = this.GetFontInfo(fontFilePath, settings, pgs, glyphs, settings.Spacing.Horizontal);
                pages = pgs;
            }
            finally
            {
                this._fontInfo.Dispose();
            }
        }

        public void Generate(string fontFile, string output, BmFontSettings settings)
        {
            byte[] ttf = File.ReadAllBytes(fontFile);

            this._fontInfo = new stbtt_fontinfo();

            fixed (byte* ttfPtr = ttf)
            {
                int offset = stbtt_GetFontOffsetForIndex(ttfPtr, settings.FontIndex);
                if (offset == -1)
                    throw new IndexOutOfRangeException($"字体索引超出范围。索引值：{settings.FontIndex}");
                if (stbtt_InitFont(this._fontInfo, ttfPtr, offset) == 0)
                    throw new Exception("无法初始化字体。");
            }

            this.InitFields(settings);
            var codepoints = GetChars(settings.Chars, settings.CharsFiles);
            Glyph[] glyphs = this.CollectGlyphs(codepoints).ToArray();
            Size[] pageSizes = ArrangeGlyphs(glyphs, settings);
            Page[] pages = this.RenderGlyphsToPages(glyphs, pageSizes, settings);
            SaveToFiles(output, pages);
        }

        private static void SaveToFiles(string output, Page[] pages)
        {

        }

        private FontFile GetFontInfo(string fontFilePath, BmFontSettings settings, Page[] pages, Glyph[] glyphs, int spacing)
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
                    LineHeight = (int)Math.Round(this._lineHeight),
                    Base = (int)Math.Round(this._ascender),
                    ScaleW = settings.TextureSize.Width,
                    ScaleH = settings.TextureSize.Height,
                    Pages = pages.Length,
                    Packed = 0,         // TODO
                    AlphaChannel = 0,   // TODO
                    RedChannel = 4,     // TODO
                    GreenChannel = 4,   // TODO
                    BlueChannel = 4,    // TODO
                },
                Pages = new List<FontPage>(pages.Select(page => new FontPage
                {
                    ID = page.Id,
                    File = page.Name
                })),
                Chars = new List<FontChar>(glyphs.Select(glyph => new FontChar
                {
                    ID = glyph.Id,
                    X = glyph.X,
                    Y = glyph.Y,
                    Width = glyph.Width,
                    Height = glyph.Height,
                    XOffset = glyph.XOffset,
                    YOffset = glyph.YOffset,
                    XAdvance = glyph.XAdvance + spacing,
                    Page = glyph.Page,
                    Channel = 15
                })),
                Kernings = new List<FontKerning>() // TODO
            };
        }

        private Page[] RenderGlyphsToPages(Glyph[] glyphs, Size[] pages, BmFontSettings settings)
        {
            List<Page> result = new(pages.Length);

            for (int i = 0; i < pages.Length; i++)
            {
                byte[] buffer = new byte[pages[i].Width * pages[i].Height];
                fixed (byte* bufferPtr = buffer)
                    foreach (Glyph glyph in glyphs)
                    {
                        if (glyph.Page != i)
                            continue;

                        stbtt_MakeGlyphBitmapSubpixel(this._fontInfo,
                            bufferPtr + glyph.X + glyph.Y * pages[i].Width,
                            glyph.Width, glyph.Height, pages[i].Width,
                            this._scale, this._scale,
                            0, 0,
                            glyph.GlyphIndex);
                    }

                result.Add(new Page
                {
                    Width = pages[i].Width,
                    Height = pages[i].Height,
                    Buffer = buffer,
                    Id = i,
                    Name = $"{settings.Name}_{i}"
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

        private void InitFields(BmFontSettings settings)
        {
            this._scale = stbtt_ScaleForPixelHeight(this._fontInfo, settings.FontSize);

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(this._fontInfo, &ascent, &descent, &lineGap);
            if (ascent == 0 && descent == 0)
                stbtt_GetFontVMetricsOS2(this._fontInfo, &ascent, &descent, &lineGap);
            this._ascender = ascent * this._scale;
            this._descender = descent * this._scale;
            this._lineHeight = (ascent - descent + lineGap) * this._scale;
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

        private HashSet<Glyph> CollectGlyphs(HashSet<int> codepoints)
        {
            HashSet<Glyph> result = new();

            int x0 = 0,
                y0 = 0,
                x1 = 0,
                y1 = 0,
                advanceWidth = 0;
            foreach (int codepoint in codepoints)
            {
                int glyphIndex = stbtt_FindGlyphIndex(this._fontInfo, codepoint);
                if (glyphIndex != 0)  // 未定义 TODO: 警告
                {
                    stbtt_GetGlyphBitmapBox(this._fontInfo, glyphIndex, this._scale, this._scale, &x0, &y0, &x1, &y1);
                    stbtt_GetGlyphHMetrics(this._fontInfo, glyphIndex, &advanceWidth, null);

                    Glyph glyph = new Glyph();
                    glyph.Id = codepoint;
                    glyph.GlyphIndex = glyphIndex;
                    glyph.Width = x1 - x0;
                    glyph.Height = y1 - y0;
                    glyph.XAdvance = (int)Math.Ceiling(advanceWidth * this._scale);
                    glyph.XOffset = x0;
                    glyph.YOffset = (int)Math.Floor(y0 + this._ascender);

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
            public string Name;
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
        /// <summary>Name of the font file (.fnt), if exported. Also base name of pages.</summary>
        public string Name { get; set; } = Guid.NewGuid().ToString().Substring(0, 8);
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
