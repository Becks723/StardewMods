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

        private TextureBoxBorder _previewBoard;

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
                this._previewBoard.Box = TextureBoxes.Default;  // 解决方法：将背景框改成默认款。
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
            Grid grid = new Grid();
            grid.SuggestedWidth = 1000;
            grid.SuggestedHeight = 600;
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace /*new GridLength(800 + IClickableMenu.borderWidth * 2)*/ });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var prevFontButton = new TextureButton(
                Game1.mouseCursors, new(352, 495, 12, 11), 4f);
            prevFontButton.HorizontalAlignment = HorizontalAlignment.Left;
            prevFontButton.VerticalAlignment = VerticalAlignment.Center;
            prevFontButton.Margin = new Thickness(0, 0, 48, 0);
            context.OneWayBinds(() => this._viewModel.MoveToPrevFont, () => prevFontButton.Command);
            prevFontButton.ClickSound = "smallSelect";
            grid.Children.Add(prevFontButton);
            grid.SetColumn(prevFontButton, 0);

            var nextFontButton = new TextureButton(
                Game1.mouseCursors, new(365, 495, 12, 11), 4f);
            nextFontButton.HorizontalAlignment = HorizontalAlignment.Right;
            nextFontButton.VerticalAlignment = VerticalAlignment.Center;
            nextFontButton.Margin = new Thickness(48, 0, 0, 0);
            context.OneWayBinds(() => this._viewModel.MoveToNextFont, () => nextFontButton.Command);
            nextFontButton.ClickSound = "smallSelect";
            grid.Children.Add(nextFontButton);
            grid.SetColumn(nextFontButton, 2);

            var mainBorder = new TextureBoxBorder();
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
                mainBorder.Child = mainGrid;
                {
                    var titleLabel = new Label();
                    titleLabel.Font = FontType.SpriteText;
                    titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    titleLabel.VerticalAlignment = VerticalAlignment.Center;
                    titleLabel.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                    context.OneWayBinds(() => this._viewModel.Title, () => titleLabel.Text);
                    mainGrid.Children.Add(titleLabel);
                    mainGrid.SetRow(titleLabel, 0);

                    var previewBorder = this._previewBoard = new TextureBoxBorder();
                    previewBorder.Box = TextureBoxes.DefaultBorderless;
                    previewBorder.DrawShadow = false;
                    previewBorder.Padding += new Thickness(borderWidth / 3);
                    mainGrid.Children.Add(previewBorder);
                    mainGrid.SetRow(previewBorder, 1);
                    {
                        Grid previewGrid = new Grid();
                        previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                        previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(borderWidth / 2) });
                        previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        previewBorder.Child = previewGrid;
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
                                toggleOffsetButton.ClickSound = "smallSelect";
                                context.TwoWayBinds(() => this._viewModel.IsTuningCharOffset, () => toggleOffsetButton.IsToggled);
                                previewMainGrid.Children.Add(toggleOffsetButton);
                                previewMainGrid.SetColumn(toggleOffsetButton, 0);
                                previewMainGrid.SetRow(toggleOffsetButton, 0);

                                var xOffsetSlider = new Slider();
                                xOffsetSlider.Orientation = Orientation.Horizontal;
                                xOffsetSlider.VerticalAlignment = VerticalAlignment.Center;
                                xOffsetSlider.Margin = new Thickness(borderWidth / 3, 0, 0, 0);
                                xOffsetSlider.SuggestedHeight = 24;
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
                                yOffsetSlider.SuggestedWidth = 24;
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
                                    var checkbox = new CheckBox();
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
                                    var checkbox = new CheckBox();
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
                                    var checkbox = new CheckBox();
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
                        prevPresetButton.ClickSound = "smallSelect";
                        context.OneWayBinds(() => this._viewModel.MoveToPrevPreset, () => prevPresetButton.Command);
                        presetGrid.Children.Add(prevPresetButton);
                        presetGrid.SetColumn(prevPresetButton, 1);

                        var nextPresetButton = new TextureButton(
                            Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
                        nextPresetButton.Margin = new Thickness(0, borderWidth / 2, borderWidth / 3, borderWidth / 2);
                        nextPresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                        nextPresetButton.VerticalAlignment = VerticalAlignment.Center;
                        nextPresetButton.ToolTipText = I18n.Ui_Tooltip_NextPreset();
                        nextPresetButton.ClickSound = "smallSelect";
                        context.OneWayBinds(() => this._viewModel.MoveToNextPreset, () => nextPresetButton.Command);
                        presetGrid.Children.Add(nextPresetButton);
                        presetGrid.SetColumn(nextPresetButton, 2);

                        var newPresetButton = new TextureButton(
                            Game1.mouseCursors, new Rectangle(0, 428, 10, 10), 4f);
                        newPresetButton.Margin = new Thickness(0, borderWidth / 2, borderWidth / 3, borderWidth / 2);
                        newPresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                        newPresetButton.VerticalAlignment = VerticalAlignment.Center;
                        newPresetButton.ToolTipText = I18n.Ui_Tooltip_NewPreset();
                        newPresetButton.ClickSound = "coin";
                        context.OneWayBinds(() => this._viewModel.CanSaveCurrentAsNewPreset, () => newPresetButton.GreyedOut, new TrueFalseConverter());
                        context.OneWayBinds(() => this._viewModel.SaveCurrentAsNewPreset, () => newPresetButton.Command);
                        context.OneWayBinds<Func<IOverlayMenu>, object>(() => this.CreateNewPresetMenu, () => newPresetButton.CommandParameter);
                        presetGrid.Children.Add(newPresetButton);
                        presetGrid.SetColumn(newPresetButton, 3);

                        var savePresetButton = new TextureButton(
                            Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 3f);
                        savePresetButton.Margin = new Thickness(0, borderWidth / 2, borderWidth / 3, borderWidth / 2);
                        savePresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                        savePresetButton.VerticalAlignment = VerticalAlignment.Center;
                        savePresetButton.ToolTipText = I18n.Ui_Tooltip_SavePreset();
                        savePresetButton.ClickSound = "newRecipe";
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
                        deletePresetButton.ClickSound = "trashcan";
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
                            enableOption.HorizontalAlignment = HorizontalAlignment.Left;
                            enableOption.VerticalAlignment = VerticalAlignment.Center;
                            leftGrid.Children.Add(enableOption);
                            leftGrid.SetRow(enableOption, 0);
                            {
                                var checkbox = new CheckBox();
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
                            fontSizeOption.HorizontalAlignment = HorizontalAlignment.Left;
                            fontSizeOption.VerticalAlignment = VerticalAlignment.Center;
                            leftGrid.Children.Add(fontSizeOption);
                            leftGrid.SetRow(fontSizeOption, 1);
                            {
                                var slider = new Slider();
                                slider.Interval = 1;
                                slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                slider.SuggestedWidth = 300;
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
                            spacingOption.HorizontalAlignment = HorizontalAlignment.Left;
                            spacingOption.VerticalAlignment = VerticalAlignment.Center;
                            leftGrid.Children.Add(spacingOption);
                            leftGrid.SetRow(spacingOption, 2);
                            {
                                var slider = new Slider();
                                slider.Interval = 1;
                                slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                slider.SuggestedWidth = 300;
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
                            lineSpacingOption.HorizontalAlignment = HorizontalAlignment.Left;
                            lineSpacingOption.VerticalAlignment = VerticalAlignment.Center;
                            leftGrid.Children.Add(lineSpacingOption);
                            leftGrid.SetRow(lineSpacingOption, 3);
                            {
                                var slider = new Slider();
                                slider.Interval = 1;
                                slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                slider.SuggestedWidth = 300;
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
                                refreshButton.ClickSound = "trashcan";
                                context.OneWayBinds(() => this._viewModel.RefreshFonts, () => refreshButton.Command);
                                rightTopGrid.Children.Add(refreshButton);
                                rightTopGrid.SetColumn(refreshButton, 0);

                                var fontDropDown = new ComboBox();
                                fontDropDown.SuggestedWidth = 400;
                                fontDropDown.VerticalAlignment = VerticalAlignment.Center;
                                fontDropDown.ItemAppearance = Appearance.ForData(new FontAppearance());
                                context.OneWayBinds(() => this._viewModel.AllFonts, () => fontDropDown.ItemsSource);
                                context.TwoWayBinds(() => this._viewModel.CurrentFont, () => fontDropDown.SelectedItem);
                                //fontDropDown.SelectionChanged += this.UpdateExampleCurrent;
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

            context.SetRootElement(grid);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this._isNewPresetMenu)
                this._newPresetMenu.Dispose();
        }

        public override void update(GameTime time)
        {
            if (!this._isNewPresetMenu)
                base.update(time);
            else
            {
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
                this._newPresetMenu.draw(b);
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);

            if (this._isNewPresetMenu)
            {
                this._newPresetMenu.receiveKeyPress(key);
            }
        }

        protected override bool CanClose()
        {
            if (!this._isNewPresetMenu)
                return true;
            else
                return this._newPresetMenu.readyToClose();
        }

        private string DisplayFontOnComboBox(FontModel? font)
        {
            if (font == null)
                return string.Empty;

            if (font.FullPath == null)
                return I18n.OptionsPage_Font_KeepOrig();
            else
                return $"{font.FamilyName} ({font.SubfamilyName})";
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
            void OnMenuOpened(NewPresetMenu menu)
            {
                this._isNewPresetMenu = true;
                this._newPresetMenu = menu;
            }

            void OnMenuClosed(NewPresetMenu menu)
            {
                menu.Dispose();

                this._isNewPresetMenu = false;
                this._newPresetMenu = null;
            }

            var result = new NewPresetMenu(
                this._presetManager,
                OnMenuOpened,
                OnMenuClosed);

            return result;
        }

        /// <summary>一些在单例<see cref="Instance"/>更新前（单例在每次创建实例时更新）要做的事。</summary>
        /// <param name="instance">上一个实例<see cref="Instance"/>。</param>
        private void BeforeUpdateSingleton(FontSettingsPage instance)
        {
            // 首次创建实例，直接返回。
            if (instance == null) return;

            this._isNewPresetMenu = instance._isNewPresetMenu;
            this._newPresetMenu = instance._newPresetMenu;
        }

        private class FontAppearance : DataAppearanceBuilder<FontModel>
        {
            protected override Element Build(AppearanceBuildContext<FontModel> context)
            {
                FontModel? font = context.Target;

                Label l = new Label();
                l.Text = GetText(font);
                l.HorizontalAlignment = HorizontalAlignment.Left;
                l.VerticalAlignment = VerticalAlignment.Center;
                return l;
            }

            private string GetText(FontModel? font)
            {
                if (font == null)
                    return string.Empty;

                if (font.FullPath == null)
                    return I18n.OptionsPage_Font_KeepOrig();
                else
                    return $"{font.FamilyName} ({font.SubfamilyName})";
            }
        }
    }
}
