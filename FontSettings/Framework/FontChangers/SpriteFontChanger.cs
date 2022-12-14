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

        private FontData _data;

        protected abstract string AssetName { get; }

        public SpriteFontChanger(IModHelper helper, ModConfig config)
        {
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            this._config = config;
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

        public override bool ChangeGameFont(FontConfig font)
        {
            try
            {
                bool success;
                try
                {
                    this._data = ResolveFontConfig(font);
                    success = true;
                }
                catch (Exception ex)
                {
                    this._data = null;
                    success = false;
                }

                if (success)
                {
                    if (!this._gameContent.InvalidateCache(this.AssetName))
                        this._gameContent.Load<SpriteFont>(this.AssetName);
                }

                return success;
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

        public override async Task<bool> ChangeGameFontAsync(FontConfig font)
        {
            try
            {
                bool success = await Task.Run(() =>
                {
                    try
                    {
                        this._data = ResolveFontConfig(font);
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
                    if (!this._gameContent.InvalidateCache(this.AssetName))
                        this._gameContent.Load<SpriteFont>(this.AssetName);
                }

                return success;
            }
            finally
            {
                this._data = null;
            }
        }

        static FontData ResolveFontConfig(FontConfig config)
        {
            if (!config.Enabled)
                return new FontData(config, EditMode.DoNothing, null);

            if (config.FontFilePath is null)
                return new FontData(config, EditMode.Edit, null);

            return new FontData(config, EditMode.Replace, CreateSpriteFont(config));
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

        static SpriteFont CreateSpriteFont(FontConfig config)
        {
            return SpriteFontGenerator.FromTtf(
                ttfPath: InstalledFonts.GetFullPath(config.FontFilePath),
                fontIndex: config.FontIndex,
                fontPixelHeight: config.FontSize,
                characterRanges: config.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(config.GetLanguage()),
                spacing: config.Spacing,
                lineSpacing: config.LineSpacing,
                charOffsetX: config.CharOffsetX,
                charOffsetY: config.CharOffsetY);
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
