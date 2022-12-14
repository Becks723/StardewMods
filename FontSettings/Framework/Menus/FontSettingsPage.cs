﻿using System;
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
using StardewValleyUI.Controls.Primitives;
using StardewValleyUI.Data;
using StardewValleyUI.Data.Converters;
using StardewValleyUI.Menus;

namespace FontSettings.Framework.Menus
{
    internal class FontSettingsPage : BaseMenu<FontSettingsMenuModel>
    {
        private readonly FontPresetManager _presetManager;
        private readonly IModRegistry _registry;
        private readonly FontSettingsMenuModel _viewModel;
        private readonly Texture2D _icons;

        private bool _isNewPresetMenu;
        private NewPresetMenu _newPresetMenu;

        public FontSettingsPage(ModConfig config, FontManager fontManager, IAsyncGameFontChanger fontChanger, FontPresetManager presetManager, Action<FontConfigs> saveFontSettings, IModRegistry registry)
        {
            this._presetManager = presetManager;
            this._registry = registry;
            this._icons = Textures.Icons;

            this.ResetComponents();

            this._viewModel = new FontSettingsMenuModel(config, fontManager, fontChanger, presetManager, saveFontSettings);
            this.DataContext = this._viewModel;
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
            Grid rootGrid = new Grid();
            rootGrid.SuggestedWidth = 1000;
            rootGrid.SuggestedHeight = 600;
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
            {
                var titleBorder = new TextureBoxBorder();
                titleBorder.DrawShadow = false;
                titleBorder.Padding += new Thickness(0, borderWidth / 3);
                rootGrid.Children.Add(titleBorder);
                rootGrid.SetRow(titleBorder, 0);
                {
                    var titleLabel = new Label();
                    titleLabel.Font = FontType.SpriteText;
                    titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    titleLabel.VerticalAlignment = VerticalAlignment.Center;
                    titleLabel.Text = I18n.Ui_MainMenu_Title();
                    titleBorder.Child = titleLabel;
                }

                Grid mainGrid = new Grid();
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnit.Percent) });
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnit.Percent) });
                rootGrid.Children.Add(mainGrid);
                rootGrid.SetRow(mainGrid, 1);
                {
                    var scrollViewer = new ScrollViewer();
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    scrollViewer.Appearance = Appearance.ForControl<ScrollViewer>(ctx =>
                    {
                        var context = ctx;
                        var scrollViewer = context.Target;

                        Grid grid = new Grid();
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        {
                            // added.
                            var backgroundBorder = new TextureBoxBorder();
                            backgroundBorder.Box = TextureBoxes.ThickBorder;
                            backgroundBorder.DrawShadow = false;
                            grid.Children.Add(backgroundBorder);
                            grid.SetColumn(backgroundBorder, 0);
                            grid.SetRow(backgroundBorder, 0);
                            {
                                var contentPresenter = new ScrollContentPresenter();
                                context.DefinePart("PART_CONTENT", contentPresenter);
                                context.DefinePart("PART_ContentPresenter", contentPresenter);
                                backgroundBorder.Child = contentPresenter;
                            }

                            ScrollBar vertiScrollBar = new ScrollBar();
                            vertiScrollBar.Orientation = Orientation.Vertical;
                            context.OneWayBinds(() => scrollViewer.ActualVerticalScrollBarVisibility, () => vertiScrollBar.Visibility);
                            context.OneWayBinds(() => scrollViewer.VerticalOffset, () => vertiScrollBar.Value);
                            context.OneWayBinds(() => scrollViewer.ScrollableHeight, () => vertiScrollBar.Maximum);
                            context.OneWayBinds(() => scrollViewer.ViewportHeight, () => vertiScrollBar.Viewport);
                            context.DefinePart("PART_VSCROLLBAR", vertiScrollBar);
                            grid.Children.Add(vertiScrollBar);
                            grid.SetColumn(vertiScrollBar, 1);
                            grid.SetRow(vertiScrollBar, 0);

                            ScrollBar horizScrollBar = new ScrollBar();
                            horizScrollBar.Orientation = Orientation.Horizontal;
                            context.OneWayBinds(() => scrollViewer.ActualHorizontalScrollBarVisibility, () => horizScrollBar.Visibility);
                            context.OneWayBinds(() => scrollViewer.HorizontalOffset, () => horizScrollBar.Value);
                            context.OneWayBinds(() => scrollViewer.ScrollableWidth, () => horizScrollBar.Maximum);
                            context.OneWayBinds(() => scrollViewer.ViewportWidth, () => horizScrollBar.Viewport);
                            context.DefinePart("PART_HSCROLLBAR", horizScrollBar);
                            grid.Children.Add(horizScrollBar);
                            grid.SetColumn(horizScrollBar, 0);
                            grid.SetRow(horizScrollBar, 1);
                        }
                        return grid;
                    });
                    mainGrid.Children.Add(scrollViewer);
                    mainGrid.SetColumn(scrollViewer, 1);
                    {
                        var stack = new StackContainer();
                        stack.Orientation = Orientation.Vertical;
                        stack.Margin = new Thickness(borderWidth / 2);
                        scrollViewer.Content = stack;
                        {
                            Grid fontTypeGrid = new Grid();
                            fontTypeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            fontTypeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                            fontTypeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            fontTypeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            stack.Children.Add(fontTypeGrid);
                            {
                                var prevFontButton = new TextureButton(
                                    Game1.mouseCursors, new(352, 495, 12, 11), 4f);
                                context.OneWayBinds(() => this._viewModel.MoveToPrevFont, () => prevFontButton.Command);
                                prevFontButton.VerticalAlignment = VerticalAlignment.Center;
                                prevFontButton.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                prevFontButton.ClickSound = "smallSelect";
                                fontTypeGrid.Children.Add(prevFontButton);
                                fontTypeGrid.SetColumn(prevFontButton, 0);

                                var fontTypeLabel = new Label();
                                fontTypeLabel.Font = FontType.SpriteText;
                                fontTypeLabel.HorizontalAlignment = HorizontalAlignment.Center;
                                fontTypeLabel.VerticalAlignment = VerticalAlignment.Center;
                                fontTypeLabel.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                context.OneWayBinds(() => this._viewModel.Title, () => fontTypeLabel.Text);
                                fontTypeGrid.Children.Add(fontTypeLabel);
                                fontTypeGrid.SetColumn(fontTypeLabel, 1);

                                var nextFontButton = new TextureButton(
                                    Game1.mouseCursors, new(365, 495, 12, 11), 4f);
                                context.OneWayBinds(() => this._viewModel.MoveToNextFont, () => nextFontButton.Command);
                                nextFontButton.VerticalAlignment = VerticalAlignment.Center;
                                nextFontButton.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                nextFontButton.ClickSound = "smallSelect";
                                fontTypeGrid.Children.Add(nextFontButton);
                                fontTypeGrid.SetColumn(nextFontButton, 2);

                                var helpButton = new TextureButton(
                                    Game1.mouseCursors, new Rectangle(240, 192, 16, 16), 3f);
                                helpButton.VerticalAlignment = VerticalAlignment.Center;
                                helpButton.Margin = new Thickness(0, 0, 0, 0);
                                helpButton.ToolTip = this.GetFontTypeHelpText();
                                fontTypeGrid.Children.Add(helpButton);
                                fontTypeGrid.SetColumn(helpButton, 3);
                            }

                            // general
                            var generalSection = new Label();
                            generalSection.Font = FontType.SpriteText;
                            generalSection.Text = I18n.Ui_MainMenu_Section_General();
                            generalSection.Margin = new Thickness(0, borderWidth / 2, 0, borderWidth / 2);
                            stack.Children.Add(generalSection);

                            var enableOption = new StackContainer();
                            enableOption.Orientation = Orientation.Horizontal;
                            enableOption.HorizontalAlignment = HorizontalAlignment.Left;
                            enableOption.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                            stack.Children.Add(enableOption);
                            {
                                var checkbox = new CheckBox();
                                checkbox.Checked += this.UpdateExampleCurrent;
                                checkbox.Unchecked += this.UpdateExampleCurrent;
                                context.TwoWayBinds(() => this._viewModel.FontEnabled, () => checkbox.IsChecked);

                                var label = new Label();
                                label.Font = FontType.SmallFont;
                                label.Text = I18n.Ui_MainMenu_Enable();
                                label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                enableOption.Children.Add(checkbox);
                                enableOption.Children.Add(label);
                            }

                            var fontComboBox = new ComboBox();
                            fontComboBox.SuggestedWidth = 400;
                            fontComboBox.HorizontalAlignment = HorizontalAlignment.Left;
                            fontComboBox.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                            fontComboBox.ItemAppearance = Appearance.ForData(new FontAppearance());
                            context.OneWayBinds(() => this._viewModel.AllFonts, () => fontComboBox.ItemsSource);
                            context.TwoWayBinds(() => this._viewModel.CurrentFont, () => fontComboBox.SelectedItem);
                            fontComboBox.SelectionChanged += this.UpdateExampleCurrent;
                            stack.Children.Add(fontComboBox);

                            var fontSizeOption = new StackContainer();
                            fontSizeOption.Orientation = Orientation.Horizontal;
                            fontSizeOption.HorizontalAlignment = HorizontalAlignment.Left;
                            fontSizeOption.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                            stack.Children.Add(fontSizeOption);
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
                                label.Text = I18n.Ui_MainMenu_FontSize();
                                label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                fontSizeOption.Children.Add(slider);
                                fontSizeOption.Children.Add(label);
                            }

                            var spacingOption = new StackContainer();
                            spacingOption.Orientation = Orientation.Horizontal;
                            spacingOption.HorizontalAlignment = HorizontalAlignment.Left;
                            spacingOption.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                            stack.Children.Add(spacingOption);
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
                                label.Text = I18n.Ui_MainMenu_Spacing();
                                label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                spacingOption.Children.Add(slider);
                                spacingOption.Children.Add(label);
                            }

                            var lineSpacingOption = new StackContainer();
                            lineSpacingOption.Orientation = Orientation.Horizontal;
                            lineSpacingOption.HorizontalAlignment = HorizontalAlignment.Left;
                            lineSpacingOption.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                            stack.Children.Add(lineSpacingOption);
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
                                label.Text = I18n.Ui_MainMenu_LineSpacing();
                                label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                lineSpacingOption.Children.Add(slider);
                                lineSpacingOption.Children.Add(label);
                            }

                            var xOffsetOption = new StackContainer();
                            xOffsetOption.Orientation = Orientation.Horizontal;
                            xOffsetOption.HorizontalAlignment = HorizontalAlignment.Left;
                            xOffsetOption.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                            stack.Children.Add(xOffsetOption);
                            {
                                var slider = new Slider();
                                slider.Interval = 0.5f;
                                slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                slider.SuggestedWidth = 300;
                                slider.ValueChanged += this.UpdateExampleCurrent;
                                context.TwoWayBinds(() => this._viewModel.CharOffsetX, () => slider.Value);
                                context.OneWayBinds(() => this._viewModel.MinCharOffsetX, () => slider.Minimum);
                                context.OneWayBinds(() => this._viewModel.MaxCharOffsetX, () => slider.Maximum);

                                var label = new Label();
                                label.Font = FontType.SmallFont;
                                label.Text = I18n.Ui_MainMenu_XOffset();
                                label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                xOffsetOption.Children.Add(slider);
                                xOffsetOption.Children.Add(label);
                            }

                            var yOffsetOption = new StackContainer();
                            yOffsetOption.Orientation = Orientation.Horizontal;
                            yOffsetOption.HorizontalAlignment = HorizontalAlignment.Left;
                            yOffsetOption.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                            stack.Children.Add(yOffsetOption);
                            {
                                var slider = new Slider();
                                slider.Interval = 0.5f;
                                slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                slider.SuggestedWidth = 300;
                                slider.ValueChanged += this.UpdateExampleCurrent;
                                context.TwoWayBinds(() => this._viewModel.CharOffsetY, () => slider.Value);
                                context.OneWayBinds(() => this._viewModel.MinCharOffsetY, () => slider.Minimum);
                                context.OneWayBinds(() => this._viewModel.MaxCharOffsetY, () => slider.Maximum);

                                var label = new Label();
                                label.Font = FontType.SmallFont;
                                label.Text = I18n.Ui_MainMenu_YOffset();
                                label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                yOffsetOption.Children.Add(slider);
                                yOffsetOption.Children.Add(label);
                            }

                            var okButton = new TextureButton(
                                Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46));
                            okButton.HorizontalAlignment = HorizontalAlignment.Right;
                            context.OneWayBinds(() => this._viewModel.CanGenerateFont, () => okButton.GreyedOut, new TrueFalseConverter());
                            okButton.Click += this.OkButtonClicked;
                            stack.Children.Add(okButton);

                            // preset
                            var presetSection = new Label();
                            presetSection.Font = FontType.SpriteText;
                            presetSection.Text = I18n.Ui_MainMenu_Section_Preset();
                            presetSection.Margin = new Thickness(0, borderWidth / 2, 0, borderWidth / 2);
                            stack.Children.Add(presetSection);

                            Grid presetGrid = new Grid();
                            presetGrid.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            stack.Children.Add(presetGrid);
                            {
                                var prevPresetButton = new TextureButton(
                                    Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
                                prevPresetButton.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                prevPresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                                prevPresetButton.VerticalAlignment = VerticalAlignment.Center;
                                prevPresetButton.ToolTip = I18n.Ui_MainMenu_PrevPreset();
                                prevPresetButton.ClickSound = "smallSelect";
                                context.OneWayBinds(() => this._viewModel.MoveToPrevPreset, () => prevPresetButton.Command);
                                presetGrid.Children.Add(prevPresetButton);
                                presetGrid.SetColumn(prevPresetButton, 0);

                                var label = new Label();
                                label.Font = FontType.DialogueFont;
                                label.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                label.HorizontalAlignment = HorizontalAlignment.Left;
                                label.VerticalAlignment = VerticalAlignment.Center;
                                context.OneWayBinds(() => this._viewModel.CurrentPresetName, () => label.Text);
                                presetGrid.Children.Add(label);
                                presetGrid.SetColumn(label, 1);

                                var nextPresetButton = new TextureButton(
                                    Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
                                nextPresetButton.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                nextPresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                                nextPresetButton.VerticalAlignment = VerticalAlignment.Center;
                                nextPresetButton.ToolTip = I18n.Ui_MainMenu_NextPreset();
                                nextPresetButton.ClickSound = "smallSelect";
                                context.OneWayBinds(() => this._viewModel.MoveToNextPreset, () => nextPresetButton.Command);
                                presetGrid.Children.Add(nextPresetButton);
                                presetGrid.SetColumn(nextPresetButton, 2);

                                var savePresetButton = new TextureButton(
                                    this._icons, new Rectangle(64, 0, 16, 16), 4f);
                                savePresetButton.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                savePresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                                savePresetButton.VerticalAlignment = VerticalAlignment.Center;
                                savePresetButton.ToolTip = I18n.Ui_MainMenu_SavePreset();
                                savePresetButton.ClickSound = "newRecipe";
                                context.OneWayBinds(() => this._viewModel.CanSaveCurrentPreset, () => savePresetButton.GreyedOut, new TrueFalseConverter());
                                context.OneWayBinds(() => this._viewModel.SaveCurrentPreset, () => savePresetButton.Command);
                                presetGrid.Children.Add(savePresetButton);
                                presetGrid.SetColumn(savePresetButton, 3);

                                var deletePresetButton = new TextureButton(
                                    this._icons, new Rectangle(80, 0, 16, 16), 4f);
                                deletePresetButton.Margin = new Thickness(0, 0, 0, 0);
                                deletePresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                                deletePresetButton.VerticalAlignment = VerticalAlignment.Center;
                                deletePresetButton.ToolTip = I18n.Ui_MainMenu_DelPreset();
                                deletePresetButton.ClickSound = "trashcan";
                                context.OneWayBinds(() => this._viewModel.CanDeleteCurrentPreset, () => deletePresetButton.GreyedOut, new TrueFalseConverter());
                                context.OneWayBinds(() => this._viewModel.DeleteCurrentPreset, () => deletePresetButton.Command);
                                presetGrid.Children.Add(deletePresetButton);
                                presetGrid.SetColumn(deletePresetButton, 4);
                            }

                            Button newPresetButton = new Button();
                            newPresetButton.MinWidth = 200;
                            newPresetButton.MinHeight = 68;
                            newPresetButton.HorizontalAlignment = HorizontalAlignment.Left;
                            newPresetButton.ClickSound = "coin";
                            //context.OneWayBinds(() => this._viewModel.CanSaveCurrentAsNewPreset, () => newPresetButton.GreyedOut, new TrueFalseConverter());
                            context.OneWayBinds(() => this._viewModel.SaveCurrentAsNewPreset, () => newPresetButton.Command);
                            context.OneWayBinds<Func<IOverlayMenu>, object>(() => this.CreateNewPresetMenu, () => newPresetButton.CommandParameter);
                            stack.Children.Add(newPresetButton);
                            {
                                Label label = new Label();
                                label.HorizontalAlignment = HorizontalAlignment.Center;
                                label.VerticalAlignment = VerticalAlignment.Center;
                                label.Text = I18n.Ui_MainMenu_NewPreset();
                                newPresetButton.Content = label;
                            }
                        }
                    }

                    Grid previewGrid = new Grid();
                    previewGrid.Margin = new Thickness(borderWidth / 2);
                    previewGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
                    previewGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    mainGrid.Children.Add(previewGrid);
                    mainGrid.SetColumn(previewGrid, 0);
                    {
                        var realPreviewGrid = new FontPreviewGrid(this.DefaultBorderless(whenStarrySkyInterface: TextureBoxes.Default));
                        realPreviewGrid.Orientation = Orientation.Vertical;
                        context.OneWayBinds(() => this._viewModel.ExamplesMerged, () => realPreviewGrid.IsMerged);
                        var vanillaTextLabel = realPreviewGrid.VanillaFontExample;
                        var currentTextLabel = realPreviewGrid.CurrentFontExample;
                        context.OneWayBinds(() => this._viewModel.ShowExampleBounds, () => vanillaTextLabel.ShowBounds);
                        context.OneWayBinds(() => this._viewModel.ShowExampleBounds, () => currentTextLabel.ShowBounds);
                        context.OneWayBinds(() => this._viewModel.ShowExampleText, () => vanillaTextLabel.ShowText);
                        context.OneWayBinds(() => this._viewModel.ShowExampleText, () => currentTextLabel.ShowText);
                        context.OneWayBinds(() => this._viewModel.ExampleText, () => vanillaTextLabel.Text);
                        context.OneWayBinds(() => this._viewModel.ExampleText, () => currentTextLabel.Text);
                        context.OneWayBinds(() => this._viewModel.ExampleVanillaFont, () => vanillaTextLabel.Font);
                        context.OneWayBinds(() => this._viewModel.ExampleCurrentFont, () => currentTextLabel.Font);
                        previewGrid.Children.Add(realPreviewGrid);
                        previewGrid.SetRow(realPreviewGrid, 0);

                        var optionsStack = new StackContainer();
                        optionsStack.Orientation = Orientation.Horizontal;
                        previewGrid.Children.Add(optionsStack);
                        previewGrid.SetRow(optionsStack, 1);
                        {
                            var mergeButton = new ToggleTextureButton(
                                onTexture: this._icons, onSourceRectangle: new(32, 0, 16, 16), onScale: 4f,
                                offTexture: this._icons, offSourceRectangle: new(48, 0, 16, 16), offScale: 4f);
                            mergeButton.ClickSound = "coin";
                            context.TwoWayBinds(() => this._viewModel.ExamplesMerged, () => mergeButton.IsToggled);
                            optionsStack.Children.Add(mergeButton);

                            //var showBoundsButton = new ToggleTextureButton(
                            //    onTexture: this._icons, onSourceRectangle: new(64, 0, 16, 16), onScale: 4f,
                            //    offTexture: this._icons, offSourceRectangle: new(80, 0, 16, 16), offScale: 4f);
                            //showBoundsButton.ClickSound = "coin";
                            //context.TwoWayBinds(() => this._viewModel.ShowExampleBounds, () => showBoundsButton.IsToggled);
                            //optionsStack.Children.Add(showBoundsButton);

                            //var showTextButton = new ToggleTextureButton(
                            //    onTexture: this._icons, onSourceRectangle: new(32, 0, 16, 16), onScale: 4f,
                            //    offTexture: this._icons, offSourceRectangle: new(48, 0, 16, 16), offScale: 4f);
                            //showTextButton.ClickSound = "coin";
                            //context.TwoWayBinds(() => this._viewModel.ShowExampleText, () => showTextButton.IsToggled);
                            //optionsStack.Children.Add(showTextButton);
                        }
                    }
                }
            }

            context.SetRootElement(rootGrid);
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
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
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

        private string GetFontTypeHelpText()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(I18n.Ui_MainMenu_FontTypeHelp_Paragraph());

            var lang = FontHelpers.GetCurrentLanguage();
            if (FontHelpers.IsLatinLanguage(lang))
            {
                sb.AppendLine();
                sb.AppendLine()
                    .Append(I18n.Ui_MainMenu_FontTypeHelp_LatinLang_Paragraph(lang.Locale));
            }

            return sb.ToString();
        }

        /// <summary>问题：如果同时安装了<see href="https://www.nexusmods.com/stardewvalley/mods/2668">魔法少女界面</see>，设置字体的界面会出现黑框问题，此函数为解决方法。</summary>
        /// <remarks>
        /// 问题根源在两个模组使用了不同的贴图。魔法少女最近一次更新在18年8月23号，当时即使最新的游戏版本也是1.3.28。当时Maps/MenuTiles中（64，320，64，64）的图片和现在不同，当时是深青色背景一个淡青色的鬼；现在则是一个背景框。扔进游戏里自然就出错了。
        /// </remarks>
        private TextureBox DefaultBorderless(TextureBox whenStarrySkyInterface)
        {
            TextureBox result = TextureBoxes.DefaultBorderless;

            const string ID = "BeneathThePlass.StarrySkyInterfaceCP";
            if (this._registry.IsLoaded(ID))
            {
                result = whenStarrySkyInterface;
            }

            return result;
        }

        private class FontAppearance : DataAppearanceBuilder<FontModel>
        {
            protected override Element Build(AppearanceBuildContext<FontModel> context)
            {
                FontModel? font = context.Target;

                Label l = new Label();
                l.Text = this.GetText(font);
                l.Font = FontType.SmallFont;
                l.Margin = new Thickness(4, 0, 0, 0);
                l.HorizontalAlignment = HorizontalAlignment.Left;
                l.VerticalAlignment = VerticalAlignment.Center;
                return l;
            }

            private string GetText(FontModel? font)
            {
                if (font == null)
                    return string.Empty;

                if (font.FullPath == null)
                    return I18n.Ui_MainMenu_Font_KeepOrig();
                else
                    return $"{font.FamilyName} ({font.SubfamilyName})";
            }
        }
    }
}
