using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.FontPatching;
using FontSettings.Framework.Fonts;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FontSettings.Framework
{
    internal class VanillaFontProvider : IVanillaFontProvider
    {
        /// <summary>记录不同语言下字体信息是否已经缓存了。</summary>
        private readonly IDictionary<Tuple<LanguageInfo, GameFontType>, bool> _cacheTable
            = new Dictionary<Tuple<LanguageInfo, GameFontType>, bool>();

        private readonly IDictionary<Tuple<LanguageInfo, GameFontType>, ISpriteFont> _cachedFonts
            = new Dictionary<Tuple<LanguageInfo, GameFontType>, ISpriteFont>();

        private readonly IDictionary<Tuple<LanguageInfo, GameFontType>, IEnumerable<CharacterRange>> _cachedCharacterRanges
            = new Dictionary<Tuple<LanguageInfo, GameFontType>, IEnumerable<CharacterRange>>();

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;

        private RecordInfo? _recording;

        public event EventHandler<RecordEventArgs> RecordStarted;

        public event EventHandler<RecordEventArgs> RecordFinished;

        public VanillaFontProvider(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            this._helper = helper;
            this._monitor = monitor;
            this._config = config;
        }

        /// <summary>This must be attached earlier than font patch logic.</summary>
        public void OnAssetRequested(AssetRequestedEventArgs e)
        {
            if (this.IsFontAsset(e, out GameFontType fontType, out LanguageInfo language)
                && !this.HasRecorded(language, fontType))
            {
                this._recording = new(e.Name, e.NameWithoutLocale, fontType, language);

                this.RaiseRecordStarted(language, fontType);
            }
        }

        /// <summary>This must be attached earlier than font patch logic.</summary>
        public void OnAssetReady(AssetReadyEventArgs e)
        {
            if (this._recording != null
                && this._recording.Name.IsEquivalentTo(e.Name)
                && this._recording.NameWithoutLocale.IsEquivalentTo(e.NameWithoutLocale))
            {
                var fontType = this._recording.FontType;
                var language = this._recording.Language;

                this._recording = null;

                switch (fontType)
                {
                    case GameFontType.SmallFont:
                        {
                            var smallFont = Game1.content.Load<SpriteFont>("Fonts/SmallFont");

                            this.RecordFont(language, fontType, new XNASpriteFont(smallFont));
                            this.RecordCharacterRanges(language, fontType, GetCharacterRanges(smallFont));
                        }
                        break;

                    case GameFontType.DialogueFont:
                        {
                            var dialogueFont = Game1.content.Load<SpriteFont>("Fonts/SpriteFont1");

                            this.RecordFont(language, fontType, new XNASpriteFont(dialogueFont));
                            this.RecordCharacterRanges(language, fontType, GetCharacterRanges(dialogueFont));
                        }
                        break;

                    case GameFontType.SpriteText:
                        {
                            string fontFileName = e.NameWithoutLocale.BaseName;
                            FontFile fontFile = FontLoader.Parse(Game1.content.Load<XmlSource>(fontFileName).Source);

                            var pages = new List<Texture2D>(fontFile.Pages.Count);
                            foreach (FontPage fontPage in fontFile.Pages)
                                pages.Add(Game1.content.Load<Texture2D>($"Fonts/{fontPage.File}"));

                            var bmFont = new GameBitmapSpriteFont(
                                bmFont: new Models.BmFontData(fontFile, pages.ToArray()),
                                pixelZoom: FontHelpers.GetDefaultFontPixelZoom(language),
                                language: language,
                                bmFontInLatinLanguages: () => this._config.EnableLatinDialogueFont);

                            this.RecordFont(language, fontType, bmFont);
                            this.RecordCharacterRanges(language, fontType, GetCharacterRanges(bmFont));
                        }
                        break;
                }

                this.SetHasRecorded(language, fontType);

                this.RaiseRecordFinished(language, fontType);

                _ = this._mainFontPatcher.PatchAsync(new FontContext(language, fontType));
            }
        }

        private bool IsFontAsset(AssetRequestedEventArgs e, out GameFontType fontType, out LanguageInfo language)
        {
            fontType = default;
            language = null;

            if (e.NameWithoutLocale.StartsWith("Fonts/"))
            {
                language = e.Name.LanguageCode != null
                    ? new LanguageInfo(e.Name.LanguageCode.Value, e.Name.LocaleCode)
                    : FontHelpers.LanguageEn;

                if (e.NameWithoutLocale.IsEquivalentTo("Fonts/SmallFont"))
                {
                    fontType = GameFontType.SmallFont;
                    return true;
                }
                else if (e.NameWithoutLocale.IsEquivalentTo("Fonts/SpriteFont1"))
                {
                    fontType = GameFontType.DialogueFont;
                    return true;
                }
                else if (this.IsBmFontFontFile(e.NameWithoutLocale.BaseName, ref language))
                {
                    fontType = GameFontType.SpriteText;
                    return true;
                }
            }

            return false;
        }

        private MainFontPatcher _mainFontPatcher;
        public void SetInvalidateHelper(MainFontPatcher mainFontPatcher)
        {
            this._mainFontPatcher = mainFontPatcher;
        }

        public bool HasRecorded(LanguageInfo language, GameFontType fontType)
        {
            return this.HasCached(language, fontType);
        }

        public void SetHasRecorded(LanguageInfo language, GameFontType fontType)
        {
            this.SetHasCached(language, fontType);
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

        private void RecordFont(LanguageInfo language, GameFontType fontType, ISpriteFont font)
        {
            this._cachedFonts[this.Key(language, fontType)] = font;
        }

        private void RecordCharacterRanges(LanguageInfo language, GameFontType fontType, IEnumerable<CharacterRange> ranges)
        {
            this._monitor.Log($"记录{language},{fontType}的字符集：{FontHelpers.GetCharactersCount(ranges)}个字符。");

            string RangeToString(CharacterRange range)
                => $"{range.Start}({(int)range.Start}) - {range.End}({(int)range.End})";
            this._monitor.VerboseLog(string.Join('\n', ranges.Select(r => RangeToString(r))));

            this._cachedCharacterRanges[this.Key(language, fontType)] = ranges;
        }

        private bool HasCached(LanguageInfo lang, GameFontType fontType)
        {
            var key = this.Key(lang, fontType);

            if (this._cacheTable.TryGetValue(key, out bool cached))
                return cached;

            // 如果不存在此键，创建并返回false。
            this._cacheTable.Add(key, false);
            return false;
        }

        private void SetHasCached(LanguageInfo lang, GameFontType fontType)
        {
            var key = this.Key(lang, fontType);

            if (this._cacheTable.ContainsKey(key))
                this._cacheTable[key] = true;
        }

        private Tuple<LanguageInfo, GameFontType> Key(LanguageInfo language, GameFontType fontType)
            => Tuple.Create(language, fontType);

        private void RaiseRecordStarted(LanguageInfo language, GameFontType fontType)
        {
            this.RaiseRecordStarted(
                new RecordEventArgs(language: language, fontType: fontType));
        }

        private void RaiseRecordFinished(LanguageInfo language, GameFontType fontType)
        {
            this.RaiseRecordFinished(
                new RecordEventArgs(language: language, fontType: fontType));
        }

        protected virtual void RaiseRecordStarted(RecordEventArgs e)
        {
            RecordStarted?.Invoke(this, e);
        }

        protected virtual void RaiseRecordFinished(RecordEventArgs e)
        {
            RecordFinished?.Invoke(this, e);
        }

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

        private static IEnumerable<CharacterRange> GetCharacterRanges(GameBitmapSpriteFont? font)
        {
            return FontHelpers.GetCharacterRanges(
                font?.CharacterMap.Select(pair => pair.Key));
        }

        private bool IsBmFontFontFile(string assetName, ref LanguageInfo language)
        {
            if (assetName == FontHelpers.GetFontFileAssetName(language))
                return true;

            var allLanguages = FontHelpers.GetAllAvailableLanguages();
            foreach (LanguageInfo lang in allLanguages)
            {
                if (assetName == FontHelpers.GetFontFileAssetName(lang))
                {
                    language = lang;
                    return true;
                }
            }

            return false;
        }

        record RecordInfo(IAssetName Name, IAssetName NameWithoutLocale, GameFontType FontType, LanguageInfo Language);
    }

    internal class RecordEventArgs : EventArgs
    {
        public LanguageInfo Language { get; }

        public GameFontType FontType { get; }

        public RecordEventArgs(LanguageInfo language, GameFontType fontType)
        {
            this.Language = language;
            this.FontType = fontType;
        }
    }
}
