using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FontSettings.Framework.FontInfomation;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValleyUI.Mvvm;

namespace FontSettings.Framework.Menus
{
    internal class FontSettingsMenuModel : MenuModelBase
    {
        private static FontSettingsMenuModel _instance;

        private static readonly Dictionary<GameFontType, bool> _isGeneratingFont = new()
        {
            { GameFontType.SmallFont, false },
            { GameFontType.DialogueFont, false },
            { GameFontType.SpriteText, false }
        };
        private static readonly StagedValues _stagedValues = new();

        private readonly Dictionary<FontPresetFontType, FontPresetViewModel> _presetViewModels = new();
        private readonly ModConfig _config;
        private readonly FontManager _fontManager;
        private readonly IFontGenerator _sampleFontGenerator;
        private readonly IAsyncFontGenerator _sampleAsyncFontGenerator;
        private readonly IAsyncGameFontChanger _fontChanger;
        private readonly FontPresetManager _presetManager;
        private readonly Action<FontConfigs> _saveFontSettings;

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

        #region Title Property

        private string _title;

        public string Title
        {
            get => this._title;
            set => this.SetField(ref this._title, value);
        }

        #endregion

        #region ExamplesMerged Property

        private bool _exmaplesMerged;

        public bool ExamplesMerged
        {
            get => this._exmaplesMerged;
            set => this.SetField(ref this._exmaplesMerged, value);
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

        public string? ExampleText
        {
            get
            {
                string text = this._config.ExampleText;

                if (string.IsNullOrWhiteSpace(text))
                    text = this._config.Sample.GetTextForCurrentLangauge();

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

        private int _lineSpacing;

        public int LineSpacing
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

        private FontModel _currentFont;

        public FontModel CurrentFont
        {
            get => this._currentFont;
            set
            {
                this.SetField(ref this._currentFont, value);   // TODO: 取消注释。
                //this._currentFont = value;      // TODO: 删
                //this.RaisePropertyChanged();    // TODO: 删

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

        public string? FontFilePath => InstalledFonts.SimplifyPath(this.CurrentFont.FullPath);

        public int FontIndex => this.CurrentFont.FontIndex;

        public bool IsGeneratingFont => _isGeneratingFont[this.CurrentFontType];

        public bool CanGenerateFont => !this.IsGeneratingFont && this.IsCurrentPresetValid;

        #region AllFonts Property

        private ObservableCollection<FontModel> _allFonts;

        public ObservableCollection<FontModel> AllFonts
        {
            get => this._allFonts;
            set => this.SetField(ref this._allFonts, value);
        }

        #endregion

        public FontPreset? CurrentPreset => this.PresetViewModel(this.CurrentFontType).CurrentPreset;

        #region CurrentPresetName Property

        private string? _currentPresetName;

        public string? CurrentPresetName
        {
            get => this._currentPresetName;
            set => this.SetField(ref this._currentPresetName, value);
        }

        #endregion

        #region IsCurrentPresetValid Property

        private bool _isCurrentPresetValid;

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

        public ICommand MoveToPrevFont { get; }

        public ICommand MoveToNextFont { get; }

        public ICommand MoveToPrevPreset { get; }

        public ICommand MoveToNextPreset { get; }

        public ICommand SaveCurrentPreset { get; }

        public ICommand SaveCurrentAsNewPreset { get; }

        public ICommand DeleteCurrentPreset { get; }

        public ICommand RefreshFonts { get; }

        public FontSettingsMenuModel(ModConfig config, FontManager fontManager, IFontGenerator sampleFontGenerator, IAsyncFontGenerator sampleAsyncFontGenerator, IAsyncGameFontChanger fontChanger, FontPresetManager presetManager, Action<FontConfigs> saveFontSettings)
        {
            _instance = this;
            this._config = config;
            this._fontManager = fontManager;
            this._sampleFontGenerator = sampleFontGenerator;
            this._sampleAsyncFontGenerator = sampleAsyncFontGenerator;
            this._fontChanger = fontChanger;
            this._presetManager = presetManager;
            this._saveFontSettings = saveFontSettings;

            // 初始化子ViewModel。
            var presetFontTypes = new[] { FontPresetFontType.Small, FontPresetFontType.Medium, FontPresetFontType.Dialogue };
            foreach (var type in presetFontTypes)
            {
                var vm = new FontPresetViewModel(presetManager, type);
                vm.PresetChanged += this.OnPresetChanged;
                this._presetViewModels.Add(type, vm);
            }

            // 每当部分属性变化时，记录它们的值，以便再次打开菜单时填入。（记忆功能）
            this.RegisterCallbackToStageValues();

            // 填入之前记录的属性值。
            this.CurrentFontType = _stagedValues.FontType;
            this.ExamplesMerged = _stagedValues.ExamplesMerged;
            this.ShowExampleBounds = _stagedValues.ShowBounds;
            this.ShowExampleText = _stagedValues.ShowText;
            this.IsTuningCharOffset = _stagedValues.OffsetTuning;
            foreach (var pair in this._presetViewModels)
                pair.Value.CurrentPreset = _stagedValues.Presets[pair.Key];

            this.AllFonts = new ObservableCollection<FontModel>(this.LoadAllFonts());
            this.OnFontTypeChanged(this.CurrentFontType);

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

            // 初始化命令。
            this.MoveToPrevFont = new DelegateCommand(this.PreviousFontType);
            this.MoveToNextFont = new DelegateCommand(this.NextFontType);
            this.MoveToPrevPreset = new DelegateCommand(this.PreviousPreset);
            this.MoveToNextPreset = new DelegateCommand(this.NextPreset);
            this.SaveCurrentPreset = new DelegateCommand(this._SaveCurrentPreset);
            this.SaveCurrentAsNewPreset = new DelegateCommand<Func<IOverlayMenu>>(this._SaveCurrentAsNewPreset);
            this.DeleteCurrentPreset = new DelegateCommand(this._DeleteCurrentPreset);
            this.RefreshFonts = new DelegateCommand(this.RefreshAllFonts);
        }

        private void PreviousFontType()
        {
            this.CurrentFontType = this.CurrentFontType.Previous(LocalizedContentManager.CurrentLanguageLatin);
            this.OnFontTypeChanged(this.CurrentFontType);
        }

        private void NextFontType()
        {
            this.CurrentFontType = this.CurrentFontType.Next(LocalizedContentManager.CurrentLanguageLatin);
            this.OnFontTypeChanged(this.CurrentFontType);
        }

        private void PreviousPreset()
        {
            this.PresetViewModel(this.CurrentFontType).MoveToPreviousPreset();
        }

        private void NextPreset()
        {
            this.PresetViewModel(this.CurrentFontType).MoveToNextPreset();
        }

        private void _SaveCurrentPreset()
        {
            this.PresetViewModel(this.CurrentFontType).SaveCurrentPreset(
                this.FontFilePath, this.FontIndex, this.FontSize, this.Spacing, this.LineSpacing, this.CharOffsetX, this.CharOffsetY, this.PixelZoom);
        }

        private void _SaveCurrentAsNewPreset(Func<IOverlayMenu> createOverlay)
        {
            if (createOverlay == null) return;

            var overlay = createOverlay();
            if (overlay != null)
            {
                overlay.Open();
                overlay.Closed += (s, e) =>
                {
                    if (e.Parameter is string presetName)
                        this._SaveCurrentAsNewPreset(presetName);
                };
            }
        }

        private void _SaveCurrentAsNewPreset(string newPresetName)
        {
            this.PresetViewModel(this.CurrentFontType).SaveCurrentAsNewPreset(newPresetName,
                this.FontFilePath, this.FontIndex, this.FontSize, this.Spacing, this.LineSpacing, this.CharOffsetX, this.CharOffsetY, this.PixelZoom);
        }

        private void _DeleteCurrentPreset()
        {
            this.PresetViewModel(this.CurrentFontType).DeleteCurrentPreset();
        }

        private void RefreshAllFonts()
        {
            var lastFont = this.CurrentFont;  // 记录当前选中的字体。

            // 重新扫描本地字体文件。
            this.AllFonts = new ObservableCollection<FontModel>(this.LoadAllFonts(true));

            // 检查重新扫描后，之前选中的还在不在。
            bool match = false;
            foreach (FontModel font in this.AllFonts)
            {
                if (font == lastFont)
                {
                    match = true;
                    break;
                }
            }

            if (match)
                this.CurrentFont = lastFont;                // 如果还在，更新选中项。
            else
            {
                this.CurrentFont = this.AllFonts[0];  // 如果不在了，保持原版。
                this.UpdateExampleCurrent();                // 同时更新示例。
            }
        }

        public async Task<(GameFontType fontType, bool success)> TryGenerateFont()
        {
            FontConfigs fontSettings = this._config.Fonts;
            FontConfig config = fontSettings.GetOrCreateFontConfig(LocalizedContentManager.CurrentLanguageCode,
                FontHelpers.GetCurrentLocale(), this.CurrentFontType);

            FontConfig tempConfig = new FontConfig();
            config.CopyTo(tempConfig);

            tempConfig.Enabled = this.FontEnabled;
            tempConfig.FontFilePath = this.FontFilePath;
            tempConfig.FontIndex = this.FontIndex;
            tempConfig.FontSize = this.FontSize;
            tempConfig.Spacing = this.Spacing;
            tempConfig.LineSpacing = this.LineSpacing;
            tempConfig.CharOffsetX = this.CharOffsetX;
            tempConfig.CharOffsetY = this.CharOffsetY;
            tempConfig.PixelZoom = this.PixelZoom;

            var fontType = this.CurrentFontType;  // 这行是必要的，因为要确保异步前后是同一个字体。
            this.UpdateIsGeneratingFont(fontType, true);

            return await this._fontChanger.ChangeGameFontAsync(tempConfig).ContinueWith(task =>
            {
                // 外面的this 和 ContinueWith中的this 不一定是同一个实例。
                // 如：用户关闭了菜单，那么this为null；用户关闭又打开了菜单，那么this为另一个实例。
                // 在这里需要保证不为null，因此使用静态的_instance字段代替this。
                var instance = _instance;

                instance.UpdateIsGeneratingFont(fontType, false);

                bool success = task.Result;

                // 如果成功，更新配置值。
                if (success)
                {
                    tempConfig.CopyTo(config);
                    instance._saveFontSettings(fontSettings);
                }
                return (fontType, success);
            });
        }

        public void UpdateExampleVanilla()
        {
            if (this.CurrentFontType is GameFontType.SpriteText)
                this.ExampleVanillaFont = this._fontManager.GetBuiltInBmFont();
            else
                this.ExampleVanillaFont = new XNASpriteFont(
                    this._fontManager.GetBuiltInSpriteFont(this.CurrentFontType));
        }

        public void UpdateExampleCurrent()
        {
            string fontPath = this.FontFilePath != null 
                ? InstalledFonts.GetFullPath(this.FontFilePath)
                : null;

            var param = new SampleFontGeneratorParameter(
                Enabled: this.FontEnabled,
                FontFilePath: fontPath,
                FontSize: this.FontSize,
                Spacing: this.Spacing,
                LineSpacing: this.LineSpacing,
                SampleText: this.ExampleText,
                FontType: this.CurrentFontType,
                Language: FontHelpers.GetCurrentLanguage(),
                PixelZoom: this.PixelZoom,
                FontIndex: this.FontIndex,
                CharOffsetX: this.CharOffsetX,
                CharOffsetY: this.CharOffsetY);
            this.ExampleCurrentFont = this._sampleFontGenerator.GenerateFont(param);
        }

        private bool _isUpdatingExampleCurrent;
        private CancellationTokenSource _tokenSource;
        public async Task UpdateExampleCurrentAsync()
        {
            string fontPath = this.FontFilePath != null
                ? InstalledFonts.GetFullPath(this.FontFilePath)
                : null;
            var param = new SampleFontGeneratorParameter(
                Enabled: this.FontEnabled,
                FontFilePath: fontPath,
                FontSize: this.FontSize,
                Spacing: this.Spacing,
                LineSpacing: this.LineSpacing,
                SampleText: this.ExampleText,
                FontType: this.CurrentFontType,
                Language: FontHelpers.GetCurrentLanguage(),
                PixelZoom: this.PixelZoom,
                FontIndex: this.FontIndex,
                CharOffsetX: this.CharOffsetX,
                CharOffsetY: this.CharOffsetY);

            if (this._isUpdatingExampleCurrent)
            {
                this._tokenSource.Cancel();
                this._tokenSource.Dispose();
            }

            await this.RestartUpdateExampleCurrent(param);
        }

        private async Task RestartUpdateExampleCurrent(SampleFontGeneratorParameter param)
        {
            this._tokenSource = new CancellationTokenSource();
            var token = this._tokenSource.Token;

            var lastExampleCurrentFont = this.ExampleCurrentFont;
            this._isUpdatingExampleCurrent = true;
            try
            {
                this.ExampleCurrentFont = await this._sampleAsyncFontGenerator.GenerateFontAsync(param, token);
                ILog.Trace("Sample: Set");
            }
            catch (OperationCanceledException)
            {
                ILog.Trace("Sample: Cancelled");
                this.ExampleCurrentFont = lastExampleCurrentFont;
            }
            catch (Exception ex)
            {
                ILog.Trace($"Sample: {ex.Message}\n{ex.StackTrace}");
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
            // 更新标题。
            this.Title = newFontType.LocalizedName();

            // 更新各个属性。
            FontConfig fontConfig = this._config.Fonts.GetOrCreateFontConfig(LocalizedContentManager.CurrentLanguageCode,
                FontHelpers.GetCurrentLocale(), newFontType);
            this.FontEnabled = fontConfig.Enabled;
            this.FontSize = fontConfig.FontSize;
            this.Spacing = fontConfig.Spacing;
            this.LineSpacing = fontConfig.LineSpacing;
            this.CharOffsetX = fontConfig.CharOffsetX;
            this.CharOffsetY = fontConfig.CharOffsetY;
            this.CurrentFont = this.FindFont(fontConfig.FontFilePath, fontConfig.FontIndex);
            this.PixelZoom = fontConfig.PixelZoom;

            // 更新预设。
            this.OnPresetChanged(null, EventArgs.Empty);

            // 更新预览图。
            this.UpdateExampleVanilla();
            this.UpdateExampleCurrent();
        }

        private void OnPresetChanged(object sender, EventArgs e)
        {
            FontPreset? newPreset = this.PresetViewModel(this.CurrentFontType).CurrentPreset;

            // 更新预设名字。
            if (newPreset == null)
                this.CurrentPresetName = "-";
            else
                this.CurrentPresetName = newPreset.Name;

            // 更新几个状态：是否能保存、另存为、删除该预设。
            this.CanSaveCurrentPreset = this.CanSavePreset(newPreset);
            this.CanSaveCurrentAsNewPreset = this.CanSaveAsNewPreset(newPreset);
            this.CanDeleteCurrentPreset = this.CanDeletePreset(newPreset);

            // 如果无预设，则载入保存的设置。
            if (newPreset == null)
            {
                // 无预设时当然满足。
                this.IsCurrentPresetValid = true;

                FontConfig fontConfig = this._config.Fonts.GetOrCreateFontConfig(LocalizedContentManager.CurrentLanguageCode,
                    FontHelpers.GetCurrentLocale(), this.CurrentFontType);
                this.FontEnabled = fontConfig.Enabled;
                this.FontSize = fontConfig.FontSize;
                this.Spacing = fontConfig.Spacing;
                this.LineSpacing = fontConfig.LineSpacing;
                this.CharOffsetX = fontConfig.CharOffsetX;
                this.CharOffsetY = fontConfig.CharOffsetY;
                this.CurrentFont = this.FindFont(fontConfig.FontFilePath, fontConfig.FontIndex);
                this.PixelZoom = fontConfig.PixelZoom;

                this.UpdateExampleCurrent();
                return;
            }

            // 如果有预设，检查是否满足该预设的要求。
            this.IsCurrentPresetValid = this.IsPresetValid(newPreset, out FontModel fontMatched, out string message);
            this.MessageWhenPresetIsInvalid = message;
            // 满足，则填值。
            if (this.IsCurrentPresetValid)
            {
                this.FontEnabled = true;
                this.FontSize = newPreset.FontSize;
                this.Spacing = newPreset.Spacing;
                this.LineSpacing = newPreset.LineSpacing;
                this.CharOffsetX = newPreset.CharOffsetX;
                this.CharOffsetY = newPreset.CharOffsetY;
                this.CurrentFont = fontMatched;
                this.PixelZoom = newPreset.PixelZoom;

                this.UpdateExampleCurrent();
            }
        }

        private IEnumerable<FontModel> LoadAllFonts(bool rescan = false)  // rescan: 是否重新扫描本地字体。
        {
            if (rescan)
                InstalledFonts.Rescan();
            FontModel empty = new FontModel();
            FontModel[] fonts = InstalledFonts.GetAll().ToArray();
            return new FontModel[1] { empty }
                .Concat(fonts);
        }

        private FontModel FindFont(string fontFilePath, int fontIndex) // 这里path是简化后的
        {
            // 如果找不到字体文件，保持原版。
            if (fontFilePath == null
                || !InstalledFonts.TryGetFullPath(fontFilePath, out string fullPath))
                return this.AllFonts[0];

            return this.AllFonts.Where(f => f.FullPath == fullPath && f.FontIndex == fontIndex)
                .FirstOrDefault();
        }

        private bool IsPresetValid(FontPreset preset, out FontModel match, out string invalidMessage)
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

            foreach (FontModel font in allFonts)
            {
                if (keepOriginal)
                {
                    if (font.FullPath == null)
                    {
                        match = font;
                        return true;
                    }
                }
                else
                {
                    string name = Path.GetFileNameWithoutExtension(font.FullPath);
                    string extension = Path.GetExtension(font.FullPath);
                    if (name == specifiedName
                        && extension.Equals(specifiedExtension, StringComparison.OrdinalIgnoreCase)
                        && preset.FontIndex == font.FontIndex)
                    {
                        match = font;
                        return true;
                    }
                }
            }

            invalidMessage = $"当前预设不可用。需要安装字体文件：{preset.Requires.FontFileName}。";
            return false;
        }

        private bool CanSaveAsNewPreset(FontPreset? preset)
        {
            return true;
        }

        private bool CanSavePreset(FontPreset? preset)
        {
            return preset != null                                 // 无选中预设时不可。
                && !this._presetManager.IsBuiltInPreset(preset);  // 内置的预设不可编辑。
        }

        private bool CanDeletePreset(FontPreset? preset)
        {
            return preset != null                                 // 无选中预设时不可。
                && !this._presetManager.IsBuiltInPreset(preset);  // 内置的预设不可删除。
        }

        private FontPresetViewModel PresetViewModel(GameFontType fontType)
        {
            var key = fontType switch
            {
                GameFontType.SmallFont => FontPresetFontType.Small,
                GameFontType.DialogueFont => FontPresetFontType.Medium,
                GameFontType.SpriteText => FontPresetFontType.Dialogue,
                _ => throw new NotSupportedException(),
            };

            return this._presetViewModels[key];
        }

        private void UpdateIsGeneratingFont(GameFontType fontType, bool isGeneratingFont)
        {
            _isGeneratingFont[fontType] = isGeneratingFont;

            this.RaisePropertyChanged(nameof(this.IsGeneratingFont));
            this.RaisePropertyChanged(nameof(this.CanGenerateFont));
        }

        private void RegisterCallbackToStageValues()
        {
            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(this.CurrentFontType):
                        _stagedValues.FontType = this.CurrentFontType;
                        break;

                    case nameof(this.ExamplesMerged):
                        _stagedValues.ExamplesMerged = this.ExamplesMerged;
                        break;

                    case nameof(this.ShowExampleBounds):
                        _stagedValues.ShowBounds = this.ShowExampleBounds;
                        break;

                    case nameof(this.ShowExampleText):
                        _stagedValues.ShowText = this.ShowExampleText;
                        break;

                    case nameof(this.IsTuningCharOffset):
                        _stagedValues.OffsetTuning = this.IsTuningCharOffset;
                        break;
                }
            };

            foreach (var pair in this._presetViewModels)
            {
                var fontType = pair.Key;
                var vm = pair.Value;

                vm.PresetChanged += (s, e) =>
                {
                    _stagedValues.Presets[fontType] = vm.CurrentPreset;
                };
            }
        }

        private class StagedValues
        {
            public GameFontType FontType { get; set; } = GameFontType.SmallFont;
            public bool ExamplesMerged { get; set; } = false;
            public bool ShowBounds { get; set; } = false;
            public bool ShowText { get; set; } = true;
            public bool OffsetTuning { get; set; } = false;
            public Dictionary<FontPresetFontType, FontPreset?> Presets { get; } = new()
            {
                { FontPresetFontType.Small, null },
                { FontPresetFontType.Medium, null },
                { FontPresetFontType.Dialogue, null }
            };
        }
    }
}
