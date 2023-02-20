using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;

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

        /// <summary>Needs this for BmFont file name.</summary>
        private List<ModLanguage> _modLanguages;

        public event EventHandler<RecordEventArgs> RecordStarted;

        public event EventHandler<RecordEventArgs> RecordFinished;

        public VanillaFontProvider(IModHelper helper, IMonitor monitor)
        {
            this._helper = helper;
            this._monitor = monitor;
        }

        /// <summary>This must be attached earlier than font patch logic.</summary>
        public void OnAssetRequested(AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.StartsWith("Fonts/")) return;

            var language = e.Name.LanguageCode != null
                ? new LanguageInfo(e.Name.LanguageCode.Value, e.Name.LocaleCode)
                : FontHelpers.LanguageEn;
            GameFontType? fontTypeNullable = 0 switch
            {
                _ when e.NameWithoutLocale.IsEquivalentTo("Fonts/SmallFont") => GameFontType.SmallFont,
                _ when e.NameWithoutLocale.IsEquivalentTo("Fonts/SpriteFont1") => GameFontType.DialogueFont,
                _ when this.IsBmFontFontFile(e.NameWithoutLocale.BaseName, ref language) => GameFontType.SpriteText,
                _ => null
            };

            if (fontTypeNullable != null
                && !this.HasRecorded(language, fontTypeNullable.Value))
            {
                var fontType = fontTypeNullable.Value;

                this.RaiseRecordStarted(language, fontType);
            }
        }

        /// <summary>This must be attached earlier than font patch logic.</summary>
        public void OnAssetReady(AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/AdditionalLanguages"))
                this._modLanguages = Game1.content.Load<List<ModLanguage>>("Data/AdditionalLanguages");

            if (!e.NameWithoutLocale.StartsWith("Fonts/")) return;

            var language = e.Name.LanguageCode != null
                ? new LanguageInfo(e.Name.LanguageCode.Value, e.Name.LocaleCode)
                : FontHelpers.LanguageEn;
            GameFontType? fontTypeNullable = 0 switch
            {
                _ when e.NameWithoutLocale.IsEquivalentTo("Fonts/SmallFont") => GameFontType.SmallFont,
                _ when e.NameWithoutLocale.IsEquivalentTo("Fonts/SpriteFont1") => GameFontType.DialogueFont,
                _ when this.IsBmFontFontFile(e.NameWithoutLocale.BaseName, ref language) => GameFontType.SpriteText,
                _ => null
            };

            if (fontTypeNullable != null
                && !this.HasRecorded(language, fontTypeNullable.Value))
            {
                var fontType = fontTypeNullable.Value;
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

                            var bmFont = new GameBitmapSpriteFont()
                            {
                                FontFile = fontFile,
                                Pages = pages,
                                LanguageCode = language.Code
                            };

                            this.RecordFont(language, fontType, bmFont);
                            this.RecordCharacterRanges(language, fontType, GetCharacterRanges(bmFont));
                        }
                        break;
                }

                this.SetHasRecorded(language, fontType);

                this.RaiseRecordFinished(language, fontType);

                this._invalidateContextList.Add(new(language, fontType, fontType == GameFontType.SpriteText
                        ? e.NameWithoutLocale.BaseName
                        : null));
            }
        }

        private readonly IList<InvalidateContext> _invalidateContextList = new List<InvalidateContext>();
        public void OnUpdateTicking(UpdateTickingEventArgs e)
        {
            try
            {
                foreach (var context in this._invalidateContextList)
                {
                    if (context != null)
                        this.InvalidateTargetFont(context.Language, context.FontType, context.FontFile);
                }
            }
            finally
            {
                this._invalidateContextList.Clear();
            }
        }

        private record InvalidateContext(LanguageInfo Language, GameFontType FontType, string? FontFile = null);

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
            this._cachedCharacterRanges[this.Key(language, fontType)] = ranges;
        }

        private void RecordCharacterRanges(LanguageInfo language, GameFontType fontType, SpriteFont font)
            => this.RecordCharacterRanges(language, fontType, GetCharacterRanges(font));

        private void RecordCharacterRanges(LanguageInfo language, GameFontType fontType, GameBitmapSpriteFont font)
            => this.RecordCharacterRanges(language, fontType, GetCharacterRanges(font));

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
            // `_modLanguages` may be not available at this point.

            // game lang
            foreach (var code in Enum.GetValues<LocalizedContentManager.LanguageCode>())
            {
                if (code == LocalizedContentManager.LanguageCode.th
                    || code == LocalizedContentManager.LanguageCode.mod) continue;

                if (assetName == FontHelpers.GetFontFileAssetName(code))
                {
                    language = FontHelpers.GetLanguage(code);
                    return true;
                }
            }

            // mod lang
            var modLanguage = this._modLanguages
                ?.Where(lang => lang.FontFile == assetName)
                .FirstOrDefault();
            if (modLanguage != null)
            {
                language = FontHelpers.GetModLanguage(modLanguage);
                return true;
            }

            return false;
        }

        private void InvalidateTargetFont(LanguageInfo language, GameFontType fontType, string? fontFile = null)
        {
            switch (fontType)
            {
                case GameFontType.SmallFont:
                    this._helper.GameContent.InvalidateCache(
                        FontHelpers.LocalizeAssetName("Fonts/SmallFont", language));
                    break;

                case GameFontType.DialogueFont:
                    this._helper.GameContent.InvalidateCache(
                        FontHelpers.LocalizeAssetName("Fonts/SpriteFont1", language));
                    break;

                case GameFontType.SpriteText:
                    if (fontFile != null)
                        this.InvalidateBmFontFile(language, fontFile);
                    break;
            }
        }

        private void InvalidateBmFontFile(LanguageInfo language, string fontFileName)
        {
            FontFile fontFile = this.LoadFontFile(fontFileName);

            this._helper.GameContent.InvalidateCache(fontFileName);
            this._helper.GameContent.InvalidateCache(
                FontHelpers.LocalizeAssetName(fontFileName, language));

            if (fontFile != null)
            {
                foreach (FontPage page in fontFile.Pages)
                {
                    string pageName = $"Fonts/{page.File}";

                    this._helper.GameContent.InvalidateCache(pageName);
                    this._helper.GameContent.InvalidateCache(
                        FontHelpers.LocalizeAssetName(pageName, language));
                }
            }

            // propagate
            {
                // fontFile
                fontFile = this.LoadFontFile(fontFileName);

                // characterMap
                var characterMap = new Dictionary<char, FontChar>();
                foreach (FontChar current in fontFile.Chars)
                {
                    char key = (char)current.ID;
                    characterMap.Add(key, current);
                }

                // fontPages
                var fontPages = new List<Texture2D>();
                foreach (FontPage current2 in fontFile.Pages)
                {
                    fontPages.Add(Game1.content.Load<Texture2D>("Fonts\\" + current2.File));
                }

                SpriteTextFields.FontFile = fontFile;
                SpriteTextFields._characterMap = characterMap;
                SpriteTextFields.fontPages = fontPages;
            }
        }

        private FontFile LoadFontFile(string fontFile)
        {
            return FontLoader.Parse(Game1.content.Load<XmlSource>(fontFile).Source);
        }
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
