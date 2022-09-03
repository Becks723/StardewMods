using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontInfomation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValleyUI;
using StardewValleyUI.Controls;
using StardewValleyUI.Data;
using StardewValleyUI.Data.Converters;
using StardewValleyUI.Menus;

namespace FontSettings.Framework.Menus
{
    internal class FontSettingsPage : BaseMenu<FontSettingsMenuModel>
    {
        private static FontSettingsPage Instance { get; set; }

        private readonly FontPresetManager _presetManager;
        private readonly FontSettingsMenuModel _viewModel;

        private readonly Color _gameExampleColor = Color.Gray * 0.67f;
        private readonly Color _customExampleColor = Game1.textColor;
        private TextureButton _button_prevFontType;
        private TextureButton _button_nextFontType;
        private Label2 _label_title;
        private TextureBox _exampleBoard;
        private LabeledElement<Checkbox> _box_merge;  // TODO: 改成图标？
        private LabeledElement<Checkbox> _box_showBounds;
        private LabeledElement<Checkbox> _box_showText;
        private FontExampleLabel _label_gameExample;
        private FontExampleLabel _label_currentExample;
        private Label2 _label_currentPreset;
        private TextureButton _button_prevPreset;
        private TextureButton _button_nextPreset;
        private TextureButton _button_new;
        private TextureButton _button_save;
        private TextureButton _button_delete;
        private LabeledElement<Checkbox> _box_enabledFont;
        private Label2 _label_game;
        private Label2 _label_current;
        private ColorBlock _colorBlock_game;
        private ColorBlock _colorBlock_current;
        private ToggleTextureButton _button_offsetTuning;
        private Slider<float> _slider_charOffsetX;
        private Slider<float> _slider_charOffsetY;
        private ComboBox _dropDown_font;
        private RefreshButton _button_refresh;
        private LabeledElement<Slider<int>> _slider_fontSize;
        private LabeledElement<Slider<int>> _slider_spacing;
        private LabeledElement<Slider<int>> _slider_lineSpacing;
        private TextureButton _button_ok;

        private bool _isNewPresetMenu;
        private NewPresetMenu _newPresetMenu;

        protected override bool ManualInitializeComponents => true;

        public FontSettingsPage(ModConfig config, RuntimeFontManager fontManager, GameFontChanger fontChanger, FontPresetManager presetManager, Action<ModConfig> saveConfig,
            int x, int y, int width, int height, bool showUpperRightCloseButton = false)
            : base(x, y, width, height, showUpperRightCloseButton)
        {
            // 游戏中GameMenu在每次窗口大小改变时，都会重新创建实例。这导致GameMenu的子菜单（见GameMenu.pages字段）中保存的信息、状态直接清零。
            // 于是，下面这个函数通过一个单例，将前一实例的信息传递到下一实例。
            this.BeforeUpdateSingleton(Instance);

            Instance = this;

            this._presetManager = presetManager;

            this.ResetComponents();

            this._viewModel = new FontSettingsMenuModel(config, fontManager, fontChanger, presetManager, saveConfig);
            this._viewModel.TitleChanged += (_, _) => this._label_title.LocalPosition = new Vector2(this.width / 2 - this._label_title.Width / 2, 108);  // TODO: 去掉
            this._viewModel.ExampleVanillaUpdated += (_, _) => this.UpdateExamplePositions();  // TODO: 去掉
            this._viewModel.ExampleCurrentUpdated += (_, _) => this.UpdateExamplePositions();  // TODO: 去掉
            this._viewModel.UpdateExampleCurrent();
            this.DataContext = this._viewModel;
        }

        /// <summary>问题：如果同时安装了<see href="https://www.nexusmods.com/stardewvalley/mods/2668">魔法少女界面</see>，设置字体的界面会出现黑框问题，此函数为解决方法。</summary>
        /// <remarks>
        /// 问题根源在两个模组使用了不同的贴图。魔法少女最近一次更新在18年8月23号，当时即使最新的游戏版本也是1.3.28。当时Maps/MenuTiles中（64，320，64，64）的图片和现在不同，当时是深青色背景一个淡青色的鬼；现在则是一个背景框。扔进游戏里自然就出错了。
        /// </remarks>
        public FontSettingsPage FixConflictWithStarrySkyInterface(IModRegistry registry)
        {
            const string ID = "BeneathThePlass.StarrySkyInterfaceCP";
            if (registry.IsLoaded(ID))
            {
                this._exampleBoard.Kind = TextureBoxes.Default;  // 解决方法：将背景框改成默认款。
            }

            return this;
        }

