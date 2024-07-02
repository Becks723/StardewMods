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
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValleyUI.Mvvm;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class FontSettingsMenuModelAsync : FontSettingsMenuModel
    {
        private readonly object _allFontsLock = new();

        private readonly IAsyncFontInfoRetriever _asyncFontInfoRetriever;

        public FontSettingsMenuModelAsync(ModConfig config, IMonitor monitor, IVanillaFontProvider vanillaFontProvider, ISampleFontGenerator sampleFontGenerator, IFontPresetManager presetManager, IFontConfigManager fontConfigManager, IVanillaFontConfigProvider vanillaFontConfigProvider, IAsyncGameFontChanger gameFontChanger, IFontFileProvider fontFileProvider, IDictionary<IContentPack, IFontFileProvider> cpFontFileProviders, IFontInfoRetriever fontInfoRetriever, IAsyncFontInfoRetriever asyncFontInfoRetriever, IFontExporter exporter, SearchManager searchManager, FontSettingsMenuContextModel stagedValues, Func<string> i18nKeepOrigFont, Func<string, string> i18nValidationFontFileNotFound, Func<string, string> i18nFailedToReadFontFile)
            : base(config, monitor, vanillaFontProvider, sampleFontGenerator, presetManager, fontConfigManager, vanillaFontConfigProvider, gameFontChanger, fontFileProvider, cpFontFileProviders, fontInfoRetriever, exporter, searchManager, stagedValues, i18nKeepOrigFont, i18nValidationFontFileNotFound, i18nFailedToReadFontFile)
        {
            this._asyncFontInfoRetriever = asyncFontInfoRetriever;

            void LogAsyncException(string commandName, Exception ex)  // commandName: must end with "Command"
            {
                int length = commandName.Length - "Command".Length;
                string name = commandName.Substring(0, length);

                this._monitor.Log($"Error when {name}: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }
            this.RefreshFontsCommand = new AsyncDelegateCommand(this.RefreshAllFontsAsync, this.CanRefreshAllFonts, ex => LogAsyncException(nameof(this.RefreshFontsCommand), ex));

            // 放在最后（所有依赖项都已初始化完）
            _ = Task.Run(async () =>
            {
                this.AllFonts = new ObservableCollection<FontViewModel>(await this.LoadAllFontsAsync(rescan: false));
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
                if (!this._fontConfigManager.TryGetFontConfig(this.Language, this.CurrentFontType, out FontConfig config))
                    return;

                var userFont = this._fontFileProvider.FontFiles
                    .Where(file => file == config.FontFilePath)
                    .FirstOrDefault();
                var cpFont = this._cpFontFileProviders
                    .Select(x => new { Pack = x.Key, File = x.Value.FontFiles.Where(file => file == config.FontFilePath).FirstOrDefault() })
                    .Where(x => x.File != null)
                    .FirstOrDefault();

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
                else if (cpFont != null)
                {
                    var fontModels = this.GetFontInfoOrWarn(cpFont.File);
                    foreach (FontModel font in fontModels)
                    {
                        this.AllFonts.Add(new FontFromPackViewModel(
                            fontFilePath: font.FullPath,
                            fontIndex: font.FontIndex,
                            displayText: $"{font.FamilyName} ({font.SubfamilyName})",
                            packManifest: cpFont.Pack.Manifest));
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
                //await Task.Delay(2000);  // 延迟2秒，调试更明显

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
                this._monitor.Log(this._i18nFailedToReadFontFile(fontFile), LogLevel.Warn);
                this._monitor.Log($"{result.GetError()}");
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
                this._monitor.Log(this._i18nFailedToReadFontFile(fontFile), LogLevel.Warn);
                this._monitor.Log($"{result.GetError()}");
                return Array.Empty<FontModel>();
            }
        }

        private async Task<IEnumerable<FontViewModel>> LoadAllFontsAsync(bool rescan)
        {
            var newAllFonts = new List<FontViewModel>();
            newAllFonts.Add(this.KeepOriginalFont);
            newAllFonts.AddRange(await this.LoadInstalledFontsAsync(rescan));
            newAllFonts.AddRange(await this.LoadPackFontsAsync(_ => rescan));
            return newAllFonts;
        }

        private async Task<IEnumerable<FontFromPackViewModel>> LoadPackFontsAsync(Func<IContentPack, bool> rescan)
        {
            foreach (var pair in this._cpFontFileProviders)
            {
                if (rescan(pair.Key))
                    pair.Value.RescanForFontFiles();
            }

            return (await Task.WhenAll(
                this._cpFontFileProviders
                    .Select(async pair => (
                        await Task.WhenAll(
                            pair.Value.FontFiles.Select(file => this.GetFontInfoOrWarnAsync(file)))
                        )
                        .SelectMany(font => font)
                        .Select(font => new { Pack = pair.Key, Font = font })
                    ))
                )
                .SelectMany(x => x)
                .Select(x => new FontFromPackViewModel(
                    fontFilePath: x.Font.FullPath,
                    fontIndex: x.Font.FontIndex,
                    displayText: $"{x.Font.FamilyName} ({x.Font.SubfamilyName})",
                    packManifest: x.Pack.Manifest));
        }
    }
}
