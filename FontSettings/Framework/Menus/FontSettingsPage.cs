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
using StardewValley.Menus;
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
        private readonly bool _isStandalone;
        private readonly FontSettingsMenuModel _viewModel;

        private TextureBox _previewBoard;

        private bool _isNewPresetMenu;
        private NewPresetMenu _newPresetMenu;

        public FontSettingsPage(ModConfig config, FontManager fontManager, GameFontChanger fontChanger, FontPresetManager presetManager, Action<ModConfig> saveConfig,
            int x, int y, int width, int height, bool isStandalone = false)
            : base(x, y, width, height)
        {
            // 游戏中GameMenu在每次窗口大小改变时，都会重新创建实例。这导致GameMenu的子菜单（见GameMenu.pages字段）中保存的信息、状态直接清零。
            // 于是，下面这个函数通过一个单例，将前一实例的信息传递到下一实例。
            this.BeforeUpdateSingleton(Instance);

            Instance = this;

            this._presetManager = presetManager;
            this._isStandalone = isStandalone;

            this.ResetComponents();

            this._viewModel = new FontSettingsMenuModel(config, fontManager, fontChanger, presetManager, saveConfig);
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
                this._previewBoard.Kind = TextureBoxes.Default;  // 解决方法：将背景框改成默认款。
            }

            return this;
        }

        private void UpdateExampleCurrent(object sender, EventArgs e)
        {
            this._viewModel.UpdateExampleCurrent();
        }

        private async void OkButtonClicked(object sender, EventArgs e)
        {
            Game1.playSound("coin");

            var (fontType, success) = await this._viewModel.TryGenerateFont();
            if (success)
            {
                Game1.playSound("money");

                string message = I18n.HudMessage_SuccessSetFont(fontType.LocalizedName());
                if (Game1.gameMode == 0)  // 如果在标题页面，HUD无法显示，
                    ILog.Info(message);   // 写在日志中
                else
                    Game1.addHUDMessage(new OverlayHUDMessage(message, null));
            }
            else
            {
                Game1.playSound("cancel");

                string message = I18n.HudMessage_FailedSetFont(fontType.LocalizedName());
                if (Game1.gameMode == 0)  // 如果在标题页面，HUD无法显示，
                    ILog.Error(message);  // 写在日志中
                else
                    Game1.addHUDMessage(new OverlayHUDMessage(message, HUDMessage.error_type));
            }
        }

        protected override void ResetComponents(MenuInitializationContext context)
        {
            context
                .PositionMode(PositionMode.Auto)
                .Aligns(HorizontalAlignment.Center, VerticalAlignment.Center);
            this.width = -1;
            this.height = 600 + IClickableMenu.borderWidth * 2 + 64;

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(800 + IClickableMenu.borderWidth * 2) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var prevFontButton = new TextureButton(
                Game1.mouseCursors, new(352, 495, 12, 11), 4f);
            prevFontButton.HorizontalAlignment = HorizontalAlignment.Left;
            prevFontButton.VerticalAlignment = VerticalAlignment.Center;
            prevFontButton.Margin = new Thickness(0, 0, 48, 0);
            context.OneWayBinds(() => this._viewModel.MoveToPrevFont, () => prevFontButton.Command);
            prevFontButton.Click += (_, _) => Game1.playSound("smallSelect");
            grid.Children.Add(prevFontButton);
            grid.SetColumn(prevFontButton, 0);

            var nextFontButton = new TextureButton(
                Game1.mouseCursors, new(365, 495, 12, 11), 4f);
            nextFontButton.HorizontalAlignment = HorizontalAlignment.Right;
            nextFontButton.VerticalAlignment = VerticalAlignment.Center;
            nextFontButton.Margin = new Thickness(48, 0, 0, 0);
            context.OneWayBinds(() => this._viewModel.MoveToNextFont, () => nextFontButton.Command);
            nextFontButton.Click += (_, _) => Game1.playSound("smallSelect");
            grid.Children.Add(nextFontButton);
            grid.SetColumn(nextFontButton, 2);

            var mainBorder = new TextureBox();
            mainBorder.DrawShadow = false;
            mainBorder.Padding += new Thickness(borderWidth / 2);
            grid.Children.Add(mainBorder);
            grid.SetColumn(mainBorder, 1);
            {
                Grid mainGrid = new Grid();
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                mainBorder.Content = mainGrid;
                {
                    var titleLabel = new Label();
                    titleLabel.Font = FontType.SpriteText;
                    titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    titleLabel.VerticalAlignment = VerticalAlignment.Center;
                    titleLabel.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                    context.OneWayBinds(() => this._viewModel.Title, () => titleLabel.Text);
                    mainGrid.Children.Add(titleLabel);
                    mainGrid.SetRow(titleLabel, 0);

                    var previewBorder = this._previewBoard = new TextureBox();
                    previewBorder.Kind = TextureBoxes.DefaultBorderless;
                    previewBorder.DrawShadow = false;
                    previewBorder.Padding += new Thickness(borderWidth / 3);
                    mainGrid.Children.Add(previewBorder);
                    mainGrid.SetRow(previewBorder, 1);
                    {
                        Grid previewGrid = new Grid();
                        previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                        previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(borderWidth / 2) });
                        previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        previewBorder.Content = previewGrid;
                        {
                            Grid previewMainGrid = new Grid();
                            previewMainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            previewMainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                            previewMainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            previewMainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
                            previewGrid.Children.Add(previewMainGrid);
                            previewGrid.SetColumn(previewMainGrid, 0);
                            {
                                var toggleOffsetButton = new ToggleTextureButton(
                                    Game1.mouseCursors, new Rectangle(257, 284, 16, 16), 3f);
                                toggleOffsetButton.ToolTipText = I18n.Ui_Tooltip_ToggleCharOffsetTuning();
                                toggleOffsetButton.Click += (_, _) => Game1.playSound("smallSelect");
                                context.TwoWayBinds(() => this._viewModel.IsTuningCharOffset, () => toggleOffsetButton.IsToggled);
                                previewMainGrid.Children.Add(toggleOffsetButton);
                                previewMainGrid.SetColumn(toggleOffsetButton, 0);
                                previewMainGrid.SetRow(toggleOffsetButton, 0);

                                var xOffsetSlider = new Slider();
                                xOffsetSlider.Orientation = Orientation.Horizontal;
                                xOffsetSlider.VerticalAlignment = VerticalAlignment.Center;
                                xOffsetSlider.Margin = new Thickness(borderWidth / 3, 0, 0, 0);
                                xOffsetSlider.BarThickness = 24;
                                xOffsetSlider.Interval = 0.5f;
                                xOffsetSlider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                xOffsetSlider.ValueChanged += this.UpdateExampleCurrent;
                                context.TwoWayBinds(() => this._viewModel.CharOffsetX, () => xOffsetSlider.Value);
                                context.OneWayBinds(() => this._viewModel.MinCharOffsetX, () => xOffsetSlider.Minimum);
                                context.OneWayBinds(() => this._viewModel.MaxCharOffsetX, () => xOffsetSlider.Maximum);
                                previewMainGrid.Children.Add(xOffsetSlider);
                                previewMainGrid.SetColumn(xOffsetSlider, 1);
                                previewMainGrid.SetRow(xOffsetSlider, 0);

                                var yOffsetSlider = new Slider();
                                yOffsetSlider.Orientation = Orientation.Vertical;
                                yOffsetSlider.HorizontalAlignment = HorizontalAlignment.Center;
                                yOffsetSlider.Margin = new Thickness(0, borderWidth / 3, 0, 0);
                                yOffsetSlider.BarThickness = 24;
                                yOffsetSlider.Interval = 0.5f;
                                yOffsetSlider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                yOffsetSlider.ValueChanged += this.UpdateExampleCurrent;
                                context.TwoWayBinds(() => this._viewModel.CharOffsetY, () => yOffsetSlider.Value);
                                context.OneWayBinds(() => this._viewModel.MinCharOffsetY, () => yOffsetSlider.Minimum);
                                context.OneWayBinds(() => this._viewModel.MaxCharOffsetY, () => yOffsetSlider.Maximum);
                                previewMainGrid.Children.Add(yOffsetSlider);
                                previewMainGrid.SetColumn(yOffsetSlider, 0);
                                previewMainGrid.SetRow(yOffsetSlider, 1);

                                context.OneWayBinds(() => this._viewModel.IsTuningCharOffset, () => xOffsetSlider.Visibility, new BooleanVisibilityConverter(Visibility.Hidden));
                                context.OneWayBinds(() => this._viewModel.IsTuningCharOffset, () => yOffsetSlider.Visibility, new BooleanVisibilityConverter(Visibility.Hidden));

                                var previewExampleGrid = new FontPreviewGrid(borderWidth);
                                previewExampleGrid.Margin = new Thickness(borderWidth / 3, borderWidth / 3, 0, 0);
                                context.OneWayBinds(() => this._viewModel.ExamplesMerged, () => previewExampleGrid.IsMerged);
                                context.OneWayBinds(() => this._viewModel.ShowExampleBounds, () => previewExampleGrid.VanillaFontExample.ShowBounds);
                                context.OneWayBinds(() => this._viewModel.ShowExampleBounds, () => previewExampleGrid.CurrentFontExample.ShowBounds);
                                context.OneWayBinds(() => this._viewModel.ShowExampleText, () => previewExampleGrid.VanillaFontExample.ShowText);
                                context.OneWayBinds(() => this._viewModel.ShowExampleText, () => previewExampleGrid.CurrentFontExample.ShowText);
                                context.OneWayBinds(() => this._viewModel.ExampleText, () => previewExampleGrid.VanillaFontExample.Text);
                                context.OneWayBinds(() => this._viewModel.ExampleText, () => previewExampleGrid.CurrentFontExample.Text);
                                context.OneWayBinds(() => this._viewModel.ExampleVanillaFont, () => previewExampleGrid.VanillaFontExample.Font);
                                context.OneWayBinds(() => this._viewModel.ExampleCurrentFont, () => previewExampleGrid.CurrentFontExample.Font);
                                previewMainGrid.Children.Add(previewExampleGrid);
                                previewMainGrid.SetColumn(previewExampleGrid, 1);
                                previewMainGrid.SetRow(previewExampleGrid, 1);
                            }

                            var previewOptionsStack = new StackContainer();
                            previewOptionsStack.Orientation = Orientation.Vertical;
                            previewGrid.Children.Add(previewOptionsStack);
                            previewGrid.SetColumn(previewOptionsStack, 2);
                            {
                                var mergeOption = new StackContainer();
                                mergeOption.Orientation = Orientation.Horizontal;
                                mergeOption.Margin = new Thickness(0, 0, 0, borderWidth / 3);
                                previewOptionsStack.Children.Add(mergeOption);
                                {
                                    var checkbox = new Checkbox();
                                    context.TwoWayBinds(() => this._viewModel.ExamplesMerged, () => checkbox.IsChecked);

                                    var label = new Label();
                                    label.Text = I18n.OptionsPage_MergeExamples();
                                    label.Font = FontType.SmallFont;
                                    label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                    mergeOption.Children.Add(checkbox);
                                    mergeOption.Children.Add(label);
                                }

                                var showBoundsOption = new StackContainer();
                                showBoundsOption.Orientation = Orientation.Horizontal;
                                showBoundsOption.Margin = new Thickness(0, 0, 0, borderWidth / 3);
                                previewOptionsStack.Children.Add(showBoundsOption);
                                {
                                    var checkbox = new Checkbox();
                                    context.TwoWayBinds(() => this._viewModel.ShowExampleBounds, () => checkbox.IsChecked);

                                    var label = new Label();
                                    label.Text = I18n.OptionsPage_ShowExampleBounds();
                                    label.Font = FontType.SmallFont;
                                    label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                    showBoundsOption.Children.Add(checkbox);
                                    showBoundsOption.Children.Add(label);
                                }

                                var showTextOption = new StackContainer();
                                showTextOption.Orientation = Orientation.Horizontal;
                                showTextOption.Margin = new Thickness(0, 0, 0, borderWidth / 3);
                                previewOptionsStack.Children.Add(showTextOption);
                                {
                                    var checkbox = new Checkbox();
                                    context.TwoWayBinds(() => this._viewModel.ShowExampleText, () => checkbox.IsChecked);

                                    var label = new Label();
                                    label.Text = I18n.OptionsPage_ShowExampleText();
                                    label.Font = FontType.SmallFont;
                                    label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                    showTextOption.Children.Add(checkbox);
                                    showTextOption.Children.Add(label);
                                }
                            }
                        }
                    }

                    var presetGrid = new Grid();
                    presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                    presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    presetGrid.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                    mainGrid.Children.Add(presetGrid);
                    mainGrid.SetRow(presetGrid, 2);
                    {
                        var presetLabel = new Label();
                        presetLabel.Font = FontType.DialogueFont;
                        presetLabel.HorizontalAlignment = HorizontalAlignment.Left;
                        presetLabel.VerticalAlignment = VerticalAlignment.Center;
                        context.OneWayBinds(() => this._viewModel.CurrentPresetName, () => presetLabel.Text);
                        presetGrid.Children.Add(presetLabel);
                        presetGrid.SetColumn(presetLabel, 0);

                        var prevPresetButton = new TextureButton(
                            Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
                        prevPresetButton.Margin = new Thickness(0, borderWidth / 2, borderWidth / 3, borderWidth / 2);
                        prevPresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                        prevPresetButton.VerticalAlignment = VerticalAlignment.Center;
                        prevPresetButton.ToolTipText = I18n.Ui_Tooltip_PrevPreset();
                        prevPresetButton.Click += (_, _) => Game1.playSound("smallSelect");
                        context.OneWayBinds(() => this._viewModel.MoveToPrevPreset, () => prevPresetButton.Command);
                        presetGrid.Children.Add(prevPresetButton);
                        presetGrid.SetColumn(prevPresetButton, 1);

                        var nextPresetButton = new TextureButton(
                            Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
                        nextPresetButton.Margin = new Thickness(0, borderWidth / 2, borderWidth / 3, borderWidth / 2);
                        nextPresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                        nextPresetButton.VerticalAlignment = VerticalAlignment.Center;
                        nextPresetButton.ToolTipText = I18n.Ui_Tooltip_NextPreset();
                        nextPresetButton.Click += (_, _) => Game1.playSound("smallSelect");
                        context.OneWayBinds(() => this._viewModel.MoveToNextPreset, () => nextPresetButton.Command);
                        presetGrid.Children.Add(nextPresetButton);
                        presetGrid.SetColumn(nextPresetButton, 2);

                        var newPresetButton = new TextureButton(
                            Game1.mouseCursors, new Rectangle(0, 428, 10, 10), 4f);
                        newPresetButton.Margin = new Thickness(0, borderWidth / 2, borderWidth / 3, borderWidth / 2);
                        newPresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                        newPresetButton.VerticalAlignment = VerticalAlignment.Center;
                        newPresetButton.ToolTipText = I18n.Ui_Tooltip_NewPreset();
                        newPresetButton.Click += (_, _) => Game1.playSound("coin");
                        context.OneWayBinds(() => this._viewModel.CanSaveCurrentAsNewPreset, () => newPresetButton.GreyedOut, new TrueFalseConverter());
                        //context.OneWayBinds(() => this._viewModel.MoveToNextPreset, () => newPresetButton.Command);  // TODO
                        presetGrid.Children.Add(newPresetButton);
                        presetGrid.SetColumn(newPresetButton, 3);

                        var savePresetButton = new TextureButton(
                            Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 3f);
                        savePresetButton.Margin = new Thickness(0, borderWidth / 2, borderWidth / 3, borderWidth / 2);
                        savePresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                        savePresetButton.VerticalAlignment = VerticalAlignment.Center;
                        savePresetButton.ToolTipText = I18n.Ui_Tooltip_SavePreset();
                        savePresetButton.Click += (_, _) => Game1.playSound("newRecipe");
                        context.OneWayBinds(() => this._viewModel.CanSaveCurrentPreset, () => savePresetButton.GreyedOut, new TrueFalseConverter());
                        context.OneWayBinds(() => this._viewModel.SaveCurrentPreset, () => savePresetButton.Command);
                        presetGrid.Children.Add(savePresetButton);
                        presetGrid.SetColumn(savePresetButton, 4);

                        var deletePresetButton = new TextureButton(
                            Game1.mouseCursors, new Rectangle(192, 256, 64, 64), 0.75f);
                        deletePresetButton.Margin = new Thickness(0, borderWidth / 2, 0, borderWidth / 2);
                        deletePresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                        deletePresetButton.VerticalAlignment = VerticalAlignment.Center;
                        deletePresetButton.ToolTipText = I18n.Ui_Tooltip_DelPreset();
                        deletePresetButton.Click += (_, _) => Game1.playSound("trashcan");
                        context.OneWayBinds(() => this._viewModel.CanDeleteCurrentPreset, () => deletePresetButton.GreyedOut, new TrueFalseConverter());
                        context.OneWayBinds(() => this._viewModel.DeleteCurrentPreset, () => deletePresetButton.Command);
                        presetGrid.Children.Add(deletePresetButton);
                        presetGrid.SetColumn(deletePresetButton, 5);
                    }

                    var settingsGrid = new Grid();
                    settingsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                    settingsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    mainGrid.Children.Add(settingsGrid);
                    mainGrid.SetRow(settingsGrid, 3);
                    {
                        var leftGrid = new Grid();
                        leftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                        leftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                        leftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                        leftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                        settingsGrid.Children.Add(leftGrid);
                        settingsGrid.SetColumn(leftGrid, 0);
                        {
                            var enableOption = new StackContainer();
                            enableOption.Orientation = Orientation.Horizontal;
                            enableOption.VerticalAlignment = VerticalAlignment.Center;
                            leftGrid.Children.Add(enableOption);
                            leftGrid.SetRow(enableOption, 0);
                            {
                                var checkbox = new Checkbox();
                                checkbox.Checked += this.UpdateExampleCurrent;
                                checkbox.Unchecked += this.UpdateExampleCurrent;
                                context.TwoWayBinds(() => this._viewModel.FontEnabled, () => checkbox.IsChecked);

                                var label = new Label();
                                label.Font = FontType.SmallFont;
                                label.Text = I18n.OptionsPage_Enable();
                                label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                enableOption.Children.Add(checkbox);
                                enableOption.Children.Add(label);
                            }

                            var fontSizeOption = new StackContainer();
                            fontSizeOption.Orientation = Orientation.Horizontal;
                            fontSizeOption.VerticalAlignment = VerticalAlignment.Center;
                            leftGrid.Children.Add(fontSizeOption);
                            leftGrid.SetRow(fontSizeOption, 1);
                            {
                                var slider = new Slider();
                                slider.Interval = 1;
                                slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                slider.BarLength = 300;
                                slider.ValueChanged += this.UpdateExampleCurrent;
                                context.TwoWayBinds(() => this._viewModel.FontSize, () => slider.Value);
                                context.OneWayBinds(() => this._viewModel.MinFontSize, () => slider.Minimum);
                                context.OneWayBinds(() => this._viewModel.MaxFontSize, () => slider.Maximum);

                                var label = new Label();
                                label.Font = FontType.SmallFont;
                                label.Text = I18n.OptionsPage_FontSize();
                                label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                fontSizeOption.Children.Add(slider);
                                fontSizeOption.Children.Add(label);
                            }

                            var spacingOption = new StackContainer();
                            spacingOption.Orientation = Orientation.Horizontal;
                            spacingOption.VerticalAlignment = VerticalAlignment.Center;
                            leftGrid.Children.Add(spacingOption);
                            leftGrid.SetRow(spacingOption, 2);
                            {
                                var slider = new Slider();
                                slider.Interval = 1;
                                slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                slider.BarLength = 300;
                                slider.ValueChanged += this.UpdateExampleCurrent;
                                context.TwoWayBinds(() => this._viewModel.Spacing, () => slider.Value);
                                context.OneWayBinds(() => this._viewModel.MinSpacing, () => slider.Minimum);
                                context.OneWayBinds(() => this._viewModel.MaxSpacing, () => slider.Maximum);

                                var label = new Label();
                                label.Font = FontType.SmallFont;
                                label.Text = I18n.OptionsPage_Spacing();
                                label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                spacingOption.Children.Add(slider);
                                spacingOption.Children.Add(label);
                            }

                            var lineSpacingOption = new StackContainer();
                            lineSpacingOption.Orientation = Orientation.Horizontal;
                            lineSpacingOption.VerticalAlignment = VerticalAlignment.Center;
                            leftGrid.Children.Add(lineSpacingOption);
                            leftGrid.SetRow(lineSpacingOption, 3);
                            {
                                var slider = new Slider();
                                slider.Interval = 1;
                                slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                slider.BarLength = 300;
                                slider.ValueChanged += this.UpdateExampleCurrent;
                                context.TwoWayBinds(() => this._viewModel.LineSpacing, () => slider.Value);
                                context.OneWayBinds(() => this._viewModel.MinLineSpacing, () => slider.Minimum);
                                context.OneWayBinds(() => this._viewModel.MaxLineSpacing, () => slider.Maximum);

                                var label = new Label();
                                label.Font = FontType.SmallFont;
                                label.Text = I18n.OptionsPage_LineSpacing();
                                label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                lineSpacingOption.Children.Add(slider);
                                lineSpacingOption.Children.Add(label);
                            }
                        }

                        var rightGrid = new Grid();
                        rightGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        rightGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
                        settingsGrid.Children.Add(rightGrid);
                        settingsGrid.SetColumn(rightGrid, 1);
                        {
                            var rightTopGrid = new Grid();
                            rightTopGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            rightTopGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                            rightGrid.Children.Add(rightTopGrid);
                            rightGrid.SetRow(rightTopGrid, 0);
                            {
                                var refreshButton = new RefreshButton(2.5f);
                                refreshButton.ToolTipText = I18n.Ui_Tooltip_RefreshFonts();
                                refreshButton.AnimationDuration = 300;
                                refreshButton.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                refreshButton.Click += (_, _) => Game1.playSound("trashcan");
                                context.OneWayBinds(() => this._viewModel.RefreshFonts, () => refreshButton.Command);
                                rightTopGrid.Children.Add(refreshButton);
                                rightTopGrid.SetColumn(refreshButton, 0);

                                var fontDropDown = new ComboBox();
                                fontDropDown.SettableWidth = 400;
                                fontDropDown.VerticalAlignment = VerticalAlignment.Center;
                                fontDropDown.DisplayTextReslover = this.DisplayFontOnComboBox;
                                fontDropDown.EqualityComparer = new FontEqualityComparer();
                                fontDropDown.MaxDisplayRows = 6;
                                context.OneWayBinds(() => this._viewModel.AllFonts, () => fontDropDown.ItemsSource);
                                context.TwoWayBinds(() => this._viewModel.CurrentFont, () => fontDropDown.SelectedItem);
                                fontDropDown.SelectionChanged += this.UpdateExampleCurrent;
                                rightTopGrid.Children.Add(fontDropDown);
                                rightTopGrid.SetColumn(fontDropDown, 1);
                            }

                            var okButton = new TextureButton(
                                Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46));
                            okButton.HorizontalAlignment = HorizontalAlignment.Right;
                            okButton.VerticalAlignment = VerticalAlignment.Bottom;
                            context.OneWayBinds(() => this._viewModel.CanGenerateFont, () => okButton.GreyedOut, new TrueFalseConverter());
                            okButton.Click += this.OkButtonClicked;
                            rightGrid.Children.Add(okButton);
                            rightGrid.SetRow(okButton, 1);
                        }
                    }
                }
            }

            context.SetContent(grid);
        }

        protected override void ResetComponents(RootElement root, IBindingContext context)
        {
            { /*
            root.LocalPosition = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen);

            this._button_prevFontType = new TextureButton(
                Game1.mouseCursors, new(352, 495, 12, 11), 4f);
            this._button_prevFontType.LocalPosition = new Vector2(-48 - 48, this.height / 2);
            this._button_prevFontType.SettableWidth = 48;
            this._button_prevFontType.SettableHeight = 44;
            this._button_prevFontType.Click += (_, _) => Game1.playSound("smallSelect");

            this._button_nextFontType = new TextureButton(
                Game1.mouseCursors, new(365, 495, 12, 11), 4f);
            this._button_nextFontType.LocalPosition = new Vector2(this.width + 48, this.height / 2);
            this._button_nextFontType.SettableWidth = 48;
            this._button_nextFontType.SettableHeight = 44;
            this._button_nextFontType.Click += (_, _) => Game1.playSound("smallSelect");

            this._label_title = new Label();
            this._label_title.Font = FontType.SpriteText;
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

            this._label_game = new Label();
            this._label_game.Text = I18n.OptionsPage_OriginalExample();

            this._label_current = new Label();
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
            this._label_gameExample.Forground = this._gameExampleColor;
            this._label_gameExample.BoundsColor = Color.Red * 0.5f;
            this._label_gameExample.ShowBounds = this._box_showBounds.Element.IsChecked;
            this._label_gameExample.ShowText = this._box_showText.Element.IsChecked;

            this._label_currentExample = new FontExampleLabel();
            this._label_currentExample.Forground = this._customExampleColor;
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
            this._button_offsetTuning.ToolTipText = I18n.Ui_Tooltip_ToggleCharOffsetTuning();
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
                this._label_currentPreset = new Label();
                this._label_currentPreset.LocalPosition = new Vector2(exampleBoardX, presetSectionY);

                this._button_delete = new TextureButton(
                    Game1.mouseCursors, new Rectangle(192, 256, 64, 64), scale_delete);
                this._button_delete.ToolTipText = I18n.Ui_Tooltip_DelPreset();
                this._button_delete.LocalPosition = new Vector2(this._exampleBoard.LocalPosition.X + this._exampleBoard.Width - size_delete.X, presetSectionY + presetSectionMaxHeight / 2 - size_delete.Y / 2);
                this._button_delete.SettableWidth = (int)size_delete.X;
                this._button_delete.SettableHeight = (int)size_delete.Y;
                this._button_delete.Click += (_, _) => Game1.playSound("trashcan");

                this._button_save = new TextureButton(
                    Game1.mouseCursors, new Rectangle(274, 284, 16, 16), scale_save);
                this._button_save.ToolTipText = I18n.Ui_Tooltip_SavePreset();
                this._button_save.LocalPosition = new Vector2(this._button_delete.LocalPosition.X - borderWidth / 3 - size_save.X, presetSectionY + presetSectionMaxHeight / 2 - size_save.Y / 2);
                this._button_save.SettableWidth = (int)size_save.X;
                this._button_save.SettableHeight = (int)size_save.Y;
                this._button_save.Click += (_, _) => Game1.playSound("newRecipe");

                this._button_new = new TextureButton(
                    Game1.mouseCursors, new Rectangle(0, 428, 10, 10), scale_new);
                this._button_new.ToolTipText = I18n.Ui_Tooltip_NewPreset();
                this._button_new.LocalPosition = new Vector2(this._button_save.LocalPosition.X - borderWidth / 3 - size_new.X, presetSectionY + presetSectionMaxHeight / 2 - size_new.Y / 2);
                this._button_new.SettableWidth = (int)size_new.X;
                this._button_new.SettableHeight = (int)size_new.Y;
                this._button_new.Click += this.NewPresetButtonClicked;

                this._button_nextPreset = new TextureButton(
                    Game1.mouseCursors, new Rectangle(365, 495, 12, 11), scale_next);
                this._button_nextPreset.ToolTipText = I18n.Ui_Tooltip_NextPreset();
                this._button_nextPreset.LocalPosition = new Vector2(this._button_new.LocalPosition.X - borderWidth / 3 - size_next.X, presetSectionY + presetSectionMaxHeight / 2 - size_next.Y / 2);
                this._button_nextPreset.SettableWidth = (int)size_next.X;
                this._button_nextPreset.SettableHeight = (int)size_next.Y;
                this._button_nextPreset.Click += (_, _) => Game1.playSound("smallSelect");

                this._button_prevPreset = new TextureButton(
                    Game1.mouseCursors, new Rectangle(352, 495, 12, 11), scale_prev);
                this._button_prevPreset.ToolTipText = I18n.Ui_Tooltip_PrevPreset();
                this._button_prevPreset.LocalPosition = new Vector2(this._button_nextPreset.LocalPosition.X - borderWidth / 3 - size_prev.X, presetSectionY + presetSectionMaxHeight / 2 - size_prev.Y / 2);
                this._button_prevPreset.SettableWidth = (int)size_prev.X;
                this._button_prevPreset.SettableHeight = (int)size_prev.Y;
                this._button_prevPreset.Click += (_, _) => Game1.playSound("smallSelect");

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
            this._button_refresh.ToolTipText = I18n.Ui_Tooltip_RefreshFonts();
            this._button_refresh.AnimationDuration = 300;
            this._button_refresh.LocalPosition = new Vector2(this._dropDown_font.LocalPosition.X - borderWidth / 3 - refreshWidth, this._dropDown_font.LocalPosition.Y + this._dropDown_font.Height / 2 - refreshHeight / 2);
            this._button_refresh.SettableWidth = refreshWidth;
            this._button_refresh.SettableHeight = refreshHeight;
            this._button_refresh.Click += (_, _) => Game1.playSound("trashcan");

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

            // commands
            context.AddBinding(() => this._viewModel.MoveToPrevFont, () => this._button_prevFontType.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.MoveToNextFont, () => this._button_nextFontType.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.MoveToPrevPreset, () => this._button_prevPreset.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.MoveToNextPreset, () => this._button_nextPreset.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.SaveCurrentPreset, () => this._button_save.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.DeleteCurrentPreset, () => this._button_delete.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.RefreshFonts, () => this._button_refresh.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.SaveCurrentPreset, () => this._button_save.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.SaveCurrentPreset, () => this._button_save.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.SaveCurrentPreset, () => this._button_save.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.SaveCurrentPreset, () => this._button_save.Command, BindingMode.OneWay);
            context.AddBinding(() => this._viewModel.SaveCurrentPreset, () => this._button_save.Command, BindingMode.OneWay);

#pragma warning restore format*/
            }
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
            //// 如果是独立菜单，需要画一个背景框。
            //if (this._isStandalone)
            //    Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);

            base.draw(b);

#if DEBUG
            b.DrawString(Game1.smallFont, $"Enabled: {this._viewModel.FontEnabled}\n"
                + $"Size: {this._viewModel.FontSize}\n"
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

        private string DisplayFontOnComboBox(object[] source, object item)
        {
            FontModel font = item as FontModel;

            if (font.FullPath == null)
                return I18n.OptionsPage_Font_KeepOrig();
            else
                return $"{font.FamilyName} ({font.SubfamilyName})";
        }

        private NewPresetMenu CreateNewPresetMenu()
        {
            var result = new NewPresetMenu(
                this._presetManager,
                this.xPositionOnScreen + this.width / 4,
                this.yPositionOnScreen + this.height / 3,
                this.width / 2,
                this.height / 3);
            result.Accepted += (_, name) => this._viewModel._SaveCurrentAsNewPreset(name);

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