        private void OnPrevFontTypeButtonClicked(object sender, EventArgs e)
        {
            Game1.playSound("smallSelect");

            this._viewModel.MoveToPreviousFontType();
        }

        private void OnNextFontTypeButtonClicked(object sender, EventArgs e)
        {
            Game1.playSound("smallSelect");

            this._viewModel.MoveToNextFontType();
        }

        private void ExampleMergeToggled(object sender, EventArgs e)
        {
            this.UpdateExamplePositions();
        }

        private void OffsetTuningToggled(object sender, EventArgs e)
        {
            Game1.playSound("smallSelect");

            this.UpdateExamplePositions();
        }

        private void OffsetXSlider_ValueChanged(object sender, EventArgs e)
        {
            this._viewModel.UpdateExampleCurrent();
        }

        private void OffsetYSlider_ValueChanged(object sender, EventArgs e)
        {
            this._viewModel.UpdateExampleCurrent();
        }

        private void FontEnableChanged(object sender, EventArgs e)
        {
            this._viewModel.UpdateExampleCurrent();
        }

        private void FontSizeSlider_ValueChanged(object sender, EventArgs e)
        {
            this._viewModel.UpdateExampleCurrent();
        }

        private void SpacingSlider_ValueChanged(object sender, EventArgs e)
        {
            this._viewModel.UpdateExampleCurrent();
        }

        private void LineSpacingSlider_ValueChanged(object sender, EventArgs e)
        {
            this._viewModel.UpdateExampleCurrent();
        }

        private void FontSelectionChanged(object sender, EventArgs e)
        {
            this._viewModel.UpdateExampleCurrent();
        }

