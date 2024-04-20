using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework.Fonts
{
    internal class GameBitmapSpriteFont : SpriteFontBase
    {
        private readonly SpriteTextObject _spriteText;

        public FontFile FontFile { get; }

        public List<Texture2D> Pages { get; } = new();

        public float FontPixelZoom { get; }

        public Dictionary<char, FontChar> CharacterMap { get; } = new();

        protected override string LineBreak => SpriteText.newLine.ToString();

        public GameBitmapSpriteFont(BmFontData? bmFont, float pixelZoom, LanguageInfo language, Func<bool> bmFontInLatinLanguages)
        {
            this._spriteText = new SpriteTextObject(bmFont, pixelZoom, language, bmFontInLatinLanguages);

            if (bmFont != null)
            {
                this.FontFile = bmFont.FontFile;
                this.Pages = new(bmFont.Pages);
                this.CharacterMap = bmFont.FontFile.Chars.ToDictionary(fontChar => (char)fontChar.ID);
            }
            this.FontPixelZoom = pixelZoom;
        }

        public override void Draw(SpriteBatch b, string text, Vector2 position, Color color)
        {
            this._spriteText.drawString(b, text, (int)position.X, (int)position.Y, color: color);
        }

        public override void DrawBounds(SpriteBatch b, string text, Vector2 position, Color color)
        {
            if (string.IsNullOrEmpty(text)) return;

            Vector2 offset = Vector2.Zero;
            foreach (char c in text)
            {
                switch (c)
                {
                    case '^':
                        offset.X = 0;
                        offset.Y += this.FontFile.Common.LineHeight * this.FontPixelZoom;
                        continue;
                }

                if (this.CharacterMap.TryGetValue(c, out FontChar fontChar))
                {
                    var p = offset;
                    p.X += fontChar.XOffset * this.FontPixelZoom;
                    p.Y += fontChar.YOffset * this.FontPixelZoom;
                    p += position;

                    b.Draw(
                        texture: Game1.staminaRect,
                        destinationRectangle: new Rectangle((int)p.X, (int)p.Y, (int)(fontChar.Width * this.FontPixelZoom), (int)(fontChar.Height * this.FontPixelZoom)),
                        sourceRectangle: null,
                        color: color,
                        rotation: 0f,
                        origin: Vector2.Zero,
                        effects: SpriteEffects.None,
                        layerDepth: 0f
                    );
                    offset.X += fontChar.XAdvance * this.FontPixelZoom;  // 没有LeftBearing、RightBearing这些东西，字间距Spacing已算在XAdvance里面。
                }
            }
        }

        public override Vector2 MeasureString(string text)
        {
            return new Vector2(
                this._spriteText.getWidthOfString(text),
                this._spriteText.getHeightOfString(text));
        }

        /// <summary>游戏内置的字体放大倍数。</summary>
        public static float GetDefaultFontPixelZoom(LocalizedContentManager.LanguageCode code)
        {
            switch (code)
            {
                case LocalizedContentManager.LanguageCode.ja: return 1.75f;
                case LocalizedContentManager.LanguageCode.ru: return 3f;
                case LocalizedContentManager.LanguageCode.zh: return 1.5f;
                case LocalizedContentManager.LanguageCode.th: return 1.5f;
                case LocalizedContentManager.LanguageCode.ko: return 1.5f;
                case LocalizedContentManager.LanguageCode.mod: return LocalizedContentManager.CurrentModLanguage.FontPixelZoom;
                default: return 3f;
            }
        }
    }
}
