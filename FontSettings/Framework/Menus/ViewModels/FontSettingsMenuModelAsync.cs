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

        public FontSettingsMenuModelAsync(ModConfig config, IVanillaFontProvider vanillaFontProvider, IFontGenerator sampleFontGenerator, IAsyncFontGenerator sampleAsyncFontGenerator, IFontPresetManager presetManager, IFontConfigManager fontConfigManager, IVanillaFontConfigProvider vanillaFontConfigProvider, IAsyncGameFontChanger gameFontChanger, IFontFileProvider fontFileProvider, IFontInfoRetriever fontInfoRetriever, IAsyncFontInfoRetriever asyncFontInfoRetriever, FontSettingsMenuContextModel stagedValues)
            : base(config, vanillaFontProvider, sampleFontGenerator, sampleAsyncFontGenerator, presetManager, fontConfigManager, vanillaFontConfigProvider, gameFontChanger, fontFileProvider, fontInfoRetriever, stagedValues)
        {
            this._asyncFontInfoRetriever = asyncFontInfoRetriever;

            void LogAsyncException(string commandName, Exception ex)  // commandName: must end with "Command"
            {
                int length = commandName.Length - "Command".Length;
                string name = commandName.Substring(0, length);

                ILog.Error($"Error when {name}: {ex.Message}\n{ex.StackTrace}");
            }
            this.RefreshFontsCommand = new AsyncDelegateCommand(this.RefreshAllFontsAsync, this.CanRefreshAllFonts, ex => LogAsyncException(nameof(this.RefreshFontsCommand), ex));

            // 放在最后（所有依赖项都已初始化完）
            _ = Task.Run(async () =>
            {
                var allFonts = await this.LoadAllFontsAsync(rescan: false);
                this.AllFonts.Clear();
                foreach (var font in allFonts)
                    this.AllFonts.Add(font);
            });
        }

        /// <summary>仅加载初始化时需要的字体，余下的大部队放异步里加载。</summary>
        protected override void InitAllFonts()
        {
            this.AllFonts = new ObservableCollection<FontViewModel>();

            // single keep-origial
            this.AllFonts.Add(this.KeepOriginalFont);

            // current user font
            {
                if (this._fontConfigManager.TryGetFontConfig(this.Language, this.CurrentFontType, out FontConfig config))
                {
                    var fontFiles = this._fontFileProvider.FontFiles;
                    var userFont = fontFiles.Where(file => file == config.FontFilePath).FirstOrDefault();
                    if (userFont != null)
                    {
                        var userFontModels = this.GetFontInfoOrWarn(userFont);
                        foreach (FontModel font in userFontModels)
                        {
                            this.AllFonts.Add(new FontViewModel(
                                fontFilePath: font.FullPath,
                                fontIndex: font.FontIndex,
                                displayText: $"{font.FamilyName} ({font.SubfamilyName})"));
                        }
                    }
                }
            }
        }

        private async Task RefreshAllFontsAsync()
        {
            try
            {
                _asyncIndicator.IsRefreshingFonts = true;

#if DEBUG
                await Task.Delay(2000);  // 延迟2秒，调试更明显

                // throw new Exception("Test exception for 'async void'");  // 异常处理测试，确保不能崩游戏。
#endif
                // 重新扫描本地字体文件。
                var allFonts = await this.LoadAllFontsAsync(rescan: true);
                this.AllFonts = new ObservableCollection<FontViewModel>(allFonts);

                // 更新选中字体。
                this.CurrentFont = this.FindFont(this.CurrentFontConfig.FontFilePath, this.CurrentFontConfig.FontIndex);

                // 更新示例。
                this.UpdateExampleCurrent();
            }
            finally
            {
                _asyncIndicator.IsRefreshingFonts = false;
            }
        }

        private bool CanRefreshAllFonts()
        {
            return !this.IsRefreshingFonts;
        }

        private async Task<IEnumerable<FontViewModel>> LoadInstalledFontsAsync(bool rescan = false, IEnumerable<string>? skipFontFiles = null)  // rescan: 是否重新扫描本地字体。
        {
            var result = new List<FontViewModel>();

            if (rescan)
                this._fontFileProvider.RescanForFontFiles();

            skipFontFiles ??= Array.Empty<string>();

            var fonts = (
                await Task.WhenAll(
                    this._fontFileProvider.FontFiles
                        .Except(skipFontFiles)
                        .Select(file => this.GetFontInfoOrWarnAsync(file)))
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

        private FontModel[] GetFontInfoOrWarn(string fontFile)
        {
            var result = this._fontInfoRetriever.GetFontInfo(fontFile);
            if (result.IsSuccess)
                return result.GetData();
            else
            {
                ILog.Warn(I18n.Ui_MainMenu_FailedToRecognizeFontFile(fontFile));
                ILog.Trace(result.GetError());
                return Array.Empty<FontModel>();
            }
        }

        private async Task<IEnumerable<FontViewModel>> LoadAllFontsAsync(bool rescan)
        {
            var newAllFonts = new List<FontViewModel>();

            newAllFonts.Add(this.KeepOriginalFont);

            var installedFonts = await this.LoadInstalledFontsAsync(rescan);
            foreach (FontViewModel font in installedFonts)
                newAllFonts.Add(font);

            return newAllFonts;
        }
    }
}