        private async void OkButtonClicked(object sender, EventArgs e)
        {
            Game1.playSound("coin");

            var (fontType, success) = await this._viewModel.TryGenerateFont();
            if (success)
            {
                Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_SuccessSetFont(fontType.LocalizedName()), null));
                Game1.playSound("money");
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_FailedSetFont(fontType.LocalizedName()), HUDMessage.error_type));
                Game1.playSound("cancel");
            }
        }

        private void PreviousPresetButtonClicked(object sender, EventArgs e)
        {
            Game1.playSound("smallSelect");

            this._viewModel.MoveToPreviousPreset();
        }

        private void NextPresetButtonClicked(object sender, EventArgs e)
        {
            Game1.playSound("smallSelect");

            this._viewModel.MoveToNextPreset();
        }

        private void NewPresetButtonClicked(object sender, EventArgs e)
        {
            Game1.playSound("coin");

            this._newPresetMenu ??= this.CreateNewPresetMenu();
            this._isNewPresetMenu = true;
        }

        private void SavePresetButtonClicked(object sender, EventArgs e)
        {
            Game1.playSound("newRecipe");

            this._viewModel.SaveCurrentPreset();
        }

        private void DeletePresetButtonClicked(object sender, EventArgs e)
        {
            Game1.playSound("trashcan");

            this._viewModel.DeleteCurrentPreset();
        }

        protected override void ResetComponents(RootElement root, IBindingContext context)
        {
            root.LocalPosition = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen);

            this._button_prevFontType = new TextureButton(
                Game1.mouseCursors, new(352, 495, 12, 11), 4f);
            this._button_prevFontType.LocalPosition = new Vector2(-48 - 48, this.height / 2);
            this._button_prevFontType.SettableWidth = 48;
            this._button_prevFontType.SettableHeight = 44;
            this._button_prevFontType.Click += this.OnPrevFontTypeButtonClicked;

            this._button_nextFontType = new TextureButton(
                Game1.mouseCursors, new(365, 495, 12, 11), 4f);
            this._button_nextFontType.LocalPosition = new Vector2(this.width + 48, this.height / 2);
            this._button_nextFontType.SettableWidth = 48;
            this._button_nextFontType.SettableHeight = 44;
            this._button_nextFontType.Click += this.OnNextFontTypeButtonClicked;

            this._label_title = new Label2();
            this._label_title.Bold = true;
            this._label_title.Text = "小字体";  // 这行只是用于初始化label的高度。
            this._label_title.LocalPosition = new Vector2(this.width / 2 - this._label_title.Width / 2, 108);

            this._exampleBoard = new TextureBox();
            this._exampleBoard.Kind = TextureBoxes.DefaultBorderless;
            this._exampleBoard.DrawShadow = false;
            this._exampleBoard.LocalPosition = new Vector2(spaceToClearSideBorder + borderWidth, this._label_title.LocalPosition.Y + this._label_title.Height);
            this._exampleBoard.SettableHeight = this.height / 3;
            this._exampleBoard.SettableWidth = this.width - spaceToClearSideBorder - borderWidth - (int)this._exampleBoard.LocalPosition.X;

            Checkbox mergeBox = new Checkbox();
            mergeBox.Checked += this.ExampleMergeToggled;
            mergeBox.Unchecked += this.ExampleMergeToggled;
            this._box_merge = new LabeledElement<Checkbox>(mergeBox);
            this._box_merge.Text = I18n.OptionsPage_MergeExamples();

            Checkbox showBoundsBox = new Checkbox();
            this._box_showBounds = new LabeledElement<Checkbox>(showBoundsBox);
            this._box_showBounds.Text = I18n.OptionsPage_ShowExampleBounds();

            Checkbox showTextBox = new Checkbox();
            this._box_showText = new LabeledElement<Checkbox>(showTextBox);
            this._box_showText.Text = I18n.OptionsPage_ShowExampleText();

            float gap = (this._exampleBoard.Height - this._box_merge.Height - this._box_showText.Height - this._box_showBounds.Height) / 4f;
            float insideBoardX = this._exampleBoard.LocalPosition.X + borderWidth / 3;
            this._box_merge.LocalPosition = new Vector2(insideBoardX, this._exampleBoard.LocalPosition.Y + gap);
            this._box_showBounds.LocalPosition = new Vector2(insideBoardX, this._box_merge.LocalPosition.Y + this._box_merge.Height + gap);
            this._box_showText.LocalPosition = new Vector2(insideBoardX, this._box_showBounds.LocalPosition.Y + this._box_showBounds.Height + gap);

            this._label_game = new Label2();
            this._label_game.Text = I18n.OptionsPage_OriginalExample();

            this._label_current = new Label2();
            this._label_current.Text = I18n.OptionsPage_CustomExample();

            this._colorBlock_game = new ColorBlock(this._gameExampleColor, 20);
            this._colorBlock_current = new ColorBlock(this._customExampleColor, 20);

            int maxWidth = Math.Max(this._label_game.Width, this._label_current.Width);
            int exampleLabelHeight = Math.Max(this._colorBlock_game.Height, this._label_game.Height);
            int currentLabelHeight = Math.Max(this._colorBlock_current.Height, this._label_current.Height);
            this._label_current.LocalPosition = new Vector2(this._exampleBoard.LocalPosition.X + this._exampleBoard.Width - borderWidth / 3 - maxWidth, this._exampleBoard.LocalPosition.Y + this._exampleBoard.Height - borderWidth / 3 - currentLabelHeight);
            this._label_game.LocalPosition = new Vector2(this._exampleBoard.LocalPosition.X + this._exampleBoard.Width - borderWidth / 3 - maxWidth, this._label_current.LocalPosition.Y - borderWidth / 3 - exampleLabelHeight);
            this._colorBlock_current.LocalPosition = new Vector2(this._label_current.LocalPosition.X - borderWidth / 6 - this._colorBlock_current.Width, this._label_current.LocalPosition.Y + this._label_current.Height / 2 - this._colorBlock_current.Height / 2);
            this._colorBlock_game.LocalPosition = new Vector2(this._label_game.LocalPosition.X - borderWidth / 6 - this._colorBlock_game.Width, this._label_game.LocalPosition.Y + this._label_game.Height / 2 - this._colorBlock_game.Height / 2);

            this._label_gameExample = new FontExampleLabel();
            this._label_gameExample.IdleTextColor = this._gameExampleColor;
            this._label_gameExample.BoundsColor = Color.Red * 0.5f;
            this._label_gameExample.ShowBounds = this._box_showBounds.Element.IsChecked;
            this._label_gameExample.ShowText = this._box_showText.Element.IsChecked;

            this._label_currentExample = new FontExampleLabel();
            this._label_currentExample.IdleTextColor = this._customExampleColor;
            this._label_currentExample.BoundsColor = Color.Green * 0.5f;
            this._label_currentExample.ShowBounds = this._box_showBounds.Element.IsChecked;
            this._label_currentExample.ShowText = this._box_showText.Element.IsChecked;

            int maxWidthInLeftThree = new[] { this._box_merge.Width, this._box_showBounds.Width, this._box_showText.Width }.Max();
            Rectangle exampleBounds = new Rectangle(
                (int)(this._box_merge.LocalPosition.X + maxWidthInLeftThree + borderWidth / 2),
                (int)(this._exampleBoard.LocalPosition.Y + borderWidth / 3),
                (int)(this._colorBlock_game.LocalPosition.X - borderWidth - this._box_merge.LocalPosition.X - maxWidthInLeftThree),
                this._exampleBoard.Height - borderWidth / 3 * 2);

            float offsetTuningScale = 3f;
            this._button_offsetTuning = new ToggleTextureButton(
                Game1.mouseCursors, new Rectangle(257, 284, 16, 16), offsetTuningScale);
            this._button_offsetTuning.SettableWidth = (int)(16 * offsetTuningScale);
            this._button_offsetTuning.SettableHeight = (int)(16 * offsetTuningScale);
            this._button_offsetTuning.LocalPosition = new Vector2(this._exampleBoard.LocalPosition.X + this._exampleBoard.Width - borderWidth / 3 - this._button_offsetTuning.Width, exampleBounds.Y);
            this._button_offsetTuning.Click += this.OffsetTuningToggled;

            this._slider_charOffsetX = new Slider<float>();
            this._slider_charOffsetX.LocalPosition = new Vector2(exampleBounds.X + 24, exampleBounds.Y);
            this._slider_charOffsetX.Orientation = Orientation.Horizontal;
            this._slider_charOffsetX.Length = exampleBounds.Width - 24;
            this._slider_charOffsetX.BarThickness = 16;
            this._slider_charOffsetX.Interval = 0.5f;
            this._slider_charOffsetX.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
            this._slider_charOffsetX.ValueChanged += this.OffsetXSlider_ValueChanged;

            this._slider_charOffsetY = new Slider<float>();
            this._slider_charOffsetY.LocalPosition = new Vector2(exampleBounds.X, exampleBounds.Y + 24);
            this._slider_charOffsetY.Orientation = Orientation.Vertical;
            this._slider_charOffsetY.Length = exampleBounds.Height - 24;
            this._slider_charOffsetY.BarThickness = 16;
            this._slider_charOffsetY.Interval = 0.5f;
            this._slider_charOffsetY.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
            this._slider_charOffsetY.ValueChanged += this.OffsetYSlider_ValueChanged;

            float exampleBoardBottom = this._exampleBoard.LocalPosition.Y + this._exampleBoard.Height;
            float exampleBoardX = this._exampleBoard.LocalPosition.X;
            float presetSectionY = exampleBoardBottom + borderWidth / 2;
            float presetSectionBottom = 0;
            {
                float scale_new = 4f;
                float scale_save = 3f;
                float scale_delete = 0.75f;
                float scale_prev = 4f;
                float scale_next = 4f;
                Vector2 size_new = new(10 * scale_new);
                Vector2 size_save = new(16 * scale_save);
                Vector2 size_delete = new(64 * scale_delete);
                Vector2 size_prev = new Vector2(12, 11) * scale_prev;
                Vector2 size_next = new Vector2(12, 11) * scale_next;
                float presetSectionMaxHeight = new[] { size_new, size_save, size_delete, size_prev, size_next }.Max(v => v.Y);
                this._label_currentPreset = new Label2();
                this._label_currentPreset.LocalPosition = new Vector2(exampleBoardX, presetSectionY);

                this._button_delete = new TextureButton(
                    Game1.mouseCursors, new Rectangle(192, 256, 64, 64), scale_delete);
                this._button_delete.LocalPosition = new Vector2(this._exampleBoard.LocalPosition.X + this._exampleBoard.Width - size_delete.X, presetSectionY + presetSectionMaxHeight / 2 - size_delete.Y / 2);
                this._button_delete.SettableWidth = (int)size_delete.X;
                this._button_delete.SettableHeight = (int)size_delete.Y;
                this._button_delete.Click += this.DeletePresetButtonClicked;

                this._button_save = new TextureButton(
                    Game1.mouseCursors, new Rectangle(274, 284, 16, 16), scale_save);
                this._button_save.LocalPosition = new Vector2(this._button_delete.LocalPosition.X - borderWidth / 3 - size_save.X, presetSectionY + presetSectionMaxHeight / 2 - size_save.Y / 2);
                this._button_save.SettableWidth = (int)size_save.X;
                this._button_save.SettableHeight = (int)size_save.Y;
                this._button_save.Click += this.SavePresetButtonClicked;

                this._button_new = new TextureButton(
                    Game1.mouseCursors, new Rectangle(0, 428, 10, 10), scale_new);
                this._button_new.LocalPosition = new Vector2(this._button_save.LocalPosition.X - borderWidth / 3 - size_new.X, presetSectionY + presetSectionMaxHeight / 2 - size_new.Y / 2);
                this._button_new.SettableWidth = (int)size_new.X;
                this._button_new.SettableHeight = (int)size_new.Y;
                this._button_new.Click += this.NewPresetButtonClicked;

                this._button_nextPreset = new TextureButton(
                    Game1.mouseCursors, new Rectangle(365, 495, 12, 11), scale_next);
                this._button_nextPreset.LocalPosition = new Vector2(this._button_new.LocalPosition.X - borderWidth / 3 - size_next.X, presetSectionY + presetSectionMaxHeight / 2 - size_next.Y / 2);
                this._button_nextPreset.SettableWidth = (int)size_next.X;
                this._button_nextPreset.SettableHeight = (int)size_next.Y;
                this._button_nextPreset.Click += this.NextPresetButtonClicked;

                this._button_prevPreset = new TextureButton(
                    Game1.mouseCursors, new Rectangle(352, 495, 12, 11), scale_prev);
                this._button_prevPreset.LocalPosition = new Vector2(this._button_nextPreset.LocalPosition.X - borderWidth / 3 - size_prev.X, presetSectionY + presetSectionMaxHeight / 2 - size_prev.Y / 2);
                this._button_prevPreset.SettableWidth = (int)size_prev.X;
                this._button_prevPreset.SettableHeight = (int)size_prev.Y;
                this._button_prevPreset.Click += this.PreviousPresetButtonClicked;

                presetSectionBottom = presetSectionY + presetSectionMaxHeight;
            }

            Checkbox enabledFontBox = new Checkbox();
            enabledFontBox.Checked += this.FontEnableChanged;
            enabledFontBox.Unchecked += this.FontEnableChanged;
            this._box_enabledFont = new LabeledElement<Checkbox>(enabledFontBox);
            this._box_enabledFont.Text = I18n.OptionsPage_Enable();

            int sliderLength = this._exampleBoard.Width / 3;
            var fontSizeSlider = new Slider<int>();
            fontSizeSlider.Length = sliderLength;
            fontSizeSlider.Interval = 1;
            fontSizeSlider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
            fontSizeSlider.ValueChanged += this.FontSizeSlider_ValueChanged;
            this._slider_fontSize = new LabeledElement<Slider<int>>(fontSizeSlider);
            this._slider_fontSize.Text = I18n.OptionsPage_FontSize();

            var spacingSlider = new Slider<int>();
            spacingSlider.Length = sliderLength;
            spacingSlider.Interval = 1;
            spacingSlider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
            spacingSlider.ValueChanged += this.SpacingSlider_ValueChanged;
            this._slider_spacing = new LabeledElement<Slider<int>>(spacingSlider);
            this._slider_spacing.Text = I18n.OptionsPage_Spacing();

            var lineSpacingSlider = new Slider<int>();
            lineSpacingSlider.Length = sliderLength;
            lineSpacingSlider.Interval = 1;
            lineSpacingSlider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
            lineSpacingSlider.ValueChanged += this.LineSpacingSlider_ValueChanged;
            this._slider_lineSpacing = new LabeledElement<Slider<int>>(lineSpacingSlider);
            this._slider_lineSpacing.Text = I18n.OptionsPage_LineSpacing();

            gap = (this.height - spaceToClearSideBorder - presetSectionBottom - this._box_enabledFont.Height - this._slider_fontSize.Height - this._slider_spacing.Height - this._slider_lineSpacing.Height) / 5;
            this._box_enabledFont.LocalPosition = new Vector2(exampleBoardX, presetSectionBottom + gap);
            this._slider_fontSize.LocalPosition = new Vector2(exampleBoardX, this._box_enabledFont.LocalPosition.Y + this._box_enabledFont.Height + gap);
            this._slider_spacing.LocalPosition = new Vector2(exampleBoardX, this._slider_fontSize.LocalPosition.Y + this._slider_fontSize.Height + gap);
            this._slider_lineSpacing.LocalPosition = new Vector2(exampleBoardX, this._slider_spacing.LocalPosition.Y + this._slider_spacing.Height + gap);

            this._dropDown_font = new ComboBox();
            this._dropDown_font.SettableWidth = this._exampleBoard.Width / 2;
            this._dropDown_font.DisplayTextReslover = this.DisplayFontOnComboBox;
            this._dropDown_font.EqualityComparer = new FontEqualityComparer();
            this._dropDown_font.MaxDisplayRows = 6;
            this._dropDown_font.LocalPosition = new Vector2(this._exampleBoard.LocalPosition.X + this._exampleBoard.Width - this._dropDown_font.Width, this._box_enabledFont.LocalPosition.Y);
            this._dropDown_font.SelectionChanged += this.FontSelectionChanged;

            float refreshScale = 2.5f;
            int refreshWidth = (int)(16 * refreshScale);
            int refreshHeight = (int)(16 * refreshScale);
            this._button_refresh = new RefreshButton(refreshScale);
            this._button_refresh.AnimationDuration = 300;
            this._button_refresh.LocalPosition = new Vector2(this._dropDown_font.LocalPosition.X - borderWidth / 3 - refreshWidth, this._dropDown_font.LocalPosition.Y + this._dropDown_font.Height / 2 - refreshHeight / 2);
            this._button_refresh.SettableWidth = refreshWidth;
            this._button_refresh.SettableHeight = refreshHeight;
            this._button_refresh.Click += this.RefreshAllFonts;

            this._button_ok = new TextureButton(
                Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46));
            this._button_ok.Click += this.OkButtonClicked;
            this._button_ok.SettableWidth = 64;
            this._button_ok.SettableHeight = 64;
            this._button_ok.LocalPosition = new Vector2(
                this.width - spaceToClearSideBorder - borderWidth - this._button_ok.Width,
                this.height - spaceToClearSideBorder - borderWidth - this._button_ok.Height);

            root.Add(
                this._button_prevFontType,
                this._button_nextFontType,
                this._label_title,
                this._exampleBoard,
                this._box_merge,
                this._box_showBounds,
                this._box_showText,
                this._colorBlock_game,
                this._label_game,
                this._colorBlock_current,
                this._label_current,
                this._label_gameExample,
                this._label_currentExample,
                this._button_offsetTuning,
                this._slider_charOffsetX,
                this._slider_charOffsetY,
                this._label_currentPreset,
                this._button_prevPreset,
                this._button_nextPreset,
                this._button_new,
                this._button_save,
                this._button_delete,
                this._box_enabledFont,
                this._slider_fontSize,
                this._slider_spacing,
                this._slider_lineSpacing,
                this._dropDown_font,
                this._button_refresh,
                this._button_ok);

