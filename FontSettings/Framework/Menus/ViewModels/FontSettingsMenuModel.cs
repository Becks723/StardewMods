﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FontSettings.Framework.Models;
using FontSettings.Framework.Preset;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValleyUI.Mvvm;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class FontSettingsMenuModel : MenuModelBase, INotifyDataErrorInfo
    {
        protected static readonly AsyncIndicator _asyncIndicator = new();

        protected readonly FontSettingsMenuContextModel _stagedValues;
        protected readonly Dictionary<GameFontType, FontPresetViewModel> _presetViewModels = new();
        protected readonly ModConfig _config;
        protected readonly IMonitor _monitor;
        protected readonly IVanillaFontProvider _vanillaFontProvider;
        protected readonly ISampleFontGenerator _sampleFontGenerator;
        protected readonly IFontConfigManager _fontConfigManager;
        protected readonly IVanillaFontConfigProvider _vanillaFontConfigProvider;
        protected readonly IAsyncGameFontChanger _gameFontChanger;
        protected readonly IFontFileProvider _fontFileProvider;
        protected readonly IDictionary<IContentPack, IFontFileProvider> _cpFontFileProviders;
        protected readonly IFontInfoRetriever _fontInfoRetriever;
        protected readonly IFontExporter _exporter;
        protected readonly SearchManager _searchManager;
        protected readonly IFontPresetManager _presetManager;
        protected readonly Func<string> _i18nKeepOrig;
        protected readonly Func<string, string> _i18nValidationFontFileNotFound;
        protected readonly Func<string, string> _i18nFailedToReadFontFile;


        #region CurrentFontType Property

        private GameFontType _currentFontType;

        public GameFontType CurrentFontType
        {
            get => this._currentFontType;
            set
            {
                this.SetField(ref this._currentFontType, value);
                this.RaisePropertyChanged(nameof(this.ExampleText));
            }
        }

        #endregion

        #region PreviewMode Property

        private PreviewMode _previewMode;

        public PreviewMode PreviewMode
        {
            get => this._previewMode;
            set => this.SetField(ref this._previewMode, value);
        }

        #endregion

        #region ShowExampleBounds Property

        private bool _showExampleBounds;

        public bool ShowExampleBounds
        {
            get => this._showExampleBounds;
            set => this.SetField(ref this._showExampleBounds, value);
        }

        #endregion

        #region ShowExampleText Property

        private bool _showExampleText;

        public bool ShowExampleText
        {
            get => this._showExampleText;
            set => this.SetField(ref this._showExampleText, value);
        }

        #endregion

        #region IsTuningCharOffset Property

        private bool _isTuningOffset;

        public bool IsTuningCharOffset
        {
            get => this._isTuningOffset;
            set => this.SetField(ref this._isTuningOffset, value);
        }

        #endregion

        public string ExampleText
        {
            get
            {
                string text = this._config.ExampleText;

                if (string.IsNullOrWhiteSpace(text))
                    text = this._config.Sample.GetTextForCurrentLangauge();

                if (string.IsNullOrWhiteSpace(text))
                    text = this._config.Sample.GetTextForLangauge(FontHelpers.LanguageEn);

                return this.CurrentFontType is GameFontType.SpriteText
                    ? text?.Replace('\n', '^')
                    : text;
            }
        }

        #region ExampleCurrentFont Property

        private ISpriteFont _example_current_font;

        public ISpriteFont ExampleCurrentFont
        {
            get => this._example_current_font;
            set => this.SetField(ref this._example_current_font, value);
        }

        #endregion

        #region ExampleVanillaFont Property

        private ISpriteFont _exampleVanillaFont;

        public ISpriteFont ExampleVanillaFont
        {
            get => this._exampleVanillaFont;
            set => this.SetField(ref this._exampleVanillaFont, value);
        }

        #endregion

        #region CurrentFontConfig Property

        private FontConfig _currentFontConfig;
        protected FontConfig CurrentFontConfig
        {
            get => this._currentFontConfig;
            set
            {
                this.SetField(ref this._currentFontConfig, value);

                this.RaisePropertyChanged(nameof(this.CurrentFontConfigRealTime));
            }
        }

        #endregion

        #region CurrentFontConfigRealTime

        private FontConfig _currentFontConfigRealTime;
        private FontConfig CurrentFontConfigRealTime
        {
            get => this._currentFontConfigRealTime;
            set => this.SetField(ref this._currentFontConfigRealTime, value);
        }

        #endregion

        #region FontEnabled Property

        private bool _fontEnabled;

        public bool FontEnabled
        {
            get => this._fontEnabled;
            set => this.SetField(ref this._fontEnabled, value);
        }

        #endregion

        #region FontSize Property

        private float _fontSize;

        public float FontSize
        {
            get => this._fontSize;
            set => this.SetField(ref this._fontSize, value);
        }

        #endregion

        #region Spacing Property

        private float _spacing;

        public float Spacing
        {
            get => this._spacing;
            set => this.SetField(ref this._spacing, value);
        }

        #endregion

        #region LineSpacing Property

        private float _lineSpacing;

        public float LineSpacing
        {
            get => this._lineSpacing;
            set => this.SetField(ref this._lineSpacing, value);
        }

        #endregion

        #region CharOffsetX Property

        private float _charOffsetX;

        public float CharOffsetX
        {
            get => this._charOffsetX;
            set => this.SetField(ref this._charOffsetX, value);
        }

        #endregion

        #region CharOffsetY Property

        private float _charOffsetY;

        public float CharOffsetY
        {
            get => this._charOffsetY;
            set => this.SetField(ref this._charOffsetY, value);
        }

        #endregion

        #region CurrentFont Property

        private FontViewModel _currentFont;

        public FontViewModel CurrentFont
        {
            get => this._currentFont;
            set
            {
                this.SetField(ref this._currentFont, value);
                this.ValidateCurrentFont();

                this.RaisePropertyChanged(nameof(this.FontFilePath));
                this.RaisePropertyChanged(nameof(this.FontIndex));
            }
        }

        #endregion

        #region PixelZoom Property

        private float _pixelZoom;

        public float PixelZoom
        {
            get => this._pixelZoom;
            set => this.SetField(ref this._pixelZoom, value);
        }

        #endregion

        #region FontFilePath Property

        public string FontFilePath => this.CurrentFont.FontFilePath;

        #endregion

        #region FontIndex Property

        public int FontIndex => this.CurrentFont.FontIndex;

        #endregion

        #region Characters Property

        private ObservableCollection<char> _characters = new();

        public ObservableCollection<char> Characters
        {
            get => this._characters;
            set => this.SetField(ref this._characters, value);
        }

        #endregion

        #region DefaultCharacter Property

        private char? _defaultCharacter;

        public char? DefaultCharacter
        {
            get => this._defaultCharacter;
            set => this.SetField(ref this._defaultCharacter, value);
        }

        #endregion

        #region IsGeneratingFont Property

        private bool _isGeneratingFont;
        public bool IsGeneratingFont
        {
            get => this._isGeneratingFont;
            set
            {
                this.SetField(ref this._isGeneratingFont, value);
                this.RaisePropertyChanged(nameof(this.CanGenerateFont));
            }
        }

        #endregion

        public bool CanGenerateFont => !this.IsGeneratingFont /*&& !this.HasErrors*/;

        #region IsRefreshingFonts Property

        private bool _isRefreshingFonts;

        public bool IsRefreshingFonts
        {
            get => this._isRefreshingFonts;
            set => this.SetField(ref this._isRefreshingFonts, value);
        }

        #endregion

        #region AllFonts Property

        private ObservableCollection<FontViewModel> _allFonts = new();

        public ObservableCollection<FontViewModel> AllFonts
        {
            get => this._allFonts;
            set => this.SetField(ref this._allFonts, value);
        }

        #endregion

        #region CurrentPresetTitle Property

        private string _currentPresetTitle;

        public string CurrentPresetTitle
        {
            get => this._currentPresetTitle;
            set => this.SetField(ref this._currentPresetTitle, value);
        }

        #endregion

        #region CurrentPresetSubtitle Property

        private string _currentPresetSubtitle;

        public string CurrentPresetSubtitle
        {
            get => this._currentPresetSubtitle;
            set => this.SetField(ref this._currentPresetSubtitle, value);
        }

        #endregion

        #region IsCurrentPresetValid Property

        private bool _isCurrentPresetValid = true;

        [Obsolete("Will be always true until finally removed.")]
        public bool IsCurrentPresetValid
        {
            get => this._isCurrentPresetValid;
            set
            {
                this.SetField(ref this._isCurrentPresetValid, value);
                this.RaisePropertyChanged(nameof(this.CanGenerateFont));
            }
        }

        #endregion

        #region MessageWhenPresetIsInvalid Property

        private string _messageWhenPresetIsInvalid;

        [Obsolete("Will be always null until finally removed.")]
        public string MessageWhenPresetIsInvalid
        {
            get => this._messageWhenPresetIsInvalid;
            set => this.SetField(ref this._messageWhenPresetIsInvalid, value);
        }

        #endregion

        #region CanSaveCurrentPreset Property

        private bool _canSaveCurrentPreset;

        public bool CanSaveCurrentPreset
        {
            get => this._canSaveCurrentPreset;
            set => this.SetField(ref this._canSaveCurrentPreset, value);
        }

        #endregion

        #region CanSaveCurrentAsNewPreset Property

        private bool _canSaveCurrentAsNewPreset;

        public bool CanSaveCurrentAsNewPreset
        {
            get => this._canSaveCurrentAsNewPreset;
            set => this.SetField(ref this._canSaveCurrentAsNewPreset, value);
        }

        #endregion

        #region CanDeleteCurrentPreset Property

        private bool _canDeleteCurrentPreset;

        public bool CanDeleteCurrentPreset
        {
            get => this._canDeleteCurrentPreset;
            set => this.SetField(ref this._canDeleteCurrentPreset, value);
        }

        #endregion

        #region MinCharOffsetX Property

        private float _minCharOffsetX;

        public float MinCharOffsetX
        {
            get => this._minCharOffsetX;
            set => this.SetField(ref this._minCharOffsetX, value);
        }

        #endregion

        #region MaxCharOffsetX Property

        private float _maxCharOffsetX;

        public float MaxCharOffsetX
        {
            get => this._maxCharOffsetX;
            set => this.SetField(ref this._maxCharOffsetX, value);
        }

        #endregion

        #region MinCharOffsetY Property

        private float _minCharOffsetY;

        public float MinCharOffsetY
        {
            get => this._minCharOffsetY;
            set => this.SetField(ref this._minCharOffsetY, value);
        }

        #endregion

        #region MaxCharOffsetY Property

        private float _maxCharOffsetY;

        public float MaxCharOffsetY
        {
            get => this._maxCharOffsetY;
            set => this.SetField(ref this._maxCharOffsetY, value);
        }

        #endregion

        #region MinFontSize Property

        private float _minFontSize;

        public float MinFontSize
        {
            get => this._minFontSize;
            set => this.SetField(ref this._minFontSize, value);
        }

        #endregion

        #region MaxFontSize Property

        private float _maxFontSize;

        public float MaxFontSize
        {
            get => this._maxFontSize;
            set => this.SetField(ref this._maxFontSize, value);
        }

        #endregion

        #region MinSpacing Property

        private float _minSpacing;

        public float MinSpacing
        {
            get => this._minSpacing;
            set => this.SetField(ref this._minSpacing, value);
        }

        #endregion

        #region MaxSpacing Property

        private float _maxSpacing;

        public float MaxSpacing
        {
            get => this._maxSpacing;
            set => this.SetField(ref this._maxSpacing, value);
        }

        #endregion

        #region MinLineSpacing Property

        private int _minLineSpacing;

        public int MinLineSpacing
        {
            get => this._minLineSpacing;
            set => this.SetField(ref this._minLineSpacing, value);
        }

        #endregion

        #region MaxLineSpacing Property

        private int _maxLineSpacing;

        public int MaxLineSpacing
        {
            get => this._maxLineSpacing;
            set => this.SetField(ref this._maxLineSpacing, value);
        }

        #endregion

        #region MinPixelZoom Property

        private float _minPixelZoom;

        public float MinPixelZoom
        {
            get => this._minPixelZoom;
            set => this.SetField(ref this._minPixelZoom, value);
        }

        #endregion

        #region MaxPixelZoom Property

        private float _maxPixelZoom;

        public float MaxPixelZoom
        {
            get => this._maxPixelZoom;
            set => this.SetField(ref this._maxPixelZoom, value);
        }

        #endregion

        #region Mask Property

        private Color _mask;

        public Color Mask
        {
            get => this._mask;
            set => this.SetField(ref this._mask, value);
        }

        #endregion

        protected FontViewModel KeepOriginalFont { get; set; }

        protected LanguageInfo Language { get; }

        public ICommand MoveToPrevFontCommand { get; }

        public ICommand MoveToNextFontCommand { get; }

        public ICommand MoveToPrevPresetCommand { get; }

        public ICommand MoveToNextPresetCommand { get; }

        public ICommand SaveCurrentPresetCommand { get; }

        public ICommand SaveCurrentAsNewPresetCommand { get; }

        public ICommand DeleteCurrentPresetCommand { get; }

        public ICommand RefreshFontsCommand { get; protected init; }

        public ICommand ManageFontsCommand { get; }

        public ICommand ResetFontCommand { get; }

        public FontSettingsMenuModel(ModConfig config, IMonitor monitor, IVanillaFontProvider vanillaFontProvider, ISampleFontGenerator sampleFontGenerator, IFontPresetManager presetManager,
            IFontConfigManager fontConfigManager, IVanillaFontConfigProvider vanillaFontConfigProvider, IAsyncGameFontChanger gameFontChanger, IFontFileProvider fontFileProvider, IDictionary<IContentPack, IFontFileProvider> cpFontFileProviders, IFontInfoRetriever fontInfoRetriever, IFontExporter exporter, SearchManager searchManager, FontSettingsMenuContextModel stagedValues,
            Func<string> i18nKeepOrigFont,
            Func<string, string> i18nValidationFontFileNotFound,
            Func<string, string> i18nFailedToReadFontFile)
        {
            // 订阅异步完成事件。
            _asyncIndicator.IsGeneratingFontChanged += (_, fontType) => this.IsGeneratingFont = _asyncIndicator.IsGeneratingFont(fontType);
            _asyncIndicator.IsRefreshingFontsChanged += (_, _) => this.IsRefreshingFonts = _asyncIndicator.IsRefreshingFonts;

            this._config = config;
            this._monitor = monitor;
            this._vanillaFontProvider = vanillaFontProvider;
            this._sampleFontGenerator = sampleFontGenerator;
            this._fontConfigManager = fontConfigManager;
            this._vanillaFontConfigProvider = vanillaFontConfigProvider;
            this._gameFontChanger = gameFontChanger;
            this._fontFileProvider = fontFileProvider;
            this._cpFontFileProviders = cpFontFileProviders;
            this._fontInfoRetriever = fontInfoRetriever;
            this._exporter = exporter;
            this._searchManager = searchManager;
            this._presetManager = presetManager;
            this._stagedValues = stagedValues;
            this._i18nKeepOrig = i18nKeepOrigFont ?? (() => "Keep Original");
            this._i18nValidationFontFileNotFound = i18nValidationFontFileNotFound ?? (filePath => $"Font file not found: {filePath}");
            this._i18nFailedToReadFontFile = i18nFailedToReadFontFile ?? (filePath => $"Failed to recognize font file: {filePath}");

            // 初始化子ViewModel。
            foreach (var type in Enum.GetValues<GameFontType>())
            {
                var vm = new FontPresetViewModel(this._presetManager, type, this._stagedValues.Presets[type]);
                vm.PresetChanged += this.OnPresetChanged;
                vm.PropertyChanged += this.OnPresetViewModelPropertyChanged;
                this._presetViewModels.Add(type, vm);
            }

            PropertyChanged += this.OnPropertyChangedNotifyPresetViewModel;

            // 每当部分属性变化时，记录它们的值，以便再次打开菜单时填入。（记忆功能）
            this.RegisterCallbackToStageValues();

            // 填入之前记录的属性值。
            this.CurrentFontType = this._stagedValues.FontType;
            this.PreviewMode = this._stagedValues.PreviewMode;
            this.ShowExampleBounds = this._stagedValues.ShowBounds;
            this.ShowExampleText = this._stagedValues.ShowText;
            this.IsTuningCharOffset = this._stagedValues.OffsetTuning;

            // 填入异步布尔值。
            this.IsGeneratingFont = _asyncIndicator.IsGeneratingFont(this.CurrentFontType);
            this.IsRefreshingFonts = _asyncIndicator.IsRefreshingFonts;

            // 部分语言不支持SpriteText，重置当前字体类型。
            if (this.SkipSpriteText() && this.CurrentFontType == GameFontType.SpriteText)
                this.CurrentFontType = GameFontType.SmallFont;

            this.MinCharOffsetX = this._config.MinCharOffsetX;
            this.MaxCharOffsetX = this._config.MaxCharOffsetX;
            this.MinCharOffsetY = this._config.MinCharOffsetY;
            this.MaxCharOffsetY = this._config.MaxCharOffsetY;
            this.MinFontSize = this._config.MinFontSize;
            this.MaxFontSize = this._config.MaxFontSize;
            this.MinSpacing = this._config.MinSpacing;
            this.MaxSpacing = this._config.MaxSpacing;
            this.MinLineSpacing = this._config.MinLineSpacing;
            this.MaxLineSpacing = this._config.MaxLineSpacing;
            this.MinPixelZoom = this._config.MinPixelZoom;
            this.MaxPixelZoom = this._config.MaxPixelZoom;
            this.Language = FontHelpers.GetCurrentLanguage();

            // 初始化命令。
            this.MoveToPrevFontCommand = new DelegateCommand(this.PreviousFontType);
            this.MoveToNextFontCommand = new DelegateCommand(this.NextFontType);
            this.MoveToPrevPresetCommand = new DelegateCommand(this.PreviousPreset);
            this.MoveToNextPresetCommand = new DelegateCommand(this.NextPreset);
            this.SaveCurrentPresetCommand = new DelegateCommand(this.SaveCurrentPreset);
            this.SaveCurrentAsNewPresetCommand = new DelegateCommand<Func<IOverlayMenu>>(this.SaveCurrentAsNewPreset);
            this.DeleteCurrentPresetCommand = new DelegateCommand(this._DeleteCurrentPreset);
            this.RefreshFontsCommand = new DelegateCommand(this.RefreshAllFonts);
            this.ManageFontsCommand = new DelegateCommand<Func<IOverlayMenu>>(this.ManageFonts);
            this.ResetFontCommand = new DelegateCommand(this.ResetCurrentFont);

            // 读取并应用搜索设置。
            this._searchManager.ApplySearchSettings();

            this.KeepOriginalFont = this.FontKeepOriginal();
            this.InitAllFonts();
            this.OnFontTypeChanged(this.CurrentFontType);
        }

        /// <summary>这里settings指直接影响字体的设置项。</summary>
        public void OnSettingsChanged()
        {
            this.CurrentFontConfigRealTime = this.CreateConfigBasedOnCurrentSettings();
        }

        public bool TryGetCurrentPresetIfSupportsDetailedInfo(out FontPreset preset)
            => this.PresetViewModel(this.CurrentFontType).TryGetCurrentPresetIfSupportsDetailedInfo(out preset);

        public ExportMenuModel GetCurrentExportViewModel()
            => this.GetExportViewModel(this.CurrentFontType);

        public ExportMenuModel GetExportViewModel(GameFontType fontType)
        {
            return new ExportMenuModel(
                exporter: this._exporter,
                fontConfig: this.CreateConfigBasedOnCurrentSettings(),
                context: new FontContext(this.Language, fontType),
                staged: this._stagedValues.Exporting[fontType]);
        }

        private void PreviousFontType()
        {
            this.CurrentFontType = this.CurrentFontType.Previous(this.SkipSpriteText());
            this.OnFontTypeChanged(this.CurrentFontType);
        }

        private void NextFontType()
        {
            this.CurrentFontType = this.CurrentFontType.Next(this.SkipSpriteText());
            this.OnFontTypeChanged(this.CurrentFontType);
        }

        private void PreviousPreset()
        {
            this.PresetViewModel(this.CurrentFontType)
                .MoveToPreviousPreset();
        }

        private void NextPreset()
        {
            this.PresetViewModel(this.CurrentFontType)
                .MoveToNextPreset();
        }

        private void SaveCurrentPreset()
        {
            this.PresetViewModel(this.CurrentFontType)
                .SaveCurrentPreset(this.CreateConfigBasedOnCurrentSettings());
        }

        private void SaveCurrentAsNewPreset(Func<IOverlayMenu> createOverlay)
        {
            if (createOverlay == null) return;

            var overlay = createOverlay();
            if (overlay != null)
            {
                overlay.Open();
                overlay.Closed += (s, e) =>
                {
                    if (e.Parameter is string presetName)
                        this.SaveCurrentAsNewPreset(presetName);
                };
            }
        }

        private void SaveCurrentAsNewPreset(string newPresetName)
        {
            this.PresetViewModel(this.CurrentFontType)
                .SaveCurrentAsNewPreset(newPresetName, this.CreateConfigBasedOnCurrentSettings());
        }

        private void _DeleteCurrentPreset()
        {
            this.PresetViewModel(this.CurrentFontType)
                .DeleteCurrentPreset();
        }

        private void RefreshAllFonts()
        {
            // 重新扫描本地字体文件。
            this.AllFonts = new ObservableCollection<FontViewModel>(this.LoadAllFonts(true));

            // 更新选中字体。
            this.CurrentFont = this.FindFont(this.FontFilePath, this.FontIndex);

            // 更新示例。
            this.UpdateExampleCurrent();
        }

        private void ManageFonts(Func<IOverlayMenu> createOverlay)
        {
            if (createOverlay == null) return;

            var overlay = createOverlay();
            if (overlay != null)
            {
                overlay.Open();
                overlay.Closed += (s, e) =>
                {
                    ManageSettings settings = e.Parameter as ManageSettings;
                    if (settings == null)
                        return;

                    this._searchManager.ApplySearchSettings(settings.Search);
                };
            }
        }

        private void ResetCurrentFont()
        {
            this.CurrentFontConfig = this._vanillaFontConfigProvider.GetVanillaFontConfig(this.Language, this.CurrentFontType);
            this.FillOptionsWithFontConfig(this.CurrentFontConfig);

            // 更新示例。
            this.UpdateExampleCurrent();
        }

        public virtual async Task<FontChangeResult> ChangeFontAsync()
        {
            // update with current settings.
            this.CurrentFontConfig = this.CreateConfigBasedOnCurrentSettings();

            var fontType = this.CurrentFontType;  // 可能在异步完成后变化，需要存一下。
            var fontConfig = this.CurrentFontConfig;  // 可能在异步完成后变化，需要存一下。

            try
            {
                _asyncIndicator.UpdateIsGeneratingFont(this.CurrentFontType, true);

                var result = await this._gameFontChanger.ChangeGameFontAsync(
                    font: this.CurrentFontConfig,
                    context: this.GetFontContext());

                // 如果成功，更新配置值。
                if (result.IsSuccess)
                    this._fontConfigManager.UpdateFontConfig(
                        this.Language, fontType, fontConfig);

                return new FontChangeResult(result, fontType);
            }
            finally
            {
                _asyncIndicator.UpdateIsGeneratingFont(fontType, false);
            }
        }

        internal record FontChangeResult(IResultWithoutData<string> InnerResult, GameFontType FontType);

        private FontContext GetFontContext()
        {
            return FontContext.For(this.Language, this.CurrentFontType);
        }

        public void UpdateExampleVanilla()
        {
            try
            {
                this.ExampleVanillaFont = this._vanillaFontProvider.GetVanillaFont(
                    FontHelpers.GetCurrentLanguage(), this.CurrentFontType);
            }
            catch (Exception ex)
            {
                this._monitor.Log($"Error in vanilla sample font: {ex.Message}\n{ex.StackTrace}");
                this.ExampleVanillaFont = null;
            }
        }

        public void UpdateExampleCurrent()
        {
            try
            {
                var config = this.CreateConfigBasedOnCurrentSettings(forSample: true);
                var context = new FontContext(this.Language, this.CurrentFontType);

                this.ExampleCurrentFont = this._sampleFontGenerator.Generate(config, context);
            }
            catch (Exception ex)
            {
                this._monitor.Log($"Error in current sample font: {ex.Message}\n{ex.StackTrace}");
                this.ExampleCurrentFont = null;
            }
        }

        private bool _isUpdatingExampleCurrent;
        private CancellationTokenSource _tokenSource;
        public async Task UpdateExampleCurrentAsync()
        {
            if (this._isUpdatingExampleCurrent)
            {
                this._tokenSource.Cancel();
                this._tokenSource.Dispose();
            }

            var config = this.CreateConfigBasedOnCurrentSettings(forSample: true);
            var context = new FontContext(this.Language, this.CurrentFontType);

            await this.RestartUpdateExampleCurrent(config, context);
        }

        private async Task RestartUpdateExampleCurrent(FontConfig config, FontContext context)
        {
            this._tokenSource = new CancellationTokenSource();
            var token = this._tokenSource.Token;

            var lastExampleCurrentFont = this.ExampleCurrentFont;
            this._isUpdatingExampleCurrent = true;
            try
            {
                this.ExampleCurrentFont = await this._sampleFontGenerator.GenerateAsync(config, context, token);
                this._monitor.VerboseLog("Sample: Set");
            }
            catch (OperationCanceledException)
            {
                this._monitor.VerboseLog("Sample: Cancelled");
                this.ExampleCurrentFont = lastExampleCurrentFont;
            }
            catch (Exception ex)
            {
                this._monitor.Log($"Sample: {ex.Message}\n{ex.StackTrace}");
                this.ExampleCurrentFont = lastExampleCurrentFont;
            }
            finally
            {
                this._isUpdatingExampleCurrent = false;
                this._tokenSource.Dispose();
            }
        }

        private void OnFontTypeChanged(GameFontType newFontType)
        {
            // 更新默认字体。
            this.KeepOriginalFont = this.FontKeepOriginal();

            // 更新各个属性。
            FontConfig fontConfig = this.GetOrCreateFontConfig();
            this.CurrentFontConfig = fontConfig;
            this.FillOptionsWithFontConfig(fontConfig);

            // 更新预设。
            this.OnPresetChanged(null, EventArgs.Empty);

            // 更新预览图。
            this.UpdateExampleVanilla();
            this.UpdateExampleCurrent();
        }

        private void OnPresetChanged(object sender, EventArgs e)
        {
            var presetViewModel = this.PresetViewModel(this.CurrentFontType);
            FontConfig settings = presetViewModel.CurrentPresetSettings;
            bool noPresetSelected = settings == null;

            // 更新预设标题。
            if (noPresetSelected)
            {
                this.CurrentPresetTitle = "-";
                this.CurrentPresetSubtitle = string.Empty;
            }
            else
            {
                this.CurrentPresetTitle = presetViewModel.CurrentPresetNameOrNull ?? string.Empty;
                this.CurrentPresetSubtitle = presetViewModel.CurrentPresetAuthorOrNull ?? string.Empty;
            }

            // 更新几个状态：是否能另存为、删除该预设。
            this.CanSaveCurrentAsNewPreset = presetViewModel.CanSaveAsNewPreset();
            this.CanDeleteCurrentPreset = presetViewModel.CanDeletePreset();

            // 如果无预设，则载入保存的设置。
            if (noPresetSelected)
            {
                this.FillOptionsWithFontConfig(this.CurrentFontConfig);
            }

            // 否则载入预设的值。
            else
            {
                this.FillOptionsWithFontConfig(settings);
            }

            this.CurrentFontConfigRealTime = this.CreateConfigBasedOnCurrentSettings();

            this.UpdateExampleCurrent();
        }

        private void OnPresetViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var presetViewModel = this.PresetViewModel(this.CurrentFontType);

            switch (e.PropertyName)
            {
                case nameof(FontPresetViewModel.CanSaveCurrentPreset):
                    this.CanSaveCurrentPreset = presetViewModel.CanSaveCurrentPreset;
                    break;
            }
        }

        private void OnPropertyChangedNotifyPresetViewModel(object sender, PropertyChangedEventArgs e)
        {
            var presetViewModel = this.PresetViewModel(this.CurrentFontType);

            switch (e.PropertyName)
            {
                case nameof(this.CurrentFontConfigRealTime):
                    presetViewModel.CurrentFontConfigRealTime = this.CurrentFontConfigRealTime;
                    break;
            }
        }

        private FontConfig GetOrCreateFontConfig()
        {
            var langugage = FontHelpers.GetCurrentLanguage();
            var fontType = this.CurrentFontType;

            if (this._fontConfigManager.TryGetFontConfig(langugage, fontType, out FontConfig config))
                return config;

            return this._vanillaFontConfigProvider.GetVanillaFontConfig(langugage, fontType);
        }

        private FontConfig CreateConfigBasedOnCurrentSettings(bool forSample = false)
        {
            var builder = new FontConfigBuilder()
                .BasicConfig(new FontConfig(
                    Enabled: this.FontEnabled,
                    FontFilePath: this.FontFilePath,
                    FontIndex: this.FontIndex,
                    FontSize: this.FontSize,
                    Spacing: this.Spacing,
                    LineSpacing: this.LineSpacing,
                    CharOffsetX: this.CharOffsetX,
                    CharOffsetY: this.CharOffsetY,
                    CharacterRanges: forSample
                        ? FontHelpers.GetCharRange(this.ExampleText, this.DefaultCharacter)
                        : FontHelpers.GetCharacterRanges(this.Characters)))
                .WithSolidColorMask(this.Mask);
            if (this.CurrentFontType != GameFontType.SpriteText)
                builder.WithDefaultCharacter(this.DefaultCharacter);
            if (this.CurrentFontType == GameFontType.SpriteText)
                builder.WithPixelZoom(this.PixelZoom);

            return builder.Build();
        }

        private IEnumerable<FontViewModel> LoadAllFonts(bool rescan = false)  // rescan: 是否重新扫描本地字体。
        {
            if (rescan)
                this._fontFileProvider.RescanForFontFiles();

            // single keep-orig font
            FontViewModel vanillaFont;
            {
                var vanillaFontConfig = this._vanillaFontConfigProvider.GetVanillaFontConfig(FontHelpers.GetCurrentLanguage(),
                    this.CurrentFontType);
                string fontFilePath = vanillaFontConfig.FontFilePath;
                int fontIndex = vanillaFontConfig.FontIndex;

                vanillaFont = new FontViewModel(
                    fontFilePath: fontFilePath,
                    fontIndex: fontIndex,
                    displayText: this._i18nKeepOrig());
            }
            yield return vanillaFont;

            // user fonts
            {
                var fonts = this._fontFileProvider.FontFiles
                    .SelectMany(file =>
                    {
                        var result = this._fontInfoRetriever.GetFontInfo(file);
                        if (result.IsSuccess)
                            return result.GetData();
                        else
                        {
                            this._monitor.Log(this._i18nFailedToReadFontFile(file), LogLevel.Warn);
                            this._monitor.Log($"{result.GetError()}");
                            return Array.Empty<FontModel>();
                        }
                    })
                    .Select(font => new FontViewModel(
                        fontFilePath: font.FullPath,
                        fontIndex: font.FontIndex,
                        displayText: $"{font.FamilyName} ({font.SubfamilyName})")
                    );
                foreach (var font in fonts)
                    yield return font;
            }

            // content pack fonts
            {
                var fonts = this._cpFontFileProviders
                    .Select(pair => (Pack: pair.Key, Fonts: pair.Value.FontFiles.SelectMany(file =>
                    {
                        var result = this._fontInfoRetriever.GetFontInfo(file);
                        if (result.IsSuccess)
                            return result.GetData();
                        else
                        {
                            this._monitor.Log(this._i18nFailedToReadFontFile(file), LogLevel.Warn);
                            this._monitor.Log($"{result.GetError()}");
                            return Array.Empty<FontModel>();
                        }
                    })))
                    .SelectMany(x => x.Fonts.Select(f => (x.Pack, Font: f)))
                    .Select(x => new FontFromPackViewModel(
                        fontFilePath: x.Font.FullPath,
                        fontIndex: x.Font.FontIndex,
                        displayText: $"{x.Font.FamilyName} ({x.Font.SubfamilyName})",
                        packManifest: x.Pack.Manifest)
                    );
                foreach (var font in fonts)
                    yield return font;
            }
        }

        protected virtual void InitAllFonts()
        {
            this.AllFonts = new ObservableCollection<FontViewModel>(this.LoadAllFonts());
        }

        protected virtual FontViewModel FindFont(string fontFilePath, int fontIndex) // 这里path是fullpath
        {
            // 如果找不到字体文件，保持原版。

            if (fontFilePath == null)
                return this.KeepOriginalFont;

            var found = this.AllFonts.Where(f => f.FontFilePath == fontFilePath && f.FontIndex == fontIndex);
            if (found.Any())
                return found.FirstOrDefault();

            return this.KeepOriginalFont;
        }

        private void FillOptionsWithFontConfig(FontConfig fontConfig)
        {
            this.FontEnabled = fontConfig.Enabled;
            this.FontSize = fontConfig.FontSize;
            this.Spacing = fontConfig.Spacing;
            this.LineSpacing = fontConfig.LineSpacing;
            this.CharOffsetX = fontConfig.CharOffsetX;
            this.CharOffsetY = fontConfig.CharOffsetY;
            this.CurrentFont = this.FindFont(fontConfig.FontFilePath, fontConfig.FontIndex);
            this.PixelZoom = fontConfig.TryGetInstance(out IWithPixelZoom withPixelZoom)
                ? withPixelZoom.PixelZoom
                : 0;
            this.Characters = new ObservableCollection<char>(
                FontHelpers.GetCharacters(fontConfig.CharacterRanges));
            this.DefaultCharacter = fontConfig.TryGetInstance(out IWithDefaultCharacter withDefaultCharacter)
                ? withDefaultCharacter.DefaultCharacter
                : null;
            this.Mask = fontConfig.TryGetInstance(out IWithSolidColor withSolidColor)
                ? withSolidColor.SolidColor
                : Color.White;
        }

        private bool SkipSpriteText()
        {
            return !this._config.EnableLatinDialogueFont && LocalizedContentManager.CurrentLanguageLatin;
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
                    displayText: this._i18nKeepOrig());
            }
            return vanillaFont;
        }

        [Obsolete("验证字体文件的逻辑需要转移，此方法本身废除。")]
        private bool IsPresetValid(Framework.DataAccess.Models.FontPresetData preset, out FontViewModel match, out string invalidMessage)
        {
            match = null;
            invalidMessage = null;

            var allFonts = this.AllFonts;
            bool keepOriginal;
            string specifiedName = null;
            string specifiedExtension = null;
            if (string.IsNullOrWhiteSpace(preset.Requires.FontFileName))
                keepOriginal = true;
            else
            {
                keepOriginal = false;
                string specifiedFile = PathUtilities.NormalizePath(preset.Requires.FontFileName);
                specifiedName = Path.GetFileNameWithoutExtension(specifiedFile);
                specifiedExtension = Path.GetExtension(specifiedFile);
            }

            foreach (FontViewModel font in allFonts)
                if (keepOriginal)
                    if (font.FontFilePath == null)
                    {
                        match = font;
                        return true;
                    }
                    else
                    {
                        string name = Path.GetFileNameWithoutExtension(font.FontFilePath);
                        string extension = Path.GetExtension(font.FontFilePath);
                        if (name == specifiedName
                            && extension.Equals(specifiedExtension, StringComparison.OrdinalIgnoreCase)
                            && preset.FontIndex == font.FontIndex)
                        {
                            match = font;
                            return true;
                        }
                    }

            invalidMessage = $"当前预设不可用。需要安装字体文件：{preset.Requires.FontFileName}。";
            return false;
        }

        private FontPresetViewModel PresetViewModel(GameFontType fontType)
        {
            return this._presetViewModels[fontType];
        }

        private void RegisterCallbackToStageValues()
        {
            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(this.CurrentFontType):
                        this._stagedValues.FontType = this.CurrentFontType;
                        break;

                    case nameof(this.PreviewMode):
                        this._stagedValues.PreviewMode = this.PreviewMode;
                        break;

                    case nameof(this.ShowExampleBounds):
                        this._stagedValues.ShowBounds = this.ShowExampleBounds;
                        break;

                    case nameof(this.ShowExampleText):
                        this._stagedValues.ShowText = this.ShowExampleText;
                        break;

                    case nameof(this.IsTuningCharOffset):
                        this._stagedValues.OffsetTuning = this.IsTuningCharOffset;
                        break;
                }
            };
        }

        // TODO: pull up
        #region INotifyDataErrorInfo implementation

        private readonly IDictionary<string, IEnumerable<object>> _errorsLookup = new Dictionary<string, IEnumerable<object>>();

        public bool HasErrors
        {
            get { return this.GetErrorsCore(string.Empty).Any(); }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            return this.GetErrorsCore(propertyName);
        }

        private IEnumerable<object> GetErrorsCore(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return this._errorsLookup.SelectMany(pair => pair.Value);

            return this._errorsLookup.TryGetValue(propertyName, out var error)
                ? error
                : Array.Empty<object>();
        }

        private void RaiseErrorsChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            ErrorsChanged?.Invoke(this,
                new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion

        private void ValidateCurrentFont()
        {
            this.ValidateProperty(() =>
            {
                var errors = new List<string>();

                var fontFilePath = this.FontFilePath;
                if (fontFilePath != null && !File.Exists(fontFilePath))
                    errors.Add(this._i18nValidationFontFileNotFound(fontFilePath));

                return errors;
            }, nameof(this.CurrentFont));
        }

        private void ValidateProperty(Func<IEnumerable<object>> getErrors, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Must be a property name.");

            bool lastHasErrors = this.HasErrors;
            this._errorsLookup.TryGetValue(propertyName, out var lastPropertyErrors);

            var currentPropertyErrors = getErrors();
            this._errorsLookup[propertyName] = currentPropertyErrors;
            bool currentHasErrors = this.HasErrors;

            if (!CompareErrors(lastPropertyErrors, currentPropertyErrors))
            {
                this.RaisePropertyChanged(nameof(this.HasErrors));

                this.RaiseErrorsChanged(propertyName);
                this.RaiseErrorsChanged(string.Empty);
            }
        }

        private static bool CompareErrors(IEnumerable<object> errors1, IEnumerable<object> errors2)
        {
            var cast1 = errors1?.Cast<object>() ?? Array.Empty<object>();
            var cast2 = errors2?.Cast<object>() ?? Array.Empty<object>();

            return cast1.Count() == cast2.Count() && !cast1.Except(cast2).Any() && !cast2.Except(cast1).Any();
        }

        protected class AsyncIndicator
        {
            private readonly Dictionary<GameFontType, bool> _isGeneratingFont = new()
            {
                { GameFontType.SmallFont, false },
                { GameFontType.DialogueFont, false },
                { GameFontType.SpriteText, false }
            };

            public event EventHandler IsRefreshingFontsChanged;
            public event EventHandler<GameFontType> IsGeneratingFontChanged;

            #region IsRefreshingFonts Property

            private bool _isRefreshingFonts;
            public bool IsRefreshingFonts
            {
                get => this._isRefreshingFonts;
                set
                {
                    if (this._isRefreshingFonts != value)
                    {
                        this._isRefreshingFonts = value;
                        IsRefreshingFontsChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            #endregion

            public bool IsGeneratingFont(GameFontType fontType)
            {
                lock (this._isGeneratingFont)
                    return this._isGeneratingFont[fontType];
            }

            public void UpdateIsGeneratingFont(GameFontType fontType, bool value)
            {
                lock (this._isGeneratingFont)
                {
                    bool lastValue = this._isGeneratingFont[fontType];
                    if (lastValue != value)
                    {
                        this._isGeneratingFont[fontType] = value;
                        IsGeneratingFontChanged?.Invoke(this, fontType);
                    }
                }
            }
        }
    }
}
