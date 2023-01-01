using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace FontSettings.Framework
{
    internal static class ExtensionMethods
    {
        public static void Draw(this ClickableTextureComponent component, SpriteBatch b, Vector2 offset)
        {
            Draw(component, b, offset, Color.White, 0.86f + component.bounds.Y / 20000f);
        }

        public static void Draw(this ClickableTextureComponent component, SpriteBatch b, Vector2 offset, Color c)
        {
            Draw(component, b, offset, c, 0.86f + component.bounds.Y / 20000f);
        }

        public static void Draw(this ClickableTextureComponent component, SpriteBatch b, Vector2 offset, Color c, float layerDepth)
        {
            component.bounds.Location += offset.ToPoint();
            component.draw(b, c, layerDepth);
            component.bounds.Location -= offset.ToPoint();
        }

        public static FontFile DeepClone(this FontFile font)
        {
            return new FontFile()
            {
                Info = new FontInfo()
                {
                    Face = font.Info.Face,
                    Size = font.Info.Size,
                    Bold = font.Info.Bold,
                    Italic = font.Info.Italic,
                    CharSet = font.Info.CharSet,
                    Unicode = font.Info.Unicode,
                    StretchHeight = font.Info.StretchHeight,
                    Smooth = font.Info.Smooth,
                    SuperSampling = font.Info.SuperSampling,
                    Padding = font.Info.Padding,
                    Spacing = font.Info.Spacing,
                    OutLine = font.Info.OutLine
                },
                Common = new FontCommon()
                {
                    LineHeight = font.Common.LineHeight,
                    Base = font.Common.Base,
                    ScaleW = font.Common.ScaleW,
                    ScaleH = font.Common.ScaleH,
                    Pages = font.Common.Pages,
                    Packed = font.Common.Packed,
                    AlphaChannel = font.Common.AlphaChannel,
                    RedChannel = font.Common.RedChannel,
                    GreenChannel = font.Common.GreenChannel,
                    BlueChannel = font.Common.BlueChannel
                },
                Pages = new List<FontPage>(font.Pages.Select(p => new FontPage()
                {
                    ID = p.ID,
                    File = p.File
                })),
                Chars = new List<FontChar>(font.Chars.Select(c => new FontChar()
                {
                    ID = c.ID,
                    X = c.X,
                    Y = c.Y,
                    Width = c.Width,
                    Height = c.Height,
                    XOffset = c.XOffset,
                    YOffset = c.YOffset,
                    XAdvance = c.XAdvance,
                    Page = c.Page,
                    Channel = c.Channel
                })),
                Kernings = new List<FontKerning>(font.Kernings.Select(k => new FontKerning()
                {
                    First = k.First,
                    Second = k.Second,
                    Amount = k.Amount
                }))
            };
        }

        public static TEnum Next<TEnum>(this TEnum value)
            where TEnum : struct, Enum
        {
            var values = Enum.GetValues<TEnum>();
            int index = Array.IndexOf(values, value);

            int next = index + 1;
            if (next == values.Length)
                next = 0;
            return values[next];
        }
    }
}