#pragma warning disable format
            context.AddBinding(() => this._viewModel.Title, () => this._label_title.Text, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.FontEnabled, () => this._box_enabledFont.Element.IsChecked, BindingMode.TwoWay);
            
            context.AddBinding(() => this._viewModel.FontSize, () => this._slider_fontSize.Element.Value, BindingMode.TwoWay);
            context.AddBinding(() => this._viewModel.MinFontSize, () => this._slider_fontSize.Element.Minimum, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.MaxFontSize, () => this._slider_fontSize.Element.Maximum, BindingMode.OneWay);
            
            context.AddBinding(() => this._viewModel.Spacing, () => this._slider_spacing.Element.Value, BindingMode.TwoWay);
            context.AddBinding(() => this._viewModel.MinSpacing, () => this._slider_spacing.Element.Minimum, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.MaxSpacing, () => this._slider_spacing.Element.Maximum, BindingMode.OneWay);
            
            context.AddBinding(() => this._viewModel.LineSpacing, () => this._slider_lineSpacing.Element.Value, BindingMode.TwoWay);
            context.AddBinding(() => this._viewModel.MinLineSpacing, () => this._slider_lineSpacing.Element.Minimum, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.MaxLineSpacing, () => this._slider_lineSpacing.Element.Maximum, BindingMode.OneWay);

            context.AddBinding(() => this._viewModel.CharOffsetX, () => this._slider_charOffsetX.Value, BindingMode.TwoWay);
            context.AddBinding(() => this._viewModel.MinCharOffsetX, () => this._slider_charOffsetX.Minimum, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.MaxCharOffsetX, () => this._slider_charOffsetX.Maximum, BindingMode.OneWay);

            context.AddBinding(() => this._viewModel.CharOffsetY, () => this._slider_charOffsetY.Value, BindingMode.TwoWay);
            context.AddBinding(() => this._viewModel.MinCharOffsetY, () => this._slider_charOffsetY.Minimum, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.MaxCharOffsetY, () => this._slider_charOffsetY.Maximum, BindingMode.OneWay);

            context.AddBinding(() => this._viewModel.AllFonts, () => this._dropDown_font.ItemsSource, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.CurrentFont, () => this._dropDown_font.SelectedItem, BindingMode.TwoWay);
            context.AddBinding(() => this._viewModel.ExamplesMerged, () => this._box_merge.Element.IsChecked, BindingMode.TwoWay);  
            context.AddBinding(() => this._viewModel.ShowExampleBounds, () => this._box_showBounds.Element.IsChecked, BindingMode.TwoWay);  
            context.AddBinding(() => this._viewModel.ShowExampleBounds, () => this._label_gameExample.ShowBounds, BindingMode.OneWay);  
            context.AddBinding(() => this._viewModel.ShowExampleBounds, () => this._label_currentExample.ShowBounds, BindingMode.OneWay);  
            context.AddBinding(() => this._viewModel.ShowExampleText, () => this._box_showText.Element.IsChecked, BindingMode.TwoWay); 
            context.AddBinding(() => this._viewModel.ShowExampleText, () => this._label_gameExample.ShowText, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.ShowExampleText, () => this._label_currentExample.ShowText, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.ExampleText, () => this._label_gameExample.Text, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.ExampleText, () => this._label_currentExample.Text, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.ExampleVanillaFont, () => this._label_gameExample.Font, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.ExampleCurrentFont, () => this._label_currentExample.Font, BindingMode.OneWay);

            context.AddBinding(() => this._viewModel.IsTuningCharOffset, () => this._button_offsetTuning.IsToggled, BindingMode.TwoWay);
            context.AddBinding(() => this._viewModel.IsTuningCharOffset, () => this._slider_charOffsetX.Visibility, BindingMode.OneWay, new BooleanVisibilityConverter());
            context.AddBinding(() => this._viewModel.IsTuningCharOffset, () => this._slider_charOffsetY.Visibility, BindingMode.OneWay, new BooleanVisibilityConverter());

            context.AddBinding(() => this._viewModel.CurrentPresetName, () => this._label_currentPreset.Text, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.CanSaveCurrentAsNewPreset, () => this._button_new.GreyedOut, BindingMode.OneWay, new TrueFalseConverter());
            context.AddBinding(() => this._viewModel.CanSaveCurrentPreset, () => this._button_save.GreyedOut, BindingMode.OneWay, new TrueFalseConverter());
            context.AddBinding(() => this._viewModel.CanDeleteCurrentPreset, () => this._button_delete.GreyedOut, BindingMode.OneWay, new TrueFalseConverter());
            context.AddBinding(() => this._viewModel.CanGenerateFont,    () => this._button_ok.GreyedOut, BindingMode.OneWay, new TrueFalseConverter());
