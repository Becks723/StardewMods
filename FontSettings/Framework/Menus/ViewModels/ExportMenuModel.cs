using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValleyUI.Mvvm;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class ExportMenuModel : MenuModelBase
    {
        private static readonly AsyncIndicator _asyncIndicator = new();

        private readonly IFontExporter _exporter;
        private readonly FontConfig _fontConfig;
        private readonly FontContext _context;
        private readonly FontFormat _fontFormat;
        private readonly ExportContextModel _staged;

        #region InXnb Property

        private bool _inXnb;
        public bool InXnb
        {
            get => this._inXnb;
            set
            {
                this.SetField(ref this._inXnb, value);

                this.RaisePropertyChanged(nameof(this.OutputExtensions));
            }
        }

        #endregion

        #region OutputName Property

        private string _outputName;
        public string OutputName
        {
            get => this._outputName;
            set => this.SetField(ref this._outputName, value);
        }

        #endregion

        #region OutputDirectory Property

        private string _outputDirectory;
        public string OutputDirectory
        {
            get => this._outputDirectory;
            set => this.SetField(ref this._outputDirectory, value);
        }

        #endregion

        #region OutputExtensions Property

        public string[] OutputExtensions
        {
            get
            {
                FontFormat format = this._fontFormat;
                bool xnb = this.InXnb;

                switch (format)
                {
                    case FontFormat.SpriteFont:
                        if (xnb)
                            return new[] { "xnb" };
                        else
                            return new[] { "json", "png" };

                    case FontFormat.BmFont:
                        if (xnb)
                            return new[] { "xnb" };
                        else
                            return new[] { "fnt", "png" };

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        #endregion

        #region XnbPlatform Property

        private XnbPlatform _xnbPlatform;
        public XnbPlatform XnbPlatform
        {
            get => this._xnbPlatform;
            set => this.SetField(ref this._xnbPlatform, value);
        }

        #endregion

        #region GameFramework Property

        private GameFramework _gameFramework;
        public GameFramework GameFramework
        {
            get => this._gameFramework;
            set => this.SetField(ref this._gameFramework, value);
        }

        #endregion

        #region GraphicsProfile Property

        private GraphicsProfile _graphicsProfile;
        public GraphicsProfile GraphicsProfile
        {
            get => this._graphicsProfile;
            set => this.SetField(ref this._graphicsProfile, value);
        }

        #endregion

        #region IsCompressed Property

        private bool _isCompressed;
        public bool IsCompressed
        {
            get => this._isCompressed;
            set => this.SetField(ref this._isCompressed, value);
        }

        #endregion

        #region BmFontPageWidth Property

        private int _bmFontPageWidth;
        public int BmFontPageWidth
        {
            get => this._bmFontPageWidth;
            set => this.SetField(ref this._bmFontPageWidth, value);
        }

        #endregion

        #region BmFontPageHeight Property

        private int _bmFontPageHeight;
        public int BmFontPageHeight
        {
            get => this._bmFontPageHeight;
            set => this.SetField(ref this._bmFontPageHeight, value);
        }

        #endregion

        #region IsExporting Property

        private bool _isExporting;
        public bool IsExporting
        {
            get => this._isExporting;
            set => this.SetField(ref this._isExporting, value);
        }

        #endregion

        public ICommand ExportCommand { get; }

        public ExportMenuModel(IFontExporter exporter, FontConfig fontConfig, FontContext context, ExportContextModel staged)
        {
            this._exporter = exporter;
            this._fontConfig = fontConfig;
            this._context = context;
            this._staged = staged;
            this._fontFormat = context.FontType != GameFontType.SpriteText
                ? FontFormat.SpriteFont
                : FontFormat.BmFont;

            PropertyChanged += this.StageValue;

            // Fill in default values (only first time)
            if (staged.IsFirstTime)
            {
                this.OutputDirectory = staged.OutputDirectory;  // 这个在初始化时就设置过默认值了
                this.InXnb = (this._fontFormat == FontFormat.SpriteFont);
                this.XnbPlatform = XnbPlatform.Windows;
                this.GameFramework = GameFramework.Monogame;
                this.GraphicsProfile = GraphicsProfile.HiDef;
                this.IsCompressed = true;

                staged.IsFirstTime = false;
            }

            // Fill in staged values
            else
            {
                this.OutputDirectory = staged.OutputDirectory;
                this.InXnb = staged.InXnb;
                this.XnbPlatform = staged.XnbPlatform;
                this.GameFramework = staged.GameFramework;
                this.GraphicsProfile = staged.GraphicsProfile;
                this.IsCompressed = staged.IsCompressed;
                this.BmFontPageWidth = staged.PageWidth;
                this.BmFontPageHeight = staged.PageHeight;
            }

            this.ExportCommand = new AsyncDelegateCommand(this.ExportAsync, () => !this.IsExporting, ex => ILog.Error($"Failed to export. {ex}"));
            this.OutputName = this.GetSuggestedOutputName();
        }

        public async Task ExportAsync()
        {
            using (_asyncIndicator.StartAsyncWork(x => this.IsExporting = x))
            {
                var settings = new FontExportSettings(
                    format: this._fontFormat,
                    inXnb: this.InXnb,
                    outputDirectory: this.OutputDirectory,
                    outputFileName: this.OutputName,
                    xnbPlatform: this.XnbPlatform,
                    gameFramework: this.GameFramework,
                    graphicsProfile: this.GraphicsProfile,
                    isCompressed: this.IsCompressed,
                    pageWidth: this.BmFontPageWidth,
                    pageHeight: this.BmFontPageHeight);

                var result = await this._exporter.Export(this._fontConfig, settings);

                if (result.IsSuccess)
                {
                    Game1.playSound("money");

                    string exportPath = new StringBuilder()
                        .Append(Path.Combine(this.OutputDirectory, this.OutputName))
                        .Append('.')
                        .AppendJoin('+', this.OutputExtensions)
                        .ToString();
                    ILog.Info(I18n.HudMessage_ExportSuccess(exportPath));
                }
                else
                {
                    Game1.playSound("cancel");

                    ILog.Error(I18n.HudMessage_ExportFail());
                    ILog.Error($"{result.GetError()}");
                }
            }
        }

        private string GetSuggestedOutputName()
        {
            string FontTypeName(FontContext context)
            {
                switch (context.FontType)
                {
                    case GameFontType.SmallFont: return "SmallFont";
                    case GameFontType.DialogueFont: return "SpriteFont1";
                    case GameFontType.SpriteText:
                        return FontHelpers.GetFontFileAssetName(context.Language).Substring(6);  // 跳过 'Fonts/'
                    default: throw new NotSupportedException();
                }
            }

            string DisplayFloat(float f)
            {
                string s = f.ToString("0.0");
                return s.TrimEnd('0').TrimEnd('.');
            }

            return new StringBuilder()
                .Append(FontTypeName(this._context))
                .Append('-')
                .Append(Path.GetFileNameWithoutExtension(this._fontConfig.FontFilePath))
                .Append('-')
                .Append(DisplayFloat(this._fontConfig.FontSize))
                .Append("px")
                .ToString();
        }

        private void StageValue(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.OutputDirectory):
                    this._staged.OutputDirectory = this.OutputDirectory;
                    break;
                case nameof(this.OutputName):
                    this._staged.OutputName = this.OutputName;
                    break;
                case nameof(this.InXnb):
                    this._staged.InXnb = this.InXnb;
                    break;
                case nameof(this.XnbPlatform):
                    this._staged.XnbPlatform = this.XnbPlatform;
                    break;
                case nameof(this.GameFramework):
                    this._staged.GameFramework = this.GameFramework;
                    break;
                case nameof(this.GraphicsProfile):
                    this._staged.GraphicsProfile = this.GraphicsProfile;
                    break;
                case nameof(this.IsCompressed):
                    this._staged.IsCompressed = this.IsCompressed;
                    break;
                case nameof(this.BmFontPageWidth):
                    this._staged.PageWidth = this.BmFontPageWidth;
                    break;
                case nameof(this.BmFontPageHeight):
                    this._staged.PageHeight = this.BmFontPageHeight;
                    break;
            }
        }

        private class AsyncIndicator
        {
            public IDisposable StartAsyncWork(Action<bool> busyIndicator)
                => new Disposable(busyIndicator).Start();

            private class Disposable : IDisposable
            {
                private readonly Action<bool> _busyIndicator;

                public Disposable(Action<bool> busyIndicator)
                {
                    this._busyIndicator = busyIndicator;
                }

                public Disposable Start()
                {
                    this._busyIndicator(true);
                    return this;
                }

                public void Dispose()
                {
                    this._busyIndicator(false);
                }
            }
        }
    }
}
