using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework
{
    internal class RuntimeFontManager : IDisposable
    {
        /// <summary>无需手动释放。</summary> 
        private readonly IDictionary<LanguageCode, Dictionary<GameFontType, SpriteFont>> _builtInSpriteFonts = new Dictionary<LanguageCode, Dictionary<GameFontType, SpriteFont>>();

        /// <summary>无需手动释放。</summary> 
        private readonly IDictionary<LanguageCode, GameBitmapSpriteFont> _builtInBmFonts = new Dictionary<LanguageCode, GameBitmapSpriteFont>();

        /// <summary>需要手动释放。</summary> 
        private readonly IDictionary<FontConfig, SpriteFont> _spriteFonts = new Dictionary<FontConfig, SpriteFont>();

        /// <summary>需要手动释放。</summary> 
        private readonly IList<GameBitmapSpriteFont> _bmFonts = new List<GameBitmapSpriteFont>();

        private readonly IModContentHelper _content;

        public RuntimeFontManager(IModContentHelper content)
        {
            this._content = content;
        }

        public SpriteFont GetLoadedFont(FontConfig config)
        {
            if (this._spriteFonts.TryGetValue(config, out SpriteFont spriteFont))
                return spriteFont;
            else
            {
                spriteFont = this._content.Load<SpriteFont>(config.ExistingFontPath);
                this._spriteFonts[config] = spriteFont;
                return spriteFont;
            }
        }

        public void RecordSpriteFont(FontConfig fontConfig, SpriteFont value)
        {
            if (value != null && !this._spriteFonts.ContainsKey(fontConfig))
                this._spriteFonts[fontConfig] = value;
        }

        public void RecordBmFont(GameBitmapSpriteFont value)
        {
            if (!this._bmFonts.Contains(value))
                this._bmFonts.Add(value);
        }

        public void RecordBuiltInSpriteFont()
        {
            this.RecordBuiltInSpriteFont((LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode);
        }

        public void RecordBuiltInBmFont()
        {
            this.RecordBuiltInBmFont((LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode);
        }

        public void RecordBuiltInBmFont(GameBitmapSpriteFont value)
        {
            this.RecordBuiltInBmFont((LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode, value);
        }

        public void RecordBuiltInSpriteFont(GameFontType fontType, SpriteFont value)
        {
            this.RecordBuiltInSpriteFont((LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode, fontType, value);
        }

        public void RecordBuiltInSpriteFont(LanguageCode code)
        {
            if (!this._builtInSpriteFonts.TryGetValue(code, out var dic))
                this._builtInSpriteFonts[code] = dic = new();
            if (!dic.ContainsKey(GameFontType.SmallFont))
                dic[GameFontType.SmallFont] = Game1.smallFont;
            if (!dic.ContainsKey(GameFontType.DialogueFont))
                dic[GameFontType.DialogueFont] = Game1.dialogueFont;
        }

        public void RecordBuiltInSpriteFont(LanguageCode code, GameFontType fontType, SpriteFont value)
        {
            if (!this._builtInSpriteFonts.TryGetValue(code, out var dic))
                this._builtInSpriteFonts[code] = dic = new();

            if (!dic.ContainsKey(fontType) && fontType != GameFontType.SpriteText)
                dic[fontType] = value;
        }

        public void RecordBuiltInBmFont(LanguageCode code)
        {
            if (!this._builtInBmFonts.ContainsKey(code) && !FontHelpers.IsLatinLanguage(code))
                this._builtInBmFonts[code] = new GameBitmapSpriteFont()
                {
                    FontFile = SpriteTextFields.FontFile,
                    Pages = SpriteTextFields.fontPages.ToList(),
                    CharacterMap = new(SpriteTextFields._characterMap),
                    LanguageCode = (LocalizedContentManager.LanguageCode)(int)code,
                    FontPixelZoom = SpriteText.fontPixelZoom
                };
        }

        public void RecordBuiltInBmFont(LanguageCode code, GameBitmapSpriteFont value)
        {
            if (!this._builtInBmFonts.ContainsKey(code) && !FontHelpers.IsLatinLanguage(code))
                this._builtInBmFonts[code] = value;
        }

        public GameBitmapSpriteFont GetBuiltInBmFont()
        {
            return this.GetBuiltInBmFont((LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode);
        }

        public SpriteFont GetBuiltInSpriteFont(GameFontType inGameType)
        {
            return this.GetBuiltInSpriteFont((LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode, inGameType);
        }

        public GameBitmapSpriteFont GetBuiltInBmFont(LanguageCode code)
        {
            return this._builtInBmFonts[code];
        }

        public SpriteFont GetBuiltInSpriteFont(LanguageCode code, GameFontType inGameType)
        {
            return this._builtInSpriteFonts[code][inGameType];
        }

        /// <summary>Cached font包括_cachedFonts和_builtInSpriteFonts两种。</summary>
        public bool IsCachedSpriteFont(SpriteFont spriteFont)
        {
            return this._spriteFonts.Values.Contains(spriteFont)
                || this.IsBuiltInFont(spriteFont);
        }

        public bool IsBuiltInSpriteFont(SpriteFont spriteFont)
        {
            var builtIn = this._builtInSpriteFonts.Values.SelectMany(dic => dic.Values);

            return builtIn.Contains(spriteFont)
                || builtIn.Any(font => object.ReferenceEquals(font.Texture, spriteFont?.Texture));
        }

        public bool IsBuiltInBmFont(ISpriteFont font)
        {
            if (font is GameBitmapSpriteFont bmFont)
            {
                return this._builtInBmFonts.Values.Contains(bmFont)
                    || this._builtInBmFonts.Values.Any(font =>
                    {
                        if (font.Pages is null) return false;
                        foreach (Texture2D page1 in font.Pages)
                            foreach (Texture2D page2 in bmFont.Pages)
                                if (object.ReferenceEquals(page1, page2))
                                    return true;
                        return false;
                    });
            }

            return false;
        }

        public void Dispose()
        {
            this.DisposeFonts();
        }

        /// <summary>清理所有需要清理的fonts，包括_cachedFonts和此时Game1里的几个fonts（如果没有contentmanager保存过）</summary>
        private void DisposeFonts()
        {
            // 目前是用smapi的contenthelper加载的，因此它会自动帮你清理。
            //foreach (SpriteFont font in this._cachedFonts.Values)
            //    font.Texture.Dispose();
            this._spriteFonts.Clear();

            if (!this.IsBuiltInFont(Game1.smallFont)) Game1.smallFont.Texture.Dispose();
            if (!this.IsBuiltInFont(Game1.dialogueFont)) Game1.dialogueFont.Texture.Dispose();

            // Bmfonts。
            foreach (Texture2D texture in this._bmFonts.SelectMany(bmFont => bmFont.Pages))
                texture.Dispose();
            this._bmFonts.Clear();
        }

        private bool IsBuiltInFont(SpriteFont spriteFont)
        {
            var builtIn = this._builtInSpriteFonts.Values.SelectMany(dic => dic.Values);

            return builtIn.Contains(spriteFont)
                || builtIn.Any(font => object.ReferenceEquals(font.Texture, spriteFont?.Texture));
        }
    }
}
