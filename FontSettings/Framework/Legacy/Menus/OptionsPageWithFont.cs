using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FontSettings.Framework.FontInfomation;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace FontSettings.Framework.Menus
{
    internal class OptionsPageWithFont : OptionsPage, IDisposable
    {
        private readonly OptionsPage _optionsPage;

        private static ModConfig _config;
        private static RuntimeFontManager _fontManager;
        private static GameFontChanger _fontChanger;
        private static Action<ModConfig> _saveConfig;
        private static ExampleFonts _exampleFonts;

        private OptionsFontSelector _fontSelector;

        private OptionsFontExample _originalExample, _customExample;

        private OptionsCheckBox _enableCheckbox;

        private OptionsDropDown _fontDropDown;

        private OptionsSlider<int> _fontSizeSlider;

        private OptionsSlider<int> _spacingSlider;

        private OptionsSlider<int> _lineSpacingSlider;

        // static是因为菜单关闭再打开后可能依旧在生成字体。
        private static readonly Lazy<Dictionary<GameFontType, OptionsOkButton>> _okButtons = new(() => new(3)
        {
            { GameFontType.SmallFont, new OptionsOkButton() },
            { GameFontType.DialogueFont, new OptionsOkButton() },
            { GameFontType.SpriteText, new OptionsOkButton() }
        });

        private OptionsOkButton CurrentOkButton => _okButtons.Value[this._fontSelector.CurrentFont];

        private bool _lastEnabled;
        private string _lastFontFilePath;
        private int _lastFontSize;
        private int _lastSpacing;
        private int _lastLineSpacing;
        private string _lastExistingFontPath;

        public static void Initialize(ModConfig config, RuntimeFontManager fontManager, GameFontChanger fontChanger, Action<ModConfig> saveConfig)
        {
            _config = config;
            _fontManager = fontManager;
            _fontChanger = fontChanger;
            _saveConfig = saveConfig;
            _exampleFonts = new ExampleFonts(fontManager);
        }

        public static void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Constructor(typeof(GameMenu), new[] { typeof(bool) }),
                postfix: new HarmonyMethod(typeof(OptionsPageWithFont), nameof(GameMenu_ctor_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(OptionsPage), "setScrollBarToCurrentIndex"),
                postfix: new HarmonyMethod(typeof(OptionsPageWithFont), nameof(OptionsPage_setScrollBarToCurrentIndex_Prefix))
            );
        }

        private static void GameMenu_ctor_Postfix(GameMenu __instance)
        {
            int index = __instance.pages.FindIndex(menu => menu is OptionsPage);
            OptionsPage orig = __instance.pages[index] as OptionsPage;
            OptionsPageWithFont newOptPage = new OptionsPageWithFont(orig);
            __instance.pages[index] = newOptPage;
            newOptPage.AddFontSection();
        }

        private static void OptionsPage_setScrollBarToCurrentIndex_Prefix(OptionsPage __instance, ClickableTextureComponent ___scrollBar, Rectangle ___scrollBarRunner, ClickableTextureComponent ___upArrow, ClickableTextureComponent ___downArrow)
        {
            if (__instance.options.Count > 0)
            {
                // 使滑块在整个滑槽中的位置更精确。（游戏内表现为滑块可能滑不到底）
                ___scrollBar.bounds.Y = (int)((___scrollBarRunner.Height - ___scrollBar.bounds.Height) / Math.Max(1f, __instance.options.Count - 7 + 1) * __instance.currentItemIndex + ___upArrow.bounds.Bottom + 4);
                if (___scrollBar.bounds.Y > ___downArrow.bounds.Y - ___scrollBar.bounds.Height - 4)
                    ___scrollBar.bounds.Y = ___downArrow.bounds.Y - ___scrollBar.bounds.Height - 4;

                // 如果已经滑到最底部，确保滑块也到了最底部。 
                if (__instance.currentItemIndex + 7 == __instance.options.Count)
                    ___scrollBar.bounds.Y = ___downArrow.bounds.Y - ___scrollBar.bounds.Height - 4;
            }
        }

        public OptionsPageWithFont(OptionsPage optionsPage)
            : base(optionsPage.xPositionOnScreen, optionsPage.yPositionOnScreen, optionsPage.width, optionsPage.height)
        {
            this.options.Clear();
            this.options.AddRange(optionsPage.options);

            this._optionsPage = optionsPage;
        }

        public void AddFontSection()
        {
            GameFontType fontType = GameFontType.SmallFont;
            FontConfig config = _config.Fonts.GetOrCreateFontConfig(LocalizedContentManager.CurrentLanguageCode,
                FontHelpers.GetCurrentLocale(), fontType);

            var title = new OptionsElement(I18n.OptionsPage_FontHeader());

            this._fontSelector = new OptionsFontSelector(LocalizedContentManager.CurrentLanguageLatin, fontType);
            this._fontSelector.SelectionChanged += this.FontTypeChanged;

            this._originalExample = new OptionsFontExample(I18n.OptionsPage_OriginalExample(), this._optionsPage.optionSlots[0].bounds.Width, this._optionsPage.optionSlots[0].bounds.Height);
            this._customExample = new OptionsFontExample(I18n.OptionsPage_CustomExample(), this._optionsPage.optionSlots[0].bounds.Width, this._optionsPage.optionSlots[0].bounds.Height);

            this._enableCheckbox = new OptionsCheckBox(I18n.OptionsPage_Enable(), config.Enabled);
            this._enableCheckbox.ValueChanged += this.EnableCheckbox_ValueChanged;

            this._fontDropDown = new OptionsDropDown(I18n.OptionsPage_Font())
            {
                dropDownOptions = this.GetFontDropDownOptions(),
                dropDownDisplayOptions = this.GetFontDropDownDisplayOptions(),
            };
            this._fontDropDown.SelectionChanged += this.FontDropDownChanged;

            this._fontSizeSlider = new OptionsSlider<int>(I18n.OptionsPage_FontSize())
            {
                MinValue = 1,  // 目前暂定最大字号100px，最小字号1px。
                MaxValue = 100,
                Interval = 1,
                Value = (int)config.FontSize
            };
            this._fontSizeSlider.ValueChanged += this.FontSizeSlider_ValueChanged;

            this._spacingSlider = new OptionsSlider<int>(I18n.OptionsPage_Spacing())
            {
                MinValue = -10,
                MaxValue = 10,
                Interval = 1,
                Value = (int)config.Spacing
            };
            this._spacingSlider.ValueChanged += this.SpacingSlider_ValueChanged;

            this._lineSpacingSlider = new OptionsSlider<int>(I18n.OptionsPage_LineSpacing())
            {
                MinValue = 1,
                MaxValue = 100,
                Interval = 1,
                Value = config.LineSpacing
            };
            this._lineSpacingSlider.ValueChanged += this.LineSpacingSlider_ValueChanged;

            // 订阅OK键事件。
            foreach (OptionsOkButton okButton in _okButtons.Value.Values)
                okButton.Clicked += this.OkPressed;

            //OptionsElement lastGameOption = Game1.game1.CanBrowseScreenshots()
            //    ? optionsPage.options.FirstOrDefault(elem => elem is OptionsButton btn && btn.label == Game1.content.LoadString("Strings\\UI:OptionsPage_OpenFolder"))
            //    : optionsPage.options.FirstOrDefault(elem => elem is OptionsPlusMinusButton btn && btn.label == Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11254"));
            OptionsElement[] fontOptions =
            {
                title,
                this._fontSelector,
                this._originalExample,
                this._customExample,
                this._enableCheckbox,
                this._fontDropDown,
                this._fontSizeSlider,
                this._spacingSlider,
                this._lineSpacingSlider,
                this.CurrentOkButton
            };
            this._optionsPage.options.AddRange(fontOptions);
            this.options.AddRange(fontOptions);

            // 手动调一次用于初始化。
            this.FontTypeChanged(this._fontSelector, EventArgs.Empty);
        }

        public void Dispose()
        {
            // 取消订阅按下OK键的回调函数。
            foreach (OptionsOkButton okButton in _okButtons.Value.Values)
                okButton.Clicked -= this.OkPressed;
        }

        private async void OkPressed(object sender, EventArgs e)
        {
            FontConfig config = _config.Fonts.GetOrCreateFontConfig(LocalizedContentManager.CurrentLanguageCode,
                FontHelpers.GetCurrentLocale(), this._fontSelector.CurrentFont);

            FontConfig tempConfig = new FontConfig();
            config.CopyTo(tempConfig);

            this.ParseFontDropDownOption(this._fontDropDown.dropDownOptions[this._fontDropDown.selectedOption],
                out string fontFile, out int fontIndex);

            this._lastEnabled = tempConfig.Enabled;
            this._lastFontFilePath = tempConfig.FontFilePath;
            this._lastFontSize = (int)tempConfig.FontSize;
            this._lastSpacing = (int)tempConfig.Spacing;
            this._lastLineSpacing = tempConfig.LineSpacing;
            this._lastExistingFontPath = tempConfig.ExistingFontPath;

            tempConfig.Enabled = this._enableCheckbox.isChecked;
            tempConfig.FontFilePath = fontFile;
            tempConfig.FontIndex = fontIndex;
            tempConfig.FontSize = this._fontSizeSlider.Value;
            tempConfig.Spacing = this._spacingSlider.Value;
            tempConfig.LineSpacing = this._lineSpacingSlider.Value;

            bool enabledChanged = this._lastEnabled != tempConfig.Enabled;
            bool fontFilePathChanged = this._lastFontFilePath != tempConfig.FontFilePath;
            bool fontSizeChanged = this._lastFontSize != tempConfig.FontSize;
            bool spacingChanged = this._lastSpacing != tempConfig.Spacing;
            bool lineSpacingChanged = this._lastLineSpacing != tempConfig.LineSpacing;

            // 必要时重置字体文件路径。
            if (fontFilePathChanged || fontSizeChanged || spacingChanged || lineSpacingChanged)
                tempConfig.ExistingFontPath = null;

            var okButton = this.CurrentOkButton;
            okButton.SetStateProcessing();
            bool success = await _fontChanger.ReplaceOriginalOrRemainAsync(tempConfig);
            okButton.SetStateCompleted(success,
                success
                ? I18n.HudMessage_SuccessSetFont(tempConfig.InGameType.LocalizedName())
                : I18n.HudMessage_FailedSetFont(tempConfig.InGameType.LocalizedName()));

            // 如果成功，更新配置值。
            if (success)
            {
                tempConfig.CopyTo(config);
                _saveConfig(_config);

                // 删除bmfont文件。
                if (config.InGameType is GameFontType.SpriteText)
                    FontHelpers.DeleteBmFont(this._lastExistingFontPath);
            }
        }

        private void FontTypeChanged(object sender, EventArgs e)
        {
            GameFontType curFontType = this._fontSelector.CurrentFont;
            FontConfig config = _config.Fonts.GetOrCreateFontConfig(LocalizedContentManager.CurrentLanguageCode,
                FontHelpers.GetCurrentLocale(), curFontType);

            // 更新UI的值。
            this._enableCheckbox.isChecked = config.Enabled;

            string opt = this.BuildFontDropDownOption(config.FontFilePath, config.FontIndex);
            int index = this._fontDropDown.dropDownOptions.IndexOf(opt);
            if (index == -1) index = 0;
            this._fontDropDown.selectedOption = index;

            this._fontSizeSlider.Value = (int)config.FontSize;
            this._spacingSlider.Value = (int)config.Spacing;
            this._lineSpacingSlider.Value = config.LineSpacing;

            // 更新可用UI。
            this._fontDropDown.greyedOut = !this._enableCheckbox.isChecked;
            this._fontSizeSlider.greyedOut = !this._enableCheckbox.isChecked;
            this._spacingSlider.greyedOut = !this._enableCheckbox.isChecked;
            this._lineSpacingSlider.greyedOut = !this._enableCheckbox.isChecked;

            // 更新Ok按钮。
            int okBtnIndex = this._optionsPage.options.FindIndex(opt => opt is OptionsOkButton);
            this.options[okBtnIndex] = this._optionsPage.options[okBtnIndex] = this.CurrentOkButton;

            // 更新两个示例。
            this.UpdateGameExample();
            this.UpdateCustomExample(true);
        }

        private void EnableCheckbox_ValueChanged(object sender, EventArgs e)
        {
            this.UpdateCustomExample();

            this._fontDropDown.greyedOut = !this._enableCheckbox.isChecked;
            this._fontSizeSlider.greyedOut = !this._enableCheckbox.isChecked;
            this._spacingSlider.greyedOut = !this._enableCheckbox.isChecked;
            this._lineSpacingSlider.greyedOut = !this._enableCheckbox.isChecked;
        }

        private void FontDropDownChanged(object sender, EventArgs e)
        {
            this.UpdateCustomExample();
        }

        private void FontSizeSlider_ValueChanged(object sender, EventArgs e)
        {
            this.UpdateCustomExample();
        }

        private void SpacingSlider_ValueChanged(object sender, EventArgs e)
        {
            this.UpdateCustomExample();
        }

        private void LineSpacingSlider_ValueChanged(object sender, EventArgs e)
        {
            this.UpdateCustomExample();
        }

        private void UpdateGameExample()
        {
            if (this._fontSelector.CurrentFont is GameFontType.SpriteText)
            {
                this._originalExample.ExampleFont = _fontManager.GetBuiltInBmFont();
                this._originalExample.ExampleText = _config.ExampleText?.Replace('\n', '^');
            }
            else
            {
                this._originalExample.ExampleFont = new XNASpriteFont(_fontManager.GetBuiltInSpriteFont(this._fontSelector.CurrentFont));
                this._originalExample.ExampleText = _config.ExampleText;
            }
        }

        private void UpdateCustomExample(bool reset = true)  // reset：是否重置当前的示例。
        {
            if (this._fontSelector.CurrentFont is GameFontType.SpriteText)
                this._customExample.ExampleText = _config.ExampleText?.Replace('\n', '^');
            else
                this._customExample.ExampleText = _config.ExampleText;

            this.ParseFontDropDownOption(this._fontDropDown.dropDownOptions[this._fontDropDown.selectedOption],
                out string fontFile, out int fontIndex);

            if (reset)
                this._customExample.ExampleFont = _exampleFonts.ResetThenGet(this._fontSelector.CurrentFont,
                    this._enableCheckbox.isChecked,
                    fontFile,
                    fontIndex,
                    this._fontSizeSlider.Value,
                    this._spacingSlider.Value,
                    this._lineSpacingSlider.Value,
                    this._customExample.ExampleText
                );
            else
                this._customExample.ExampleFont = _exampleFonts.Get(this._fontSelector.CurrentFont,
                    this._enableCheckbox.isChecked,
                    fontFile,
                    fontIndex,
                    this._fontSizeSlider.Value,
                    this._spacingSlider.Value,
                    this._lineSpacingSlider.Value,
                    this._customExample.ExampleText
                );
        }

        private List<string> GetFontDropDownOptions()
        {
            FontModel[] fonts = InstalledFonts.GetAll().ToArray();
            List<string> result = new(fonts.Length + 1);
            result.Add("null/0");
            foreach (FontModel font in fonts)
            {
                string file = InstalledFonts.SimplifyPath(font.FullPath);
                int index = font.FontIndex;
                result.Add(this.BuildFontDropDownOption(file, index));
            }
            return result;
        }

        private List<string> GetFontDropDownDisplayOptions()
        {
            FontModel[] fonts = InstalledFonts.GetAll().ToArray();
            List<string> result = new(fonts.Length + 1);
            result.Add(I18n.OptionsPage_Font_KeepOrig());
            foreach (FontModel font in fonts)
            {
                string name = font.FamilyName;
                string style = font.SubfamilyName;
                result.Add(string.Format("{0} ({1})", name, style));
            }
            return result;
        }

        private void ParseFontDropDownOption(string opt, out string fontFile, out int fontIndex)
        {
            string[] args = opt.Split('/');
            fontFile = args[0];
            if (fontFile is "null") fontFile = null;
            fontIndex = int.Parse(args[1]);
        }

        private string BuildFontDropDownOption(string fontFile, int fontIndex)
        {
            if (fontFile is null)
            {
                return $"null/{fontIndex}";
            }

            fontFile = fontFile.Replace('/', '\\');
            return $"{fontFile}/{fontIndex}";
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            var dropDowns = this._optionsPage.options.OfType<OptionsDropDown>();
            if (dropDowns.Any() && dropDowns.Any(d => d.IsExpanded))
            {
                var expanded = dropDowns.FirstOrDefault(d => d.IsExpanded);
                int? slotIndex = null;
                for (int i = 0; i < this._optionsPage.optionSlots.Count; i++)
                    if (ReferenceEquals(this._optionsPage.options[this._optionsPage.currentItemIndex + i], expanded))
                    {
                        slotIndex = i;
                        break;
                    }
                if (slotIndex != null)
                    expanded.ReceiveGlobalLeftClick(x - this._optionsPage.optionSlots[slotIndex.Value].bounds.X, y - this._optionsPage.optionSlots[slotIndex.Value].bounds.Y);
            }
            else
                this._optionsPage.receiveLeftClick(x, y, playSound);
        }

        public override void leftClickHeld(int x, int y)
        {
            var dropDowns = this._optionsPage.options.OfType<OptionsDropDown>();
            if (dropDowns.Any() && dropDowns.Any(d => d.IsExpanded))
            {
                var expanded = dropDowns.FirstOrDefault(d => d.IsExpanded);
                int? slotIndex = null;
                for (int i = 0; i < this._optionsPage.optionSlots.Count; i++)
                    if (ReferenceEquals(this._optionsPage.options[this._optionsPage.currentItemIndex + i], expanded))
                    {
                        slotIndex = i;
                        break;
                    }
                if (slotIndex != null)
                    expanded.leftClickHeld(x - this._optionsPage.optionSlots[slotIndex.Value].bounds.X, y - this._optionsPage.optionSlots[slotIndex.Value].bounds.Y);
            }
            else
                this._optionsPage.leftClickHeld(x, y);
        }

        public override void releaseLeftClick(int x, int y)
        {
            var dropDowns = this._optionsPage.options.OfType<OptionsDropDown>();
            if (dropDowns.Any() && dropDowns.Any(d => d.IsExpanded))
            {
                var expanded = dropDowns.FirstOrDefault(d => d.IsExpanded);
                int? slotIndex = null;
                for (int i = 0; i < this._optionsPage.optionSlots.Count; i++)
                    if (ReferenceEquals(this._optionsPage.options[this._optionsPage.currentItemIndex + i], expanded))
                    {
                        slotIndex = i;
                        break;
                    }
                if (slotIndex != null)
                    expanded.leftClickReleased(x - this._optionsPage.optionSlots[slotIndex.Value].bounds.X, y - this._optionsPage.optionSlots[slotIndex.Value].bounds.Y);
            }
            else
                this._optionsPage.releaseLeftClick(x, y);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this._optionsPage.receiveRightClick(x, y, playSound);
        }

        public override void receiveKeyPress(Keys key)
        {
            this._optionsPage.receiveKeyPress(key);
        }

        public override void performHoverAction(int x, int y)
        {
            this._optionsPage.performHoverAction(x, y);

            for (int i = 0; i < this._optionsPage.optionSlots.Count; i++)
                if (this._optionsPage.currentItemIndex + i < this._optionsPage.options.Count
                    && this._optionsPage.options[this._optionsPage.currentItemIndex + i] is IHoverable hoverable)
                    hoverable.PerformHoverAction(x - this._optionsPage.optionSlots[i].bounds.X, y - this._optionsPage.optionSlots[i].bounds.Y);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            Point mousePos = Game1.getMousePosition();
            if (this._fontDropDown.CanScroll(mousePos))
            {
                this._fontDropDown.ReceiveScrollWheelAction(direction);
                return;
            }

            this._optionsPage.receiveScrollWheelAction(direction);
        }

        public override void update(GameTime time)
        {
            this._optionsPage.update(time);

            foreach (OptionsElement opt in this._optionsPage.options)
                if (opt is IUpdatable updatable)
                    updatable.Update(time);

            // 确保this.options与this._optionsPage.options两个集合是同步的。因为其他方法都行不通，所以只能退而求其次，在Update循环里弄。
            // 试过的行不通的方法：
            // 1. 隐藏基类options。
            // public new List<OptionsElement> options
            //    {
            //        get => this._optionsPage.options;
            //        set => this._optionsPage.options = value;
            //    }
            // 
            // 2. 继承options，实现INotifyCollectionChanged，然后在子类中隐藏基类Add、Remove等方法。
            // this.options = new ObservableList<OptionElement>();
            //
            // 行不通是因为外部看不到new修饰符的成员，看到的仍是OptionsPage.options。

            // 同步。（this._optionsPage.options向this.options同步）
            void Sync()
            {
                this._optionsPage.options.Clear();
                foreach (OptionsElement opt in this.options)
                    this._optionsPage.options.Add(opt);
            }

            // 如果长度不一样，肯定要同步。
            if (this.options.Count != this._optionsPage.options.Count)
                Sync();
            else
            {
                // 如果长度一样，就一个一个遍历排查。
                bool shouldSync = false;
                for (int i = 0; i < this.options.Count; i++)
                {
                    OptionsElement opt = this.options[i];
                    OptionsElement opt2 = this._optionsPage.options[i];
                    if (!ReferenceEquals(opt, opt2))
                    {
                        shouldSync = true;
                        break;
                    }
                }

                if (shouldSync)
                    Sync();
            }
        }

        public override void draw(SpriteBatch b)
        {
            this._optionsPage.draw(b);
        }
    }
}
