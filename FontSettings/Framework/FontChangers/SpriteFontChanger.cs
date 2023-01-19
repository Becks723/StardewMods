using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontInfomation;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FontSettings.Framework.FontChangers
{
    internal abstract class SpriteFontChanger : BaseGameFontChanger
    {
        private readonly object _lock = new object();

        private readonly ModConfig _config;
        private readonly IGameContentHelper _gameContent;

        private readonly Func<LanguageInfo, GameFontType, string> _getVanillaFontFile;

        private FontData _data;

        protected abstract string AssetName { get; }

        public SpriteFontChanger(IModHelper helper, ModConfig config, Func<LanguageInfo, GameFontType, string> getVanillaFontFile)
        {
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            this._config = config;
            this._getVanillaFontFile = getVanillaFontFile;
            this._gameContent = helper.GameContent;
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo(this.AssetName))
            {
                var data = this._data;
                if (data != null)
                {
                    var config = data.Config;
                    switch (data.EditMode)
                    {
                        case EditMode.DoNothing:
                            break;

                        case EditMode.Edit:
                            e.Edit(asset => EditSpriteFont(asset.GetData<SpriteFont>(), data.Config));
                            break;

                        case EditMode.Replace:
                            e.LoadFrom(() => data.Font, AssetLoadPriority.High);
                            break;
                    }
                }
            }
        }

        //public override bool ChangeGameFont(FontConfig font)
        //{
        //    lock (this._lock)
        //    {
        //        var lastData = this._data;
        //        try
        //        {
        //            bool success;
        //            try
        //            {
        //                this._data = ResolveFontConfig(font);
        //                success = true;
        //            }
        //            catch (Exception ex)
        //            {
        //                this._data = null;
        //                success = false;
        //                throw;
        //            }

        //            if (success)
        //            {
        //                this._gameContent.InvalidateCache(this.AssetName);
        //            }

        //            return success;
        //        }
        //        finally
        //        {
        //            lastData?.Dispose();
        //        }
        //    }
        //}

        public override IGameFontChangeResult ChangeGameFont(FontConfig font)
        {
            try
            {
                bool success;
                try
                {
                    this._data = this.ResolveFontConfig(font);
                    success = true;
                }
                catch (Exception ex)
                {
                    this._data = null;
                    success = false;
                    throw;
                }

                if (success)
                {
                    this.InvalidateAndPropagate();
                }

                return this.GetSuccessResult();
            }
            catch (Exception ex)
            {
                return this.GetErrorResult(ex);
            }
            finally
            {
                this._data = null;
            }
        }

        //public override async Task<bool> ChangeGameFontAsync(FontConfig font)
        //{
        //    var lastData = this._data;

        //    bool success = await Task.Run(() =>
        //    {
        //        lock (this._lock)
        //        {
        //            try
        //            {
        //                this._data = ResolveFontConfig(font);
        //                return true;
        //            }
        //            catch (Exception ex)
        //            {
        //                this._data = null;
        //                return false;
        //            }
        //        }
        //    });

        //    if (success)
        //    {
        //        this._gameContent.InvalidateCache(this.AssetName);

        //        lastData?.Dispose();
        //    }

        //    return success;
        //}

        public override async Task<IGameFontChangeResult> ChangeGameFontAsync(FontConfig font)
        {
            try
            {
                bool success = await Task.Run(() =>
                {
                    try
                    {
                        this._data = this.ResolveFontConfig(font);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        this._data = null;
                        return false;
                    }
                });

                if (success)
                {
                    this.InvalidateAndPropagate();
                }

                return this.GetSuccessResult();
            }
            catch (Exception ex)
            {
                return this.GetErrorResult(ex);
            }
            finally
            {
                this._data = null;
            }
        }

        private void InvalidateAndPropagate()
        {
            if (!this._gameContent.InvalidateCache(this.AssetName))
                this._gameContent.Load<SpriteFont>(this.AssetName);  // 如果没有缓存过，需要手动加载一次以修改。
        }

        FontData ResolveFontConfig(FontConfig config)
        {
            if (!config.Enabled)
                return new FontData(config, EditMode.DoNothing, null);

            FontConfig copy = new FontConfig();
            config.CopyTo(copy);

            bool isAbsolutePath = false;
            if (copy.FontFilePath == null)
            {
                copy.FontFilePath = this.GetVanillaFontFile(copy);
                if (copy.FontFilePath != null)
                    isAbsolutePath = true;
            }

            if (copy.FontFilePath == null)
                return new FontData(config, EditMode.Edit, null);

            return new FontData(config, EditMode.Replace, CreateSpriteFont(copy, isAbsolutePath));
        }

        static void EditSpriteFont(SpriteFont existingFont, FontConfig config)
        {
            SpriteFontGenerator.EditExisting(
                existingFont: existingFont,
                overridePixelHeight: config.FontSize,
                overrideCharRange: config.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(config.GetLanguage()),
                overrideSpacing: config.Spacing,
                overrideLineSpacing: config.LineSpacing,
                extraCharOffsetX: config.CharOffsetX,
                extraCharOffsetY: config.CharOffsetY);
        }

        static SpriteFont CreateSpriteFont(FontConfig config, bool isAbsolutePath)
        {
            string path = isAbsolutePath
                ? config.FontFilePath
                : InstalledFonts.GetFullPath(config.FontFilePath);
            return SpriteFontGenerator.FromTtf(
                ttfPath: path,
                fontIndex: config.FontIndex,
                fontPixelHeight: config.FontSize,
                characterRanges: config.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(config.GetLanguage()),
                spacing: config.Spacing,
                lineSpacing: config.LineSpacing,
                charOffsetX: config.CharOffsetX,
                charOffsetY: config.CharOffsetY);
        }

        private string GetVanillaFontFile(FontConfig font)
        {
            return this._getVanillaFontFile(new LanguageInfo(font.Lang, font.Locale), font.InGameType);
        }

        private record FontData(FontConfig Config, EditMode EditMode, SpriteFont Font) : IDisposable
        {
            public void Dispose()
            {
                this.Font?.Texture?.Dispose();
            }
        }

        private enum EditMode
        {
            DoNothing,
            Edit,
            Replace
        }
    }
}