#pragma warning restore format
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._newPresetMenu?.Dispose();
        }

        public override void update(GameTime time)
        {
            if (!this._isNewPresetMenu)
                base.update(time);
            else
            {
                this._newPresetMenu ??= this.CreateNewPresetMenu();

                if (this._newPresetMenu.IsFinished)
                {
                    this._newPresetMenu.Dispose();
                    this._newPresetMenu = null;
                    this._isNewPresetMenu = false;
                    return;
                }

                this._newPresetMenu.update(time);
            }
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

#if DEBUG
            b.DrawString(Game1.smallFont, $"Size: {this._viewModel.FontSize}\n"
                + $"Spacing: {this._viewModel.Spacing}\n"
                + $"Line spacing: {this._viewModel.LineSpacing}\n"
                + $"Font: {this._viewModel.FontFilePath}\n"
                + $"Font index: {this._viewModel.FontIndex}\n"
                + $"Offset-x: {this._viewModel.CharOffsetX}\n"
                + $"Offset-y: {this._viewModel.CharOffsetY}", new Vector2(this.xPositionOnScreen, this.yPositionOnScreen), Color.Blue);
#endif

            if (this._isNewPresetMenu)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
                this._newPresetMenu ??= this.CreateNewPresetMenu();
                this._newPresetMenu.draw(b);
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);

            if (this._isNewPresetMenu)
            {
                this._newPresetMenu ??= this.CreateNewPresetMenu();
                this._newPresetMenu.receiveKeyPress(key);
            }
        }

        protected override bool CanClose()
        {
            if (!this._isNewPresetMenu)
                return true;
            else
            {
                this._newPresetMenu ??= this.CreateNewPresetMenu();
                return this._newPresetMenu.readyToClose();
            }
        }

        private void UpdateExamplePositions()
        {
            Rectangle exampleBounds;
            if (this._viewModel.IsTuningCharOffset)
                exampleBounds = new Rectangle(
                    (int)(this._slider_charOffsetY.LocalPosition.X + this._slider_charOffsetY.Width + borderWidth / 2),
                    (int)(this._slider_charOffsetX.LocalPosition.Y + this._slider_charOffsetX.Height + borderWidth / 3),
                    (int)(this._colorBlock_game.LocalPosition.X - this._slider_charOffsetY.LocalPosition.X - this._slider_charOffsetY.Width - borderWidth),
                    (int)(this._exampleBoard.LocalPosition.Y + this._exampleBoard.Height - this._slider_charOffsetX.LocalPosition.Y - this._slider_charOffsetX.Height - borderWidth / 3 * 2));
            else
            {
                int maxWidthInLeftThree = new[] { this._box_merge.Width, this._box_showBounds.Width, this._box_showText.Width }.Max();
                exampleBounds = new Rectangle(
                    (int)(this._box_merge.LocalPosition.X + maxWidthInLeftThree + borderWidth / 2),
                    (int)(this._exampleBoard.LocalPosition.Y + borderWidth / 3),
                    (int)(this._colorBlock_game.LocalPosition.X - borderWidth - this._box_merge.LocalPosition.X - maxWidthInLeftThree),
                    this._exampleBoard.Height - borderWidth / 3 * 2);
            }

            int maxWidth = Math.Max(this._label_gameExample.Width, this._label_currentExample.Width);
            int maxHeight = Math.Max(this._label_gameExample.Height, this._label_currentExample.Height);
            int centerX = exampleBounds.Center.X - maxWidth / 2;
            int centerY = exampleBounds.Center.Y - maxHeight / 2;
            if (this._viewModel.ExamplesMerged)
                this._label_gameExample.LocalPosition = this._label_currentExample.LocalPosition = new Vector2(centerX, centerY);
            else
            {
                this._label_gameExample.LocalPosition = new Vector2(centerX, exampleBounds.Y);
                this._label_currentExample.LocalPosition = new Vector2(centerX, exampleBounds.Center.Y);
            }
        }

        private string DisplayFontOnComboBox(object[] source, object item)
        {
            FontModel font = item as FontModel;

            if (font.FullPath == null)
                return I18n.OptionsPage_Font_KeepOrig();
            else
                return $"{font.FamilyName} ({font.SubfamilyName})";
        }

        private void RefreshAllFonts(object sender, EventArgs e)
        {
            Game1.playSound("trashcan");

            this._viewModel.RefreshAllFonts();
        }

        private NewPresetMenu CreateNewPresetMenu()
        {
            var result = new NewPresetMenu(
                this._presetManager,
                this.xPositionOnScreen + this.width / 4,
                this.yPositionOnScreen + this.height / 3,
                this.width / 2,
                this.height / 3);
            result.Accepted += (_, name) => this._viewModel.SaveCurrentAsNewPreset(name);

            return result;
        }

        /// <summary>一些在单例<see cref="Instance"/>更新前（单例在每次创建实例时更新）要做的事。</summary>
        /// <param name="instance">上一个实例<see cref="Instance"/>。</param>
        private void BeforeUpdateSingleton(FontSettingsPage instance)
        {
            // 首次创建实例，直接返回。
            if (instance == null) return;

            this._isNewPresetMenu = instance._isNewPresetMenu;
        }
    }
}
