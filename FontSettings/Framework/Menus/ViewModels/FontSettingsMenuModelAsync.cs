using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FontSettings.Framework.Models;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValleyUI.Mvvm;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class FontSettingsMenuModelAsync : FontSettingsMenuModel
    {
        private readonly object _allFontsLock = new();

        private readonly IAsyncFontInfoRetriever _asyncFontInfoRetriever;

        public FontSettingsMenuModelAsync(ModConfig config, IVanillaFontProvider vanillaFontProvider, IFontGenerator sampleFontGenerator, IAsyncFontGenerator sampleAsyncFontGenerator, IFontPresetManager presetManager, IFontConfigManager fontConfigManager, IVanillaFontConfigProvider vanillaFontConfigProvider, IAsyncGameFontChanger gameFontChanger, IFontFileProvider fontFileProvider, IAsyncFontInfoRetriever asyncFontInfoRetriever, FontSettingsMenuContextModel stagedValues)
            : base(config, vanillaFontProvider, sampleFontGenerator, sampleAsyncFontGenerator, presetManager, fontConfigManager, vanillaFontConfigProvider, gameFontChanger, fontFileProvider, new FontInfoRetriverPlaceHolder(), stagedValues)
        {
            this._asyncFontInfoRetriever = asyncFontInfoRetriever;

            void LogAsyncException(string commandName, Exception ex)  // commandName: must end with "Command"
            {
                int length = commandName.Length - "Command".Length;
                string name = commandName.Substring(0, length);

                ILog.Error($"Error when {name}: {ex.Message}\n{ex.StackTrace}");
            }
            this.RefreshFontsCommand = new AsyncDelegateCommand(this.RefreshAllFontsAsync, this.CanRefreshAllFonts, ex => LogAsyncException(nameof(this.RefreshFontsCommand), ex));

            _ = this.UpdateAllFontsAsync(rescan: false);
        }

        protected override void InitAllFonts()  // 异步初始化AllFonts，且不要await，就可能先于此类构造器执行，构造器中的依赖项还是null。
        { }                                     // 于是改成：这里留空，到构造器末尾，依赖项全部初始化完，再执行。

        private async Task RefreshAllFontsAsync()
        {
            try
            {
                this.IsRefreshingFonts = true;

#if DEBUG
                await Task.Delay(2000);  // 延迟2秒，调试更明显

                // throw new Exception("Test exception for 'async void'");  // 异常处理测试，确保不能崩游戏。
#endif
                await this.UpdateAllFontsAsync(rescan: true);
            }
            finally
            {
                this.IsRefreshingFonts = false;
            }
        }

        private bool CanRefreshAllFonts()
        {
            return !this.IsRefreshingFonts;
        }

        private async Task<IEnumerable<FontViewModel>> LoadInstalledFontsAsync(bool rescan = false)  // rescan: 是否重新扫描本地字体。
        {
            var result = new List<FontViewModel>();

            if (rescan)
                this._fontFileProvider.RescanForFontFiles();

            var fonts = (
                await Task.WhenAll(
                    this._fontFileProvider.FontFiles.Select(file => this.GetFontInfoOrWarnAsync(file)))
                )
                .SelectMany(x => x)
                .Select(font => new FontViewModel(
                    fontFilePath: font.FullPath,
                    fontIndex: font.FontIndex,
                    displayText: $"{font.FamilyName} ({font.SubfamilyName})"));
            result.AddRange(fonts);

            return result;
        }

        private async Task<FontModel[]> GetFontInfoOrWarnAsync(string fontFile)
        {
            var result = await this._asyncFontInfoRetriever.GetFontInfoAsync(fontFile);
            if (result.IsSuccess)
                return result.GetData();
            else
            {
                ILog.Warn(I18n.Ui_MainMenu_FailedToRecognizeFontFile(fontFile));
                ILog.Trace(result.GetError());
                return Array.Empty<FontModel>();
            }
        }

        private FontViewModel FontKeepOriginal()
        {
            FontViewModel vanillaFont;
            {
                var vanillaFontConfig = this._vanillaFontConfigProvider.GetVanillaFontConfig(FontHelpers.GetCurrentLanguage(),
                    this.CurrentFontType);
                vanillaFont = new FontViewModel(
                    fontFilePath: vanillaFontConfig.FontFilePath,
                    fontIndex: vanillaFontConfig.FontIndex,
                    displayText: I18n.Ui_MainMenu_Font_KeepOrig());
            }
            return vanillaFont;
        }

        private async Task UpdateAllFontsAsync(bool rescan)
        {
            lock (this._allFontsLock)
            {
                // clear fonts but first one.
                this.AllFonts.Clear();
                this.AllFonts.Add(this.FontKeepOriginal());
            }

            var installedFonts = await this.LoadInstalledFontsAsync(rescan);

            lock (this._allFontsLock)
                foreach (var font in installedFonts)
                    this.AllFonts.Add(font);

            this.CurrentFont = this.FindFont(this.CurrentFontConfig.FontFilePath, this.CurrentFontConfig.FontIndex);
            this.UpdateExampleCurrent();
        }

        protected override FontViewModel FindFont(string fontFilePath, int fontIndex)
        {
            lock (this._allFontsLock)
            {
                // 这里我们可能遇到AllFonts还没初始化，确保有一个默认的“保持原版”项。
                if (this.AllFonts.Count == 0)
                    this.AllFonts.Add(this.FontKeepOriginal());

                return base.FindFont(fontFilePath, fontIndex);
            }
        }

        private class FontInfoRetriverPlaceHolder : IFontInfoRetriever
        {
            public IResult<FontModel[]> GetFontInfo(string fontFile)
            {
                var fakeFonts = new[]
                {
                    new FontModel
                    {
                        Name = Path.GetFileNameWithoutExtension(fontFile),
                        FullPath = fontFile,
                        FontIndex = 0,
                        FamilyName = Path.GetFileNameWithoutExtension(fontFile),
                        SubfamilyName = "未知"
                    }
                };
                return new SuccessResult<FontModel[]>(fakeFonts);
            }

            private record SuccessResult<TData>(TData Data) : IResult<TData>
            {
                public bool IsSuccess => true;
                public TData GetData() => this.Data;
                public string GetError() => string.Empty;
            }
        }
    }
}
