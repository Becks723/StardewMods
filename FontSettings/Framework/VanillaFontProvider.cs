using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FontSettings.Framework
{
    internal class VanillaFontProvider : IVanillaFontProvider
    {
        private readonly Lazy<ContentManager> _contentManager = new(
            () => new ContentManager(
                GameRunner.instance.Content.ServiceProvider,
                GameRunner.instance.Content.RootDirectory));

        /// <summary>记录不同语言下字体信息是否已经缓存了。</summary>
        private readonly IDictionary<LanguageInfo, bool> _cacheTable = new Dictionary<LanguageInfo, bool>();

        private readonly IDictionary<Tuple<LanguageInfo, GameFontType>, ISpriteFont> _cachedFonts
            = new Dictionary<Tuple<LanguageInfo, GameFontType>, ISpriteFont>();

        private readonly IDictionary<Tuple<LanguageInfo, GameFontType>, IEnumerable<CharacterRange>> _cachedCharacterRanges
            = new Dictionary<Tuple<LanguageInfo, GameFontType>, IEnumerable<CharacterRange>>();

        private readonly IMonitor _monitor;

        public event EventHandler Bypass;

        public VanillaFontProvider(IMonitor monitor)
        {
            this._monitor = monitor;
        }

        public void OnAssetRequested(AssetRequestedEventArgs e)
        {
        }

        public void OnAssetReady(AssetReadyEventArgs e)
        {
        }

        public void RecordForVanillaLangauges()
        {
            foreach (var code in Enum.GetValues<LocalizedContentManager.LanguageCode>())
            {
                if (code == LocalizedContentManager.LanguageCode.th
                 || code == LocalizedContentManager.LanguageCode.mod)
                    continue;

                this.RecordVanillaFontData(code, FontHelpers.GetLocale(code));
            }
        }

        public IEnumerable<CharacterRange> GetVanillaCharacterRanges(LanguageInfo language, GameFontType fontType)
        {
            if (this._cachedCharacterRanges.TryGetValue(this.Key(language, fontType), out IEnumerable<CharacterRange> ranges))
                return ranges;

            if (this._cachedCharacterRanges.TryGetValue(this.Key(FontHelpers.LanguageEn, fontType), out ranges))
                return ranges;

            throw new KeyNotFoundException();
        }

        public ISpriteFont GetVanillaFont(LanguageInfo language, GameFontType fontType)
        {
            if (this._cachedFonts.TryGetValue(this.Key(language, fontType), out ISpriteFont font))
                return font;

            if (this._cachedFonts.TryGetValue(this.Key(FontHelpers.LanguageEn, fontType), out font))
                return font;

            throw new KeyNotFoundException();
        }

        private void RecordVanillaFontData(LocalizedContentManager.LanguageCode languageCode, string locale)
        {
            string DisplayLanguage()
            {
                if (languageCode is LocalizedContentManager.LanguageCode.en && locale == string.Empty)
                    return "en";
                return locale;
            }

            LanguageInfo langInfo = new LanguageInfo(languageCode, locale);
            string langStr = DisplayLanguage();

            if (this.HasCached(langInfo))
            {
                this._monitor.Log($"无需记录{langStr}语言下的游戏字体数据！");
            }
            else
            {
                this._monitor.Log($"正在记录{langStr}语言下的游戏字体数据……");

                var contentManager = this._contentManager.Value;
                SpriteFont smallFont;
                try
                {
                    smallFont = contentManager.Load<SpriteFont>(FontHelpers.LocalizeAssetName("Fonts/SmallFont", languageCode, locale));
                }
                catch (ContentLoadException)
                {
                    smallFont = contentManager.Load<SpriteFont>("Fonts/SmallFont");
                }

                SpriteFont dialogueFont;
                try
                {
                    dialogueFont = contentManager.Load<SpriteFont>(FontHelpers.LocalizeAssetName("Fonts/SpriteFont1", languageCode, locale));
                }
                catch (ContentLoadException)
                {
                    dialogueFont = contentManager.Load<SpriteFont>("Fonts/SpriteFont1");
                }

                GameBitmapSpriteFont spriteText = this.LoadGameBmFont(languageCode);

                // 记录内置字体。
                this.RecordFont(langInfo, GameFontType.SmallFont, new XNASpriteFont(smallFont));
                this.RecordFont(langInfo, GameFontType.DialogueFont, new XNASpriteFont(dialogueFont));
                this.RecordFont(langInfo, GameFontType.SpriteText, spriteText);

                // 记录字符范围，加载字体要用。
                this.RecordCharacterRanges(langInfo, GameFontType.SmallFont, smallFont);
                this.RecordCharacterRanges(langInfo, GameFontType.DialogueFont, dialogueFont);
                this.RecordCharacterRanges(langInfo, GameFontType.SpriteText, spriteText);

                this._monitor.Log($"已完成记录{langStr}语言下的游戏字体数据！");
                this.SetHasCached(langInfo);
            }
        }

        private GameBitmapSpriteFont LoadGameBmFont(LocalizedContentManager.LanguageCode languageCode)
        {
            string fntPath = languageCode switch
            {
                LocalizedContentManager.LanguageCode.ja => "Fonts/Japanese",
                LocalizedContentManager.LanguageCode.ru => "Fonts/Russian",
                LocalizedContentManager.LanguageCode.zh => "Fonts/Chinese",
                LocalizedContentManager.LanguageCode.ko => "Fonts/Korean",
                _ => null
            };

            if (fntPath != null)
            {
                var contentManager = this._contentManager.Value;

                FontFile fontFile = FontLoader.Parse(contentManager.Load<XmlSource>(fntPath).Source);
                List<Texture2D> pages = new List<Texture2D>(fontFile.Pages.Count);
                foreach (FontPage fontPage in fontFile.Pages)
                {
                    string assetName = $"Fonts/{fontPage.File}";
                    pages.Add(contentManager.Load<Texture2D>(assetName));
                }

                return new GameBitmapSpriteFont()
                {
                    FontFile = fontFile,
                    Pages = pages,
                    LanguageCode = languageCode
                };
            }

            return null;
        }

        private void RecordFont(LanguageInfo language, GameFontType fontType, ISpriteFont font)
        {
            this._cachedFonts[this.Key(language, fontType)] = font;
        }

        private void RecordCharacterRanges(LanguageInfo language, GameFontType fontType, IEnumerable<CharacterRange> ranges)
        {
            this._cachedCharacterRanges[this.Key(language, fontType)] = ranges;
        }

        private void RecordCharacterRanges(LanguageInfo language, GameFontType fontType, SpriteFont font)
            => this.RecordCharacterRanges(language, fontType, GetCharacterRanges(font));

        private void RecordCharacterRanges(LanguageInfo language, GameFontType fontType, GameBitmapSpriteFont font)
            => this.RecordCharacterRanges(language, fontType, GetCharacterRanges(font));

        private bool HasCached(LanguageInfo lang)
        {
            // 如果不存在此键，创建并返回false。
            if (this._cacheTable.TryGetValue(lang, out bool cached))
                return cached;

            this._cacheTable.Add(lang, false);
            return false;
        }

        private void SetHasCached(LanguageInfo lang)
        {
            if (this._cacheTable.ContainsKey(lang))
                this._cacheTable[lang] = true;
        }

        private Tuple<LanguageInfo, GameFontType> Key(LanguageInfo language, GameFontType fontType)
            => Tuple.Create(language, fontType);

        private static IEnumerable<CharacterRange> GetCharacterRanges(SpriteFont font)
        {
            BindingFlags nonPublic = BindingFlags.Instance | BindingFlags.NonPublic;
            BindingFlags isPublic = BindingFlags.Instance | BindingFlags.Public;

            var regions = typeof(SpriteFont).GetField("_regions", nonPublic).GetValue(font) as Array;
            FieldInfo startField = null;
            FieldInfo endField = null;

            for (int i = 0; i < regions.Length; i++)
            {
                object region = regions.GetValue(i);
                if (i == 0)
                {
                    startField = region.GetType().GetField("Start", isPublic);
                    endField = region.GetType().GetField("End", isPublic);
                }
                char start = (char)startField.GetValue(region);
                char end = (char)endField.GetValue(region);
                yield return new CharacterRange(start, end);
            }
        }

        private static IEnumerable<CharacterRange> GetCharacterRanges(GameBitmapSpriteFont font)
        {
            return FontHelpers.GetCharacterRanges(
                font.CharacterMap.Select(pair => pair.Key));
        }
    }
}
