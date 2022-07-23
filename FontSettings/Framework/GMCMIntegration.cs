using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeShared.Integrations.GenericModConfigMenu;
using CodeShared.Integrations.GenericModConfigMenu.Options;
using FontSettings.Framework.FontInfomation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace FontSettings.Framework
{
    internal class GMCMIntegration : GenericModConfigMenuIntegrationBase
    {
        private readonly Func<ModConfig> _getConfig;

        private readonly FontExample _smallFontExample, _dialogueFontExample, _spriteTextExample;

        private ModConfig Config => this._getConfig();

        private readonly Dictionary<string, Func<string>> _fontDropDownOptions = InitDropDownOptions();

        private GenericModConfigMenuFluentHelper _gmcmHelper;

        public GMCMIntegration(Func<ModConfig> getConfig, RuntimeFontManager fontManager, GameFontChanger fontChanger, Action reset, Action save, IModRegistry modRegistry, IMonitor monitor, IManifest manifest)
            : base(modRegistry, monitor)
        {
            this._getConfig = getConfig;

            this._smallFontExample = new(GameFontType.SmallFont, fontManager, this.Config);
            this._dialogueFontExample = new(GameFontType.DialogueFont, fontManager, this.Config);
            this._spriteTextExample = new(GameFontType.SpriteText, fontManager, this.Config);

            this.InitFields(reset, () => this.Save(save, fontChanger, getConfig()), manifest);
        }

        protected override void IntegrateOverride(GenericModConfigMenuFluentHelper helper)
        {
            this._gmcmHelper = helper;

            // 当语言更改时，重新注册GMCM选项。
            LocalizedContentManager.OnLanguageChange += code =>
            {
                this.Reregister(helper);

                this._smallFontExample.OnLanguageChanged();
                this._dialogueFontExample.OnLanguageChanged();
                this._spriteTextExample.OnLanguageChanged();
            };

            this.RegisterOptions(helper);
        }

        private void Reregister(GenericModConfigMenuFluentHelper helper)
        {
            helper.Unregister();
            this.RegisterOptions(helper);
        }

        private void RegisterOptions(GenericModConfigMenuFluentHelper helper)
        {
            helper.Register()
                .AddTextBox(
                    name: I18n.Config_ExampleText,
                    tooltip: I18n.Config_ExampleText_Description,
                    get: () => this.NormalizeExampleText(this.Config.ExampleText),
                    set: val => this.Config.ExampleText = this.ParseBackExampleText(val),
                    fieldChanged: val =>
                    {
                        this.Example(GameFontType.SmallFont).ExampleText = this.ParseBackExampleText(val);
                        this.Example(GameFontType.DialogueFont).ExampleText = this.ParseBackExampleText(val);
                        this.Example(GameFontType.SpriteText).ExampleText = this.ParseBackExampleText(val);
                    }
                )

                // max font size
                .AddSlider(
                    name: I18n.Config_MaxFontSize,
                    tooltip: I18n.Config_MaxFontSize_Description,
                    get: () => this.Config.MaxFontSize,
                    set: val => this.Config.MaxFontSize = val,
                    max: 150,
                    min: 50,
                    interval: 1
                )

                // max spacing
                .AddSlider(
                    name: I18n.Config_MaxSpacing,
                    tooltip: I18n.Config_MaxSpacing_Description,
                    get: () => this.Config.MaxSpacing,
                    set: val => this.Config.MaxSpacing = val,
                    max: 20,
                    min: 5,
                    interval: 1
                )

                // min spacing
                .AddSlider(
                    name: I18n.Config_MinSpacing,
                    tooltip: I18n.Config_MinSpacing_Description,
                    get: () => this.Config.MinSpacing,
                    set: val => this.Config.MinSpacing = val,
                    max: -5,
                    min: -20,
                    interval: 1
                )

                // max line spacing
                .AddSlider(
                    name: I18n.Config_MaxLineSpacing,
                    tooltip: I18n.Config_MaxLineSpacing_Description,
                    get: () => this.Config.MaxLineSpacing,
                    set: val => this.Config.MaxLineSpacing = val,
                    max: 150,
                    min: 50,
                    interval: 1
                )

                // max x offset
                .AddSlider(
                    name: I18n.Config_MaxOffsetX,
                    tooltip: I18n.Config_MaxOffsetX_Description,
                    get: () => this.Config.MaxCharOffsetX,
                    set: val => this.Config.MaxCharOffsetX = val,
                    max: 25,
                    min: 5,
                    interval: 1
                )

                // min x offset
                .AddSlider(
                    name: I18n.Config_MinOffsetX,
                    tooltip: I18n.Config_MinOffsetX_Description,
                    get: () => this.Config.MinCharOffsetX,
                    set: val => this.Config.MinCharOffsetX = val,
                    max: -5,
                    min: -25,
                    interval: 1
                )

                // max y offset
                .AddSlider(
                    name: I18n.Config_MaxOffsetY,
                    tooltip: I18n.Config_MaxOffsetY_Description,
                    get: () => this.Config.MaxCharOffsetY,
                    set: val => this.Config.MaxCharOffsetY = val,
                    max: 25,
                    min: 5,
                    interval: 1
                )

                // min y offset
                .AddSlider(
                    name: I18n.Config_MinOffsetY,
                    tooltip: I18n.Config_MinOffsetY_Description,
                    get: () => this.Config.MinCharOffsetY,
                    set: val => this.Config.MinCharOffsetY = val,
                    max: -5,
                    min: -25,
                    interval: 1
                )
                .AddLineSpacing()
                .AddSectionTitle(I18n.Config_Section_Fonts, I18n.Config_Section_Fonts_Description);

            this.AddFontSettings(helper, GameFontType.SmallFont);
            helper.AddLineSpacing();
            this.AddFontSettings(helper, GameFontType.DialogueFont);
            helper.AddLineSpacing();
            if (!LocalizedContentManager.CurrentLanguageLatin)
            {
                this.AddFontSettings(helper, GameFontType.SpriteText);
                helper.AddLineSpacing();
            }
        }

        private void AddFontSettings(GenericModConfigMenuFluentHelper helper, GameFontType fontType)
        {
            FontConfig Font() => this.Config.Fonts.GetOrCreateFontConfig(LocalizedContentManager.CurrentLanguageCode, FontHelpers.GetCurrentLocale(), fontType);
            FontExample Example() => this.Example(fontType);

            helper
                .AddSectionTitle(() => fontType.LocalizedName())
                .AddCustom(() => string.Empty, null, Example())

                // 启用
                .AddCheckbox(
                    name: I18n.Config_Fonts_Enabled,
                    tooltip: I18n.Config_Fonts_Enabled_Description,
                    get: () => Font().Enabled,
                    set: val => Font().Enabled = val,
                    fieldChanged: val => Example().UpdateEnabled(val)
                )

                // 字体
                .AddDropDown(
                    name: I18n.Config_Fonts_Font,
                    tooltip: I18n.Config_Fonts_Font_Description,
                    get: () => BuildFontDropDownOption(Font().FontFilePath, Font().FontIndex),
                    set: val => this.SetFontFileAndIndex(Font(), val),
                    choices: this._fontDropDownOptions.Keys.ToArray(),
                    formattedChoices: key => this._fontDropDownOptions[key](),
                    fieldChanged: val =>
                    {
                        this.SeperateFontFileAndIndex(val, out string fontFilePath, out int fontIndex);
                        Example().UpdateFontFile(fontFilePath);
                        Example().UpdateFontIndex(fontIndex);
                    }
                )

                // 大小
                .AddSlider(
                    name: I18n.Config_Fonts_FontSize,
                    tooltip: I18n.Config_Fonts_FontSize_Description,
                    get: () => Font().FontSize,
                    set: val => Font().FontSize = val,
                    min: this.Config.MinFontSize,
                    max: this.Config.MaxFontSize,
                    interval: 1,
                    fieldChanged: val => Example().UpdateFontSize(val)
                )

                // 字间距
                .AddSlider(
                    name: I18n.Config_Fonts_Spacing,
                    tooltip: I18n.Config_Fonts_Spacing_Description,
                    get: () => Font().Spacing,
                    set: val => Font().Spacing = val,
                    min: this.Config.MinSpacing,
                    max: this.Config.MaxSpacing,
                    interval: 1,
                    fieldChanged: val => Example().UpdateSpacing(val)
                )

                // 行间距
                .AddSlider(
                    name: I18n.Config_Fonts_LineSpacing,
                    tooltip: I18n.Config_Fonts_LineSpacing_Description,
                    get: () => Font().LineSpacing,
                    set: val => Font().LineSpacing = val,
                    min: this.Config.MinLineSpacing,
                    max: this.Config.MaxLineSpacing,
                    interval: 1,
                    fieldChanged: val => Example().UpdateLineSpacing(val)
                );
        }

        private async void Save(Action save, GameFontChanger fontChanger, ModConfig config)
        {
            // 配置文件。
            save();
            this.Reregister(this._gmcmHelper);  // 涉及到控件的极值，因此重新注册。

            // 替换字体。
            bool anyFailed = false;
            foreach (GameFontType fontType in Enum.GetValues<GameFontType>())
            {
                if (LocalizedContentManager.CurrentLanguageLatin && fontType is GameFontType.SpriteText)
                    continue;

                FontConfig fontConfig = config.Fonts.GetOrCreateFontConfig(
                    LocalizedContentManager.CurrentLanguageCode,
                    FontHelpers.GetCurrentLocale(),
                    fontType);
                bool success = await fontChanger.ReplaceOriginalOrRemainAsync(fontConfig);
                if (success)
                {
                    Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_SuccessSetFont(fontType.LocalizedName()), null));
                    save?.Invoke();
                }
                else
                {
                    anyFailed = true;
                    Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_FailedSetFont(fontType.LocalizedName()), HUDMessage.error_type));
                }
            }

            if (anyFailed)
                Game1.playSound("cancel");
        }

        private FontExample Example(GameFontType fontType) => fontType switch
        {
            GameFontType.SmallFont => this._smallFontExample,
            GameFontType.DialogueFont => this._dialogueFontExample,
            GameFontType.SpriteText => this._spriteTextExample,
            _ => throw new NotSupportedException()
        };

        private string NormalizeExampleText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("\n", "\\n");
        }

        private string ParseBackExampleText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("\\n", "\n");
        }

        private void SetFontFileAndIndex(FontConfig config, string value)
        {
            this.SeperateFontFileAndIndex(value, out string fontFile, out int fontIndex);
            config.FontFilePath = fontFile;
            config.FontIndex = fontIndex;
        }

        private void SeperateFontFileAndIndex(string value, out string fontFilePath, out int fontIndex)
        {
            string[] args = value.Split('/');
            fontFilePath = args[0];
            if (fontFilePath is "null") fontFilePath = null;
            fontIndex = int.Parse(args[1]);
        }

        private static Dictionary<string, Func<string>> InitDropDownOptions()
        {
            FontModel[] fonts = InstalledFonts.GetAll().ToArray();
            Dictionary<string, Func<string>> result = new(fonts.Length + 1);
            result.Add(BuildFontDropDownOption(null, 0), I18n.OptionsPage_Font_KeepOrig);
            foreach (FontModel font in fonts)
            {
                string key = BuildFontDropDownOption(
                    InstalledFonts.SimplifyPath(font.FullPath),
                    font.FontIndex
                );
                Func<string> value = () => $"{font.FamilyName} ({font.SubfamilyName})";

                result[key] = value;  // 这里不用Add方法，因为有可能key会重复，直接覆盖即可。
            }
            return result;
        }

        private static string BuildFontDropDownOption(string fontFile, int fontIndex)
        {
            if (fontFile is null)
            {
                return $"null/{fontIndex}";
            }

            fontFile = fontFile.Replace('/', '\\');
            return $"{fontFile}/{fontIndex}";
        }

        private class FontExample : BaseCustomOption
        {
            private readonly GameFontType _fontType;
            private readonly RuntimeFontManager _fontManager;
            private readonly ModConfig _config;
            private ISpriteFont _gameFont;

            private ISpriteFont _customFont;

            private string _exampleText;

            private readonly ExampleFonts _exampleFonts;

            private readonly Color _vanillaColor = Color.Gray * 0.33f;
            private readonly Color _customColor = Game1.textColor;

            private bool _lastEnabled;
            private string _lastFontFile;
            private int _lastFontIndex;
            private float _lastFontSize;
            private float _lastSpacing;
            private int _lastLineSpacing;

            public string ExampleText
            {
                get => this._exampleText;
                set
                {
                    if (this._fontType is GameFontType.SpriteText)
                        value = value.Replace("\n", "^");

                    if (this._exampleText != value)
                    {
                        this._exampleText = value;
                        this._customFont = this.CustomFont();
                    }
                }
            }

            public override int Height => 200;

            public FontExample(GameFontType fontType, RuntimeFontManager fontManager, ModConfig config)
            {
                this._fontType = fontType;
                this._fontManager = fontManager;
                this._config = config;
                this._exampleFonts = new(fontManager);

                // 初始化各个属性和自定义字体。
                this.ExampleText = config.ExampleText;
                this.OnLanguageChanged();
            }

            public void UpdateEnabled(bool newEnabled)
            {
                if (this._lastEnabled == newEnabled) return;

                this._lastEnabled = newEnabled;
                this._customFont = this.CustomFont();
            }

            public void UpdateFontFile(string newFontFilePath)
            {
                if (this._lastFontFile == newFontFilePath) return;

                this._lastFontFile = newFontFilePath;
                this._customFont = this.CustomFont();
            }

            public void UpdateFontIndex(int newFontIndex)
            {
                if (this._lastFontIndex == newFontIndex) return;

                this._lastFontIndex = newFontIndex;
                this._customFont = this.CustomFont();
            }

            public void UpdateFontSize(float newFontSize)
            {
                if (this._lastFontSize == newFontSize) return;

                this._lastFontSize = newFontSize;
                this._customFont = this.CustomFont();
            }

            public void UpdateSpacing(float newSpacing)
            {
                if (this._lastSpacing == newSpacing) return;

                this._lastSpacing = newSpacing;
                this._customFont = this.CustomFont();
            }

            public void UpdateLineSpacing(int newLineSpacing)
            {
                if (this._lastLineSpacing == newLineSpacing) return;

                this._lastLineSpacing = newLineSpacing;
                this._customFont = this.CustomFont();
            }

            public void OnLanguageChanged()
            {
                FontConfig fontConfig = this._config.Fonts.GetOrCreateFontConfig(LocalizedContentManager.CurrentLanguageCode, FontHelpers.GetCurrentLocale(), this._fontType);
                this._lastEnabled = fontConfig.Enabled;
                this._lastFontFile = fontConfig.FontFilePath;
                this._lastFontIndex = fontConfig.FontIndex;
                this._lastFontSize = fontConfig.FontSize;
                this._lastSpacing = fontConfig.Spacing;
                this._lastLineSpacing = fontConfig.LineSpacing;
                this._gameFont = this.VanillaFont();
                this._customFont = this.CustomFont();
            }

            public override void Draw(SpriteBatch b, Vector2 drawOrigin)
            {
                Rectangle scissor = b.GraphicsDevice.ScissorRectangle;
                Rectangle bounds = new Rectangle(scissor.X, (int)drawOrigin.Y, scissor.Width, this.Height);
                Vector2 vanillaExampleSize = this._gameFont.MeasureString(this.ExampleText);
                Vector2 exampleOrigin;
                exampleOrigin.X = bounds.X + bounds.Width / 2 - vanillaExampleSize.X / 2;
                exampleOrigin.Y = bounds.Y + bounds.Height / 2 - vanillaExampleSize.Y / 2;

                string vanillaText = I18n.OptionsPage_OriginalExample();
                string customText = I18n.OptionsPage_CustomExample();
                int borderWidth = IClickableMenu.borderWidth;
                Vector2 vanillaLabelColorSize = new Vector2(20, 20);
                Vector2 vanillaLabelTextSize = Game1.dialogueFont.MeasureString(vanillaText);
                Vector2 customLabelColorSize = new Vector2(20, 20);
                Vector2 customLabelTextSize = Game1.dialogueFont.MeasureString(customText);
                Vector2 vanillaLabelColorOrigin;
                vanillaLabelColorOrigin.X = bounds.Right - borderWidth - vanillaLabelTextSize.X - borderWidth / 5 - vanillaLabelColorSize.X;
                vanillaLabelColorOrigin.Y = bounds.Bottom - borderWidth - customLabelTextSize.Y - borderWidth / 4 - vanillaLabelTextSize.Y + vanillaLabelTextSize.Y / 2 - vanillaLabelColorSize.Y / 2;
                Vector2 vanillaLabelTextOrigin;
                vanillaLabelTextOrigin.X = bounds.Right - borderWidth - vanillaLabelTextSize.X;
                vanillaLabelTextOrigin.Y = bounds.Bottom - borderWidth - customLabelTextSize.Y - borderWidth / 4 - vanillaLabelTextSize.Y;
                Vector2 customLabelColorOrigin;
                customLabelColorOrigin.X = bounds.Right - borderWidth - vanillaLabelTextSize.X - borderWidth / 5 - customLabelColorSize.X;
                customLabelColorOrigin.Y = bounds.Bottom - borderWidth - customLabelTextSize.Y + customLabelTextSize.Y / 2 - customLabelColorSize.Y / 2;
                Vector2 customLabelTextOrigin;
                customLabelTextOrigin.X = bounds.Right - borderWidth - vanillaLabelTextSize.X;
                customLabelTextOrigin.Y = bounds.Bottom - borderWidth - customLabelTextSize.Y;

                // 背景框
                //IClickableMenu.drawTextureBox(b, bounds.X, bounds.Y, bounds.Width, bounds.Height, Color.White);
                IClickableMenu.drawTextureBox(b, Game1.menuTexture, new(64, 320, 60, 60), bounds.X, bounds.Y, bounds.Width, bounds.Height, Color.White);

                // 图例标注
                b.Draw(Game1.staminaRect, new Rectangle((int)vanillaLabelColorOrigin.X, (int)vanillaLabelColorOrigin.Y, (int)vanillaLabelColorSize.X, (int)vanillaLabelColorSize.Y), this._vanillaColor);
                b.Draw(Game1.staminaRect, new Rectangle((int)customLabelColorOrigin.X, (int)customLabelColorOrigin.Y, (int)customLabelColorSize.X, (int)customLabelColorSize.Y), this._customColor);
                b.DrawString(Game1.dialogueFont, vanillaText, vanillaLabelTextOrigin, Game1.textColor);
                b.DrawString(Game1.dialogueFont, customText, customLabelTextOrigin, Game1.textColor);

                // 示例文本
                this._gameFont?.Draw(b, this.ExampleText, exampleOrigin, this._vanillaColor);
                this._customFont?.Draw(b, this.ExampleText, exampleOrigin, this._customColor);
            }

            private ISpriteFont VanillaFont()
            {
                if (LocalizedContentManager.CurrentLanguageLatin && this._fontType is GameFontType.SpriteText) return null;

                if (this._fontType is GameFontType.SpriteText)
                    return this._fontManager.GetBuiltInBmFont();
                else
                    return new XNASpriteFont(this._fontManager.GetBuiltInSpriteFont(this._fontType));
            }

            private ISpriteFont CustomFont()
            {
                if (LocalizedContentManager.CurrentLanguageLatin && this._fontType is GameFontType.SpriteText) return null;

                return this._exampleFonts.ResetThenGet(this._fontType,
                     this._lastEnabled,
                     this._lastFontFile,
                     this._lastFontIndex,
                     (int)this._lastFontSize,
                     (int)this._lastSpacing,
                     this._lastLineSpacing,
                     Vector2.Zero,
                     this.ExampleText
                 );
            }
        }

        private class FontSelector : BaseCustomOption
        {
            private readonly ClickableTextureComponent _leftArrow, _rightArrow;

            private Rectangle _bounds;

            private Vector2 _headerOrigin;

            public GameFontType CurrentFontType { get; private set; }

            public FontSelector()
            {
                this._leftArrow = new(new Rectangle(0, 0, 24, 32), Game1.mouseCursors, new(480, 96, 24, 32), 1f);
                this._rightArrow = new(new Rectangle(0, 0, 24, 32), Game1.mouseCursors, new(448, 96, 24, 32), 1f);
            }

            public override int Height => 40;

            public override void OnMenuOpening()
            {
                ModEntry.ModHelper.Events.Input.ButtonPressed += this.OnButtonPressed;
                ModEntry.ModHelper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            }


            public override void OnMenuClosing()
            {
                ModEntry.ModHelper.Events.Input.ButtonPressed -= this.OnButtonPressed;
                ModEntry.ModHelper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            }

            private void OnButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
            {
                if (e.Button is SButton.MouseLeft)
                {
                    Point point = e.Cursor.GetScaledScreenPixels().ToPoint();
                    if (this._leftArrow.containsPoint(point.X, point.Y))
                    {
                        this._leftArrow.scale -= 0.25f;
                        this._leftArrow.scale = Math.Max(0.75f, this._leftArrow.baseScale);
                        Game1.playSound("smallSelect");

                        this.CurrentFontType = this.PreviousFont(this.CurrentFontType);
                    }
                    else if (this._rightArrow.containsPoint(point.X, point.Y))
                    {
                        this._rightArrow.scale -= 0.25f;
                        this._rightArrow.scale = Math.Max(0.75f, this._rightArrow.baseScale);
                        Game1.playSound("smallSelect");

                        this.CurrentFontType = this.NextFont(this.CurrentFontType);
                    }
                }
            }

            private void OnUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
            {
                Point mousePos = Game1.getMousePosition();

                this.PerformHoverAction(mousePos.X, mousePos.Y);
            }

            private void OnCursorMoved(object sender, StardewModdingAPI.Events.CursorMovedEventArgs e)
            {
                Point point = e.NewPosition.GetScaledScreenPixels().ToPoint();

                this._leftArrow.tryHover(point.X, point.Y);
                this._rightArrow.tryHover(point.X, point.Y);
            }

            private void PerformHoverAction(int x, int y)
            {
                this._leftArrow.tryHover(x, y);
                this._rightArrow.tryHover(x, y);
            }

            public override void Draw(SpriteBatch b, Vector2 drawOrigin)
            {
                Rectangle scissor = b.GraphicsDevice.ScissorRectangle;
                this._bounds = new Rectangle(scissor.X, (int)drawOrigin.Y, scissor.Width, this.Height);
                this.UpdatePositions();

                b.Draw(Game1.staminaRect, this._bounds, new Color(253, 188, 110));
                this._leftArrow.draw(b);
                this._rightArrow.draw(b);
                DrawString(this.CurrentFontType, b, this.CurrentFontType.LocalizedName(), this._headerOrigin, Game1.textColor);
            }

            private void UpdatePositions()
            {
                this._leftArrow.bounds.X = this._bounds.X + this._bounds.Width / 5;
                this._leftArrow.bounds.Y = this._bounds.Y + this._bounds.Height / 2 - this._leftArrow.bounds.Height / 2;
                this._rightArrow.bounds.X = this._bounds.Right - this._bounds.Width / 5 - this._rightArrow.bounds.Width;
                this._rightArrow.bounds.Y = this._bounds.Y + this._bounds.Height / 2 - this._rightArrow.bounds.Height / 2;

                string header = this.CurrentFontType.LocalizedName();
                Vector2 headerSize = MeasureString(this.CurrentFontType, header);
                this._headerOrigin.X = this._bounds.X + this._bounds.Width / 2 - headerSize.X / 2;
                this._headerOrigin.Y = this._bounds.Y + this._bounds.Height / 2 - headerSize.Y / 2;
            }

            private GameFontType PreviousFont(GameFontType current)
            {
                if (LocalizedContentManager.CurrentLanguageLatin)
                    return current switch
                    {
                        GameFontType.SmallFont => GameFontType.DialogueFont,
                        GameFontType.DialogueFont => GameFontType.SmallFont,
                        GameFontType.SpriteText => GameFontType.SmallFont,
                        _ => throw new NotSupportedException()
                    };
                else
                    return current switch
                    {
                        GameFontType.SmallFont => GameFontType.SpriteText,
                        GameFontType.DialogueFont => GameFontType.SmallFont,
                        GameFontType.SpriteText => GameFontType.DialogueFont,
                        _ => throw new NotSupportedException()
                    };
            }

            private GameFontType NextFont(GameFontType current)
            {
                if (LocalizedContentManager.CurrentLanguageLatin)
                    return current switch
                    {
                        GameFontType.SmallFont => GameFontType.DialogueFont,
                        GameFontType.DialogueFont => GameFontType.SmallFont,
                        GameFontType.SpriteText => GameFontType.SmallFont,
                        _ => throw new NotSupportedException()
                    };
                else
                    return current switch
                    {
                        GameFontType.SmallFont => GameFontType.DialogueFont,
                        GameFontType.DialogueFont => GameFontType.SpriteText,
                        GameFontType.SpriteText => GameFontType.SmallFont,
                        _ => throw new NotSupportedException()
                    };
            }

            private static Vector2 MeasureString(GameFontType fontType, string s)
            {
                return fontType switch
                {
                    GameFontType.SmallFont => Game1.smallFont.MeasureString(s),
                    GameFontType.DialogueFont => Game1.dialogueFont.MeasureString(s),
                    GameFontType.SpriteText => new Vector2(
                        StardewValley.BellsAndWhistles.SpriteText.getWidthOfString(s),
                        StardewValley.BellsAndWhistles.SpriteText.getHeightOfString(s)),
                    _ => throw new NotSupportedException(),
                };
            }

            private static void DrawString(GameFontType fontType, SpriteBatch b, string s, Vector2 position, Color c)
            {
                switch (fontType)
                {
                    case GameFontType.SmallFont:
                        b.DrawString(Game1.smallFont, s, position, c);
                        break;
                    case GameFontType.DialogueFont:
                        b.DrawString(Game1.dialogueFont, s, position, c);
                        break;
                    case GameFontType.SpriteText:
                        StardewValley.BellsAndWhistles.SpriteText.drawString(b, s, (int)position.X, (int)position.Y);
                        break;
                }
            }
        }
    }
}
