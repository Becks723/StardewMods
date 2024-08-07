﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Menus.ViewModels;
using FontSettings.Framework.Menus.Views.Components;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValleyUI;
using StardewValleyUI.Controls;
using StardewValleyUI.Controls.Primitives;
using StardewValleyUI.Data.Converters;
using StardewValleyUI.Menus;

namespace FontSettings.Framework.Menus.Views
{
    internal class FontSettingsMenu : BaseMenu<FontSettingsMenuModel>
    {
        private readonly IFontPresetManager _presetManager;
        private readonly IModRegistry _registry;
        private readonly bool _enableLatinDialogueFont;
        private readonly SearchManager _searchManager;
        private readonly FontSettingsMenuModel _viewModel;

        private readonly Texture2D _save = Textures.Save;
        private readonly Texture2D _delete = Textures.Delete;
        private readonly Texture2D _sectionBox = Textures.SectionBox;
        private readonly Texture2D _previewNormal = Textures.FontPreviewNormal;
        private readonly Texture2D _previewCompare = Textures.FontPreviewCompare;
        private readonly Texture2D _refresh = Textures.Refresh;
        private readonly Texture2D _spanner = Textures.Spanner;

        private IClickableMenu _currentSubMenu;

        public FontSettingsMenu(IFontPresetManager presetManager, IModRegistry registry, bool enableLatinDialogueFont, SearchManager searchManager, FontSettingsMenuModel viewModel)
        {
            this._presetManager = presetManager;
            this._registry = registry;
            this._enableLatinDialogueFont = enableLatinDialogueFont;
            this._searchManager = searchManager;

            this.ResetComponents();

            this.DataContext = this._viewModel = viewModel;
            this._viewModel.PropertyChanged += this.OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FontSettingsMenuModel.CurrentFontType))
                this.TogglePixelZoomOption(this._viewModel.CurrentFontType == GameFontType.SpriteText);
        }

        private Container _container;
        private Element _pixelZoomOption;
        private int _index;
        private void TogglePixelZoomOption(bool on)
        {
            var container = this._container;
            var option = this._pixelZoomOption;
            if (container == null) throw new InvalidOperationException();
            if (option == null) throw new InvalidOperationException();

            if (on)
            {
                if (!container.Children.Contains(option))
                    container.Children.Insert(this._index, option);
            }
            else
                container.Children.Remove(option);
        }

        protected override void OnDataContextChanged(ValueChangedEventArgs e)
        {
            base.OnDataContextChanged(e);

            this.TogglePixelZoomOption(this._viewModel.CurrentFontType == GameFontType.SpriteText);
        }

        private async void UpdateExampleCurrent(object sender, EventArgs e)
        {
            try
            {
                await this._viewModel.UpdateExampleCurrentAsync();
            }
            catch (Exception ex)
            {
                this.LogException("Update sample", ex);
            }
        }

        private void OnCoreSettingsChanged(object sender, EventArgs e)
        {
            this._viewModel.OnSettingsChanged();
        }

        private async void OkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                await this.OkButtonClickedAsync();
            }
            catch (Exception ex)
            {
                this.LogException("Generate font", ex);
            }
        }

        private async Task OkButtonClickedAsync()
        {
            var (result, fontType) = await this._viewModel.ChangeFontAsync();
            if (result.IsSuccess)
            {
                Game1.playSound("money");

                string message = I18n.HudMessage_SuccessSetFont(fontType.LocalizedName());
                if (Game1.gameMode == 0)  // 如果在标题页面，HUD无法显示，
                    ILog.Info(message);   // 写在日志中
                else
                    Game1.addHUDMessage(new OverlayHUDMessage(message));
            }
            else
            {
                Game1.playSound("cancel");

                string message = I18n.HudMessage_FailedSetFont(fontType.LocalizedName());
                if (Game1.gameMode == 0)  // 如果在标题页面，HUD无法显示，
                    ILog.Error(message);  // 写在日志中
                else
                    Game1.addHUDMessage(new OverlayHUDMessage(message, HUDMessage.error_type));

                // 日志记录错误信息。
                string error = result.GetError();
                ILog.Error(error);
            }
        }

        private void LogException(string name, Exception exception)
        {
            ILog.Error($"Error when {name}: {exception.Message}\n{exception.StackTrace}");
        }

        private void OnPresetTitleMousePressed(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButtons.ContainsKey(MouseButtons.LeftButton))
            {
                if (this._viewModel.TryGetCurrentPresetIfSupportsDetailedInfo(out var preset))
                {
                    Game1.playSound("bigDeSelect");

                    var presetInfoMenu = new PresetInfoMenu(
                        preset: preset,
                        onClosed: _ => this.ChangeSubMenu(null));

                    this.ChangeSubMenu(presetInfoMenu);
                }
            }
        }

        private void OnExportButtonClicked(object sender, EventArgs e)
        {
            var exportMenu = new ExportMenu(
                onClosed: () => this.ChangeSubMenu(null),
                viewModel: this._viewModel.GetCurrentExportViewModel());

            this.ChangeSubMenu(exportMenu);
        }

        protected override void ResetComponents(MenuInitializationContext context)
        {
            Grid rootGrid = new Grid();
            rootGrid.SuggestedWidth = Math.Max(Game1.uiViewport.Width * 0.67f, 1000);
            rootGrid.SuggestedHeight = Math.Max(Game1.uiViewport.Height * 0.67f, 600);
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
            {
                var titleBorder = new TextureBoxBorder();
                titleBorder.Box = TextureBoxes.Default;
                titleBorder.DrawShadow = false;
                titleBorder.Padding += new Thickness(borderWidth / 3, 0);
                titleBorder.HorizontalAlignment = HorizontalAlignment.Center;
                titleBorder.Margin = new Thickness(0, 0, 0, borderWidth / 2);
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
                    Grid settingsGrid = new Grid();
                    settingsGrid.Margin = new Thickness(borderWidth / 2, 0, 0, 0);
                    settingsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
                    settingsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    mainGrid.Children.Add(settingsGrid);
                    mainGrid.SetColumn(settingsGrid, 1);
                    {
                        var scrollViewer = new ScrollViewer();
                        scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                        settingsGrid.Children.Add(scrollViewer);
                        settingsGrid.SetRow(scrollViewer, 0);
                        {
                            float frameSpacing = borderWidth / 2;
                            float optionSpacing = borderWidth / 2;

                            var stack = new StackContainer();
                            stack.Orientation = Orientation.Vertical;
                            stack.Margin = new Thickness(frameSpacing);
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
                                    context.OneWayBinds(() => this._viewModel.MoveToPrevFontCommand, () => prevFontButton.Command);
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
                                    context.OneWayBinds(() => this._viewModel.CurrentFontType, () => fontTypeLabel.Text, new ToStringConverter<GameFontType>(type => type.LocalizedName()));
                                    fontTypeGrid.Children.Add(fontTypeLabel);
                                    fontTypeGrid.SetColumn(fontTypeLabel, 1);

                                    var nextFontButton = new TextureButton(
                                        Game1.mouseCursors, new(365, 495, 12, 11), 4f);
                                    context.OneWayBinds(() => this._viewModel.MoveToNextFontCommand, () => nextFontButton.Command);
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

                                float sectionPadding = borderWidth / 3;
                                Border SectionBorder()
                                {
                                    var border = new TextureBoxBorder();
                                    border.Box = TextureBox.From(this._sectionBox, new Rectangle(0, 0, 3, 3), 1f, Thickness.One);
                                    border.Padding += new Thickness(sectionPadding);
                                    return border;
                                }

                                Label SliderValueLabel(Slider slider, Func<float, string>? format = null)
                                {
                                    Label valuelabel = new Label();
                                    valuelabel.Font = FontType.DialogueFont;
                                    valuelabel.MinWidth = FontHelper.MeasureString(FontType.DialogueFont, "999").X;
                                    var converter = format == null
                                        ? new ToStringConverter<float>()
                                        : new ToStringConverter<float>(format);
                                    context.OneWayBinds(() => slider.Value, () => valuelabel.Text, converter);
                                    return valuelabel;
                                }

                                // general
                                var generalBorder = SectionBorder();
                                generalBorder.Margin = new Thickness(0, frameSpacing, 0, 0);
                                stack.Children.Add(generalBorder);
                                {
                                    var gStack = new StackContainer();
                                    gStack.Orientation = Orientation.Vertical;
                                    generalBorder.Child = gStack;
                                    {
                                        var generalSection = new Label();
                                        generalSection.Font = FontType.SpriteText;
                                        generalSection.Text = I18n.Ui_MainMenu_Section_General();
                                        generalSection.HorizontalAlignment = HorizontalAlignment.Left;
                                        gStack.Children.Add(generalSection);

                                        var enableOption = new StackContainer();
                                        enableOption.Orientation = Orientation.Horizontal;
                                        enableOption.HorizontalAlignment = HorizontalAlignment.Left;
                                        enableOption.Margin = new Thickness(0, optionSpacing, 0, 0);
                                        gStack.Children.Add(enableOption);
                                        {
                                            var checkbox = new CheckBox();
                                            checkbox.Checked += this.UpdateExampleCurrent;
                                            checkbox.Checked += this.OnCoreSettingsChanged;
                                            checkbox.Unchecked += this.UpdateExampleCurrent;
                                            checkbox.Unchecked += this.OnCoreSettingsChanged;
                                            context.TwoWayBinds(() => this._viewModel.FontEnabled, () => checkbox.IsChecked);

                                            var label = new Label();
                                            label.Font = FontType.SmallFont;
                                            label.Text = I18n.Ui_MainMenu_Enable();
                                            label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            enableOption.Children.Add(checkbox);
                                            enableOption.Children.Add(label);
                                        }

                                        var fontOption = new StackContainer();
                                        fontOption.Orientation = Orientation.Horizontal;
                                        fontOption.HorizontalAlignment = HorizontalAlignment.Left;
                                        fontOption.Margin = new Thickness(0, optionSpacing, 0, 0);
                                        gStack.Children.Add(fontOption);
                                        {
                                            var fontComboBox = new ComboBox();
                                            fontComboBox.SuggestedWidth = 400;
                                            fontComboBox.ItemAppearance = Appearance.ForData(new FontAppearance());
                                            fontComboBox.ItemComparer = new FontViewModelComparer();
                                            context.OneWayBinds(() => this._viewModel.AllFonts, () => fontComboBox.ItemsSource);
                                            context.TwoWayBinds(() => this._viewModel.CurrentFont, () => fontComboBox.SelectedItem);
                                            fontComboBox.SelectionChanged += this.UpdateExampleCurrent;
                                            fontComboBox.SelectionChanged += this.OnCoreSettingsChanged;

                                            //var refreshButton = new RefreshButton(2.5f);
                                            //refreshButton.AnimationDuration = 300;
                                            var refreshButton = new TextureButton(this._refresh, null, 2.5f);
                                            refreshButton.ClickSound = "trashcan";
                                            refreshButton.Margin = new Thickness(optionSpacing, 0, 0, 0);
                                            refreshButton.VerticalAlignment = VerticalAlignment.Center;
                                            refreshButton.ToolTip = I18n.Ui_MainMenu_RefreshFonts();
                                            context.OneWayBinds(() => this._viewModel.RefreshFontsCommand, () => refreshButton.Command);
                                            context.OneWayBinds(() => this._viewModel.IsRefreshingFonts, () => refreshButton.GreyedOut);

                                            var manageButton = new TextureButton(this._spanner, null, 4f);
                                            manageButton.Margin = new Thickness(optionSpacing, 0, 0, 0);
                                            manageButton.VerticalAlignment = VerticalAlignment.Center;
                                            manageButton.ClickSound = "bigDeSelect";
                                            manageButton.ToolTip = I18n.Ui_MainMenu_ManageFonts();
                                            manageButton.CommandParameter = (Func<IOverlayMenu>)this.CreateFontManageMenu;
                                            context.OneWayBinds(() => this._viewModel.ManageFontsCommand, () => manageButton.Command);

                                            fontOption.Children.Add(fontComboBox);
                                            fontOption.Children.Add(refreshButton);
                                            fontOption.Children.Add(manageButton);
                                        }

                                        var fontSizeOption = new StackContainer();
                                        fontSizeOption.Orientation = Orientation.Horizontal;
                                        fontSizeOption.HorizontalAlignment = HorizontalAlignment.Left;
                                        fontSizeOption.Margin = new Thickness(0, optionSpacing, 0, 0);
                                        gStack.Children.Add(fontSizeOption);
                                        {
                                            var slider = new Slider();
                                            slider.Interval = 1;
                                            slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                            slider.SuggestedWidth = 300;
                                            slider.ValueChanged += this.UpdateExampleCurrent;
                                            slider.ValueChanged += this.OnCoreSettingsChanged;
                                            context.TwoWayBinds(() => this._viewModel.FontSize, () => slider.Value);
                                            context.OneWayBinds(() => this._viewModel.MinFontSize, () => slider.Minimum);
                                            context.OneWayBinds(() => this._viewModel.MaxFontSize, () => slider.Maximum);

                                            Label valuelabel = SliderValueLabel(slider);
                                            valuelabel.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            var label = new Label();
                                            label.Font = FontType.SmallFont;
                                            label.Text = I18n.Ui_MainMenu_FontSize();
                                            label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            fontSizeOption.Children.Add(slider);
                                            fontSizeOption.Children.Add(valuelabel);
                                            fontSizeOption.Children.Add(label);
                                        }
                                    }
                                }

                                // advanced
                                var advancedBorder = SectionBorder();
                                advancedBorder.Margin = new Thickness(0, frameSpacing, 0, 0);
                                stack.Children.Add(advancedBorder);
                                {
                                    var aStack = new StackContainer();
                                    aStack.Orientation = Orientation.Vertical;
                                    advancedBorder.Child = aStack;
                                    {
                                        var advancedSection = new Label();
                                        advancedSection.Font = FontType.SpriteText;
                                        advancedSection.Text = I18n.Ui_MainMenu_Section_Advanced();
                                        advancedSection.HorizontalAlignment = HorizontalAlignment.Left;
                                        aStack.Children.Add(advancedSection);

                                        var spacingOption = new StackContainer();
                                        spacingOption.Orientation = Orientation.Horizontal;
                                        spacingOption.HorizontalAlignment = HorizontalAlignment.Left;
                                        spacingOption.Margin = new Thickness(0, optionSpacing, 0, 0);
                                        aStack.Children.Add(spacingOption);
                                        {
                                            var slider = new Slider();
                                            slider.Interval = 1;
                                            slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                            slider.SuggestedWidth = 300;
                                            slider.ValueChanged += this.UpdateExampleCurrent;
                                            slider.ValueChanged += this.OnCoreSettingsChanged;
                                            context.TwoWayBinds(() => this._viewModel.Spacing, () => slider.Value);
                                            context.OneWayBinds(() => this._viewModel.MinSpacing, () => slider.Minimum);
                                            context.OneWayBinds(() => this._viewModel.MaxSpacing, () => slider.Maximum);

                                            Label valuelabel = SliderValueLabel(slider);
                                            valuelabel.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            var label = new Label();
                                            label.Font = FontType.SmallFont;
                                            label.Text = I18n.Ui_MainMenu_Spacing();
                                            label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            spacingOption.Children.Add(slider);
                                            spacingOption.Children.Add(valuelabel);
                                            spacingOption.Children.Add(label);
                                        }

                                        var lineSpacingOption = new StackContainer();
                                        lineSpacingOption.Orientation = Orientation.Horizontal;
                                        lineSpacingOption.HorizontalAlignment = HorizontalAlignment.Left;
                                        lineSpacingOption.Margin = new Thickness(0, optionSpacing, 0, 0);
                                        aStack.Children.Add(lineSpacingOption);
                                        {
                                            var slider = new Slider();
                                            slider.Interval = 1;
                                            slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                            slider.SuggestedWidth = 300;
                                            slider.ValueChanged += this.UpdateExampleCurrent;
                                            slider.ValueChanged += this.OnCoreSettingsChanged;
                                            context.TwoWayBinds(() => this._viewModel.LineSpacing, () => slider.Value);
                                            context.OneWayBinds(() => this._viewModel.MinLineSpacing, () => slider.Minimum);
                                            context.OneWayBinds(() => this._viewModel.MaxLineSpacing, () => slider.Maximum);

                                            Label valuelabel = SliderValueLabel(slider);
                                            valuelabel.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            var label = new Label();
                                            label.Font = FontType.SmallFont;
                                            label.Text = I18n.Ui_MainMenu_LineSpacing();
                                            label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            lineSpacingOption.Children.Add(slider);
                                            lineSpacingOption.Children.Add(valuelabel);
                                            lineSpacingOption.Children.Add(label);
                                        }

                                        var xOffsetOption = new StackContainer();
                                        xOffsetOption.Orientation = Orientation.Horizontal;
                                        xOffsetOption.HorizontalAlignment = HorizontalAlignment.Left;
                                        xOffsetOption.Margin = new Thickness(0, optionSpacing, 0, 0);
                                        aStack.Children.Add(xOffsetOption);
                                        {
                                            var slider = new Slider();
                                            slider.Interval = 0.5f;
                                            slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                            slider.SuggestedWidth = 300;
                                            slider.ValueChanged += this.UpdateExampleCurrent;
                                            slider.ValueChanged += this.OnCoreSettingsChanged;
                                            context.TwoWayBinds(() => this._viewModel.CharOffsetX, () => slider.Value);
                                            context.OneWayBinds(() => this._viewModel.MinCharOffsetX, () => slider.Minimum);
                                            context.OneWayBinds(() => this._viewModel.MaxCharOffsetX, () => slider.Maximum);

                                            Label valuelabel = SliderValueLabel(slider);
                                            valuelabel.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            var label = new Label();
                                            label.Font = FontType.SmallFont;
                                            label.Text = I18n.Ui_MainMenu_XOffset();
                                            label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            xOffsetOption.Children.Add(slider);
                                            xOffsetOption.Children.Add(valuelabel);
                                            xOffsetOption.Children.Add(label);
                                        }

                                        var yOffsetOption = new StackContainer();
                                        yOffsetOption.Orientation = Orientation.Horizontal;
                                        yOffsetOption.HorizontalAlignment = HorizontalAlignment.Left;
                                        yOffsetOption.Margin = new Thickness(0, optionSpacing, 0, 0);
                                        aStack.Children.Add(yOffsetOption);
                                        {
                                            var slider = new Slider();
                                            slider.Interval = 0.5f;
                                            slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                            slider.SuggestedWidth = 300;
                                            slider.ValueChanged += this.UpdateExampleCurrent;
                                            slider.ValueChanged += this.OnCoreSettingsChanged;
                                            context.TwoWayBinds(() => this._viewModel.CharOffsetY, () => slider.Value);
                                            context.OneWayBinds(() => this._viewModel.MinCharOffsetY, () => slider.Minimum);
                                            context.OneWayBinds(() => this._viewModel.MaxCharOffsetY, () => slider.Maximum);

                                            Label valuelabel = SliderValueLabel(slider);
                                            valuelabel.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            var label = new Label();
                                            label.Font = FontType.SmallFont;
                                            label.Text = I18n.Ui_MainMenu_YOffset();
                                            label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            yOffsetOption.Children.Add(slider);
                                            yOffsetOption.Children.Add(valuelabel);
                                            yOffsetOption.Children.Add(label);
                                        }

                                        var pixelZoomOption = new StackContainer();
                                        pixelZoomOption.Orientation = Orientation.Horizontal;
                                        pixelZoomOption.HorizontalAlignment = HorizontalAlignment.Left;
                                        pixelZoomOption.Margin = new Thickness(0, optionSpacing, 0, 0);
                                        {
                                            var slider = new Slider();
                                            slider.Interval = 0.05f;
                                            slider.RaiseEventOccasion = RaiseOccasion.WhenValueChanged;
                                            slider.SuggestedWidth = 300;
                                            slider.ValueChanged += this.UpdateExampleCurrent;
                                            slider.ValueChanged += this.OnCoreSettingsChanged;
                                            context.TwoWayBinds(() => this._viewModel.PixelZoom, () => slider.Value);
                                            context.OneWayBinds(() => this._viewModel.MinPixelZoom, () => slider.Minimum);
                                            context.OneWayBinds(() => this._viewModel.MaxPixelZoom, () => slider.Maximum);

                                            Label valuelabel = SliderValueLabel(slider, format: val => $"{val}x");
                                            valuelabel.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            var label = new Label();
                                            label.Font = FontType.SmallFont;
                                            label.Text = I18n.Ui_MainMenu_PixelZoom();
                                            label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                            pixelZoomOption.Children.Add(slider);
                                            pixelZoomOption.Children.Add(valuelabel);
                                            pixelZoomOption.Children.Add(label);
                                        }

                                        this._container = aStack;
                                        this._pixelZoomOption = pixelZoomOption;
                                        this._index = aStack.Children.Count;
                                    }
                                }

                                // unsorted
                                // reset
                                var resetOption = new Grid();
                                resetOption.Margin = new Thickness(0, frameSpacing, 0, 0);
                                stack.Children.Add(resetOption);
                                {
                                    var label = new Label();
                                    label.Font = FontType.SmallFont;
                                    label.Text = I18n.Ui_MainMenu_ResetFont();
                                    label.HorizontalAlignment = HorizontalAlignment.Left;
                                    resetOption.Children.Add(label);

                                    var button = new TextureButton(Game1.mouseCursors, new Rectangle(294, 428, 21, 11), 4f);
                                    button.HorizontalAlignment = HorizontalAlignment.Right;
                                    button.ClickSound = "bigDeSelect";
                                    context.OneWayBinds(() => this._viewModel.ResetFontCommand, () => button.Command);
                                    resetOption.Children.Add(button);
                                }

                                // preset
                                var presetBorder = SectionBorder();
                                presetBorder.Margin = new Thickness(0, frameSpacing, 0, 0);
                                stack.Children.Add(presetBorder);
                                {
                                    var pStack = new StackContainer();
                                    pStack.Orientation = Orientation.Vertical;
                                    presetBorder.Child = pStack;
                                    {
                                        var presetSection = new Label();
                                        presetSection.Font = FontType.SpriteText;
                                        presetSection.Text = I18n.Ui_MainMenu_Section_Preset();
                                        presetSection.HorizontalAlignment = HorizontalAlignment.Left;
                                        pStack.Children.Add(presetSection);

                                        Grid presetGrid = new Grid();
                                        presetGrid.Margin = new Thickness(0, optionSpacing, 0, 0);
                                        presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                        presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                                        presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                        presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                        presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                        pStack.Children.Add(presetGrid);
                                        {
                                            var prevPresetButton = new TextureButton(
                                                Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
                                            prevPresetButton.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                            prevPresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                                            prevPresetButton.VerticalAlignment = VerticalAlignment.Center;
                                            prevPresetButton.ToolTip = I18n.Ui_MainMenu_PrevPreset();
                                            prevPresetButton.ClickSound = "smallSelect";
                                            context.OneWayBinds(() => this._viewModel.MoveToPrevPresetCommand, () => prevPresetButton.Command);
                                            presetGrid.Children.Add(prevPresetButton);
                                            presetGrid.SetColumn(prevPresetButton, 0);

                                            Grid titleGrid = new Grid();
                                            titleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnit.Percent) });
                                            titleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                                            titleGrid.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                            titleGrid.MousePressed += this.OnPresetTitleMousePressed;
                                            presetGrid.Children.Add(titleGrid);
                                            presetGrid.SetColumn(titleGrid, 1);
                                            {
                                                var titleView = new ScrollViewer();
                                                titleView.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                                titleView.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                                titleView.ShowsBackground = false;
                                                titleGrid.Children.Add(titleView);
                                                titleGrid.SetRow(titleView, 0);
                                                {
                                                    var titleLabel = new Label();
                                                    titleLabel.Font = FontType.DialogueFont;
                                                    context.OneWayBinds(() => this._viewModel.CurrentPresetTitle, () => titleLabel.Text);
                                                    titleView.Content = titleLabel;
                                                }

                                                var subtitleView = new ScrollViewer();
                                                subtitleView.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                                subtitleView.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                                subtitleView.ShowsBackground = false;
                                                titleGrid.Children.Add(subtitleView);
                                                titleGrid.SetRow(subtitleView, 1);
                                                {
                                                    var subtitleLabel = new Label();
                                                    subtitleLabel.Font = FontType.SmallFont;
                                                    context.OneWayBinds(() => this._viewModel.CurrentPresetSubtitle, () => subtitleLabel.Text);
                                                    subtitleView.Content = subtitleLabel;
                                                }
                                            }

                                            var nextPresetButton = new TextureButton(
                                                Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
                                            nextPresetButton.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                            nextPresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                                            nextPresetButton.VerticalAlignment = VerticalAlignment.Center;
                                            nextPresetButton.ToolTip = I18n.Ui_MainMenu_NextPreset();
                                            nextPresetButton.ClickSound = "smallSelect";
                                            context.OneWayBinds(() => this._viewModel.MoveToNextPresetCommand, () => nextPresetButton.Command);
                                            presetGrid.Children.Add(nextPresetButton);
                                            presetGrid.SetColumn(nextPresetButton, 2);

                                            var savePresetButton = new TextureButton(
                                                this._save, null, 4f);
                                            savePresetButton.Margin = new Thickness(0, 0, borderWidth / 3, 0);
                                            savePresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                                            savePresetButton.VerticalAlignment = VerticalAlignment.Center;
                                            savePresetButton.ToolTip = I18n.Ui_MainMenu_SavePreset();
                                            savePresetButton.ClickSound = "newRecipe";
                                            context.OneWayBinds(() => this._viewModel.CanSaveCurrentPreset, () => savePresetButton.GreyedOut, new TrueFalseConverter());
                                            context.OneWayBinds(() => this._viewModel.SaveCurrentPresetCommand, () => savePresetButton.Command);
                                            presetGrid.Children.Add(savePresetButton);
                                            presetGrid.SetColumn(savePresetButton, 3);

                                            var deletePresetButton = new TextureButton(
                                                this._delete, null, 4f);
                                            deletePresetButton.Margin = new Thickness(0, 0, 0, 0);
                                            deletePresetButton.HorizontalAlignment = HorizontalAlignment.Center;
                                            deletePresetButton.VerticalAlignment = VerticalAlignment.Center;
                                            deletePresetButton.ToolTip = I18n.Ui_MainMenu_DelPreset();
                                            deletePresetButton.ClickSound = "trashcan";
                                            context.OneWayBinds(() => this._viewModel.CanDeleteCurrentPreset, () => deletePresetButton.GreyedOut, new TrueFalseConverter());
                                            context.OneWayBinds(() => this._viewModel.DeleteCurrentPresetCommand, () => deletePresetButton.Command);
                                            presetGrid.Children.Add(deletePresetButton);
                                            presetGrid.SetColumn(deletePresetButton, 4);
                                        }

                                        Button newPresetButton = new Button();
                                        newPresetButton.MinWidth = 200;
                                        newPresetButton.MinHeight = 68;
                                        newPresetButton.HorizontalAlignment = HorizontalAlignment.Left;
                                        newPresetButton.Margin = new Thickness(0, optionSpacing, 0, 0);
                                        newPresetButton.ClickSound = "coin";
                                        newPresetButton.CommandParameter = (Func<IOverlayMenu>)this.CreateNewPresetMenu;
                                        //context.OneWayBinds(() => this._viewModel.CanSaveCurrentAsNewPreset, () => newPresetButton.GreyedOut, new TrueFalseConverter());
                                        context.OneWayBinds(() => this._viewModel.SaveCurrentAsNewPresetCommand, () => newPresetButton.Command);
                                        pStack.Children.Add(newPresetButton);
                                        {
                                            Label label = new Label();
                                            label.HorizontalAlignment = HorizontalAlignment.Center;
                                            label.VerticalAlignment = VerticalAlignment.Center;
                                            label.Text = I18n.Ui_MainMenu_NewPreset();
                                            newPresetButton.Content = label;
                                        }
                                    }
                                }
                            }
                        }

                        StackContainer functionStack = new StackContainer();
                        functionStack.Orientation = Orientation.Horizontal;
                        functionStack.HorizontalAlignment = HorizontalAlignment.Center;
                        settingsGrid.Children.Add(functionStack);
                        settingsGrid.SetRow(functionStack, 1);
                        {
                            var okButton = new TextureButton(
                                Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46));
                            context.OneWayBinds(() => this._viewModel.CanGenerateFont, () => okButton.GreyedOut, new TrueFalseConverter());
                            okButton.ClickSound = "coin";
                            okButton.ToolTip = I18n.Ui_MainMenu_Ok();
                            okButton.Click += this.OkButtonClicked;
                            functionStack.Children.Add(okButton);

                            var exportButton = new TextureButton(Textures.Export, null, 4f);
                            exportButton.ClickSound = "bigDeSelect";
                            exportButton.ToolTip = I18n.Ui_MainMenu_Export();
                            exportButton.Click += this.OnExportButtonClicked;
                            functionStack.Children.Add(exportButton);
                        }
                    }

                    Grid previewGrid = new Grid();
                    previewGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
                    previewGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    mainGrid.Children.Add(previewGrid);
                    mainGrid.SetColumn(previewGrid, 0);
                    {
                        var previewControl = new FontPreviewGrid(TextureBoxes.ThickBorder, 8);
                        context.OneWayBinds(() => this._viewModel.PreviewMode, () => previewControl.Mode);
                        var vanillaTextLabel = previewControl.VanillaFontExample;
                        var currentTextLabel = previewControl.CurrentFontExample;
                        context.OneWayBinds(() => this._viewModel.ShowExampleBounds, () => vanillaTextLabel.ShowBounds);
                        context.OneWayBinds(() => this._viewModel.ShowExampleBounds, () => currentTextLabel.ShowBounds);
                        context.OneWayBinds(() => this._viewModel.ShowExampleText, () => vanillaTextLabel.ShowText);
                        context.OneWayBinds(() => this._viewModel.ShowExampleText, () => currentTextLabel.ShowText);
                        context.OneWayBinds(() => this._viewModel.ExampleText, () => vanillaTextLabel.Text);
                        context.OneWayBinds(() => this._viewModel.ExampleText, () => currentTextLabel.Text);
                        context.OneWayBinds(() => this._viewModel.ExampleVanillaFont, () => vanillaTextLabel.Font);
                        context.OneWayBinds(() => this._viewModel.ExampleCurrentFont, () => currentTextLabel.Font);
                        previewGrid.Children.Add(previewControl);
                        previewGrid.SetRow(previewControl, 0);

                        var optionsStack = new StackContainer();
                        optionsStack.Orientation = Orientation.Horizontal;
                        optionsStack.HorizontalAlignment = HorizontalAlignment.Center;
                        previewGrid.Children.Add(optionsStack);
                        previewGrid.SetRow(optionsStack, 1);
                        {
                            var previewButton = new SwitchTextureButton<PreviewMode>(
                                (mode, button) =>
                                {
                                    switch (mode)
                                    {
                                        case PreviewMode.Normal:
                                            button.Texture = this._previewNormal;
                                            button.SourceRectangle = null;
                                            button.Scale = 4f;
                                            break;

                                        case PreviewMode.Compare:
                                        case PreviewMode.PreciseCompare:
                                            button.Texture = this._previewCompare;
                                            button.SourceRectangle = null;
                                            button.Scale = 4f;
                                            break;
                                    }
                                });
                            previewButton.ClickSound = "coin";
                            context.TwoWayBinds(() => this._viewModel.PreviewMode, () => previewButton.Flag);
                            optionsStack.Children.Add(previewButton);

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

            if (this._currentSubMenu is IDisposable disposable)
                disposable.Dispose();
        }

        public override void update(GameTime time)
        {
            // 在有子菜单的情况下，主菜单停止刷新。
            if (this._currentSubMenu != null)
                this._currentSubMenu.update(time);
            else
                base.update(time);
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            base.draw(b);

#if DEBUG
            string FormatColor(Color color) => $"R={color.R} G={color.G} B={color.B} A={color.A}";
            string GetDebugInfo()
            {
                return $"CurrentFontType: {this._viewModel.CurrentFontType}"
                    + $"\nCurrentLanguage: {FontHelpers.GetCurrentLanguage()}"
                    + $"\nEnabled: {this._viewModel.FontEnabled}"
                    + $"\nSize: {this._viewModel.FontSize}"
                    + $"\nSpacing: {this._viewModel.Spacing}"
                    + $"\nLine spacing: {this._viewModel.LineSpacing}"
                    + $"\nFont: {this._viewModel.FontFilePath}"
                    + $"\nFont index: {this._viewModel.FontIndex}"
                    + $"\nOffset-x: {this._viewModel.CharOffsetX}"
                    + $"\nOffset-y: {this._viewModel.CharOffsetY}"
                    + $"\nPixel Zoom: {this._viewModel.PixelZoom}"
                    + $"\nCharacters: Count = {this._viewModel.Characters.Count}"
                    + $"\nDefault character: {this._viewModel.DefaultCharacter}"
                    + $"\nMask: {FormatColor(this._viewModel.Mask)}";
            }
            b.DrawString(Game1.smallFont, GetDebugInfo(), new Vector2(this.xPositionOnScreen, this.yPositionOnScreen), Color.Blue);
#endif

            if (this._currentSubMenu != null)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
                this._currentSubMenu.draw(b);
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);

            this._currentSubMenu?.receiveKeyPress(key);
        }

        protected override bool CanClose()
        {
            if (this._currentSubMenu is NewPresetMenu newPresetMenu)
                return newPresetMenu.readyToClose();
            else
                return true;
        }

        protected override void OnGameWindowSizeChanged(Point oldSize, Point newSize)
        {
            base.OnGameWindowSizeChanged(oldSize, newSize);

            this._currentSubMenu?.gameWindowSizeChanged(
                new Rectangle(Point.Zero, oldSize),
                new Rectangle(Point.Zero, newSize));
        }

        private void ChangeSubMenu(IClickableMenu? newSubMenu)
        {
            if (object.ReferenceEquals(this._currentSubMenu, newSubMenu))
                return;

            // clean last submenu
            if (this._currentSubMenu is IDisposable disposable)
                disposable.Dispose();

            this._currentSubMenu = newSubMenu;
        }

        private FontManageMenu CreateFontManageMenu()
        {
            void OnMenuOpened(FontManageMenu menu) => this.ChangeSubMenu(menu);
            void OnMenuClosed(FontManageMenu menu) => this.ChangeSubMenu(null);

            var result = new FontManageMenu(
                this._searchManager,
                OnMenuOpened,
                OnMenuClosed);

            return result;
        }

        private NewPresetMenu CreateNewPresetMenu()
        {
            void OnMenuOpened(NewPresetMenu menu) => this.ChangeSubMenu(menu);
            void OnMenuClosed(NewPresetMenu menu) => this.ChangeSubMenu(null);

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
            if (!this._enableLatinDialogueFont && FontHelpers.IsLatinLanguage(lang))
            {
                string locale = FontHelpers.GetCurrentDisplayLocale();

                sb.AppendLine();
                sb.AppendLine()
                    .Append(I18n.Ui_MainMenu_FontTypeHelp_LatinLang_Paragraph(locale));
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
                result = whenStarrySkyInterface;

            return result;
        }

        private class FontAppearance : DataAppearanceBuilder<FontViewModel>
        {
            protected override Element Build(AppearanceBuildContext<FontViewModel> context)
            {
                FontViewModel font = context.Target;

                Thickness margin = new Thickness(4, 0, 0, 0);
                switch (font)
                {
                    case null:
                        return null;

                    case FontFromPackViewModel fontFromPack:
                        var stack = new StackContainer();
                        stack.Orientation = Orientation.Vertical;
                        stack.Margin = margin;
                        {
                            Label fontLabel = this.DefaultLabel(font, Thickness.Zero, HorizontalAlignment.Left, VerticalAlignment.Center);
                            stack.Children.Add(fontLabel);

                            var cpStack = new StackContainer();
                            cpStack.Orientation = Orientation.Horizontal;
                            stack.Children.Add(cpStack);
                            {
                                // TODO: 加一个表示fontpack的图标

                                Label packLabel = new Label();
                                packLabel.Font = FontType.SmallFont;
                                packLabel.Text = I18n.Ui_MainMenu_FontFromPack(fontFromPack.PackManifest.Name);
                                cpStack.Children.Add(packLabel);
                            }
                        }
                        return stack;

                    default:
                        return this.DefaultLabel(font, margin, HorizontalAlignment.Left, VerticalAlignment.Center);
                }
            }

            private Label DefaultLabel(FontViewModel font,
                Thickness margin,
                HorizontalAlignment horizontalAlignment,
                VerticalAlignment verticalAlignment)
            {
                Label l = new Label();
                l.Text = this.GetText(font);
                l.Font = FontType.SmallFont;
                l.Margin = margin;
                l.HorizontalAlignment = horizontalAlignment;
                l.VerticalAlignment = verticalAlignment;
                return l;
            }

            private string GetText(FontViewModel font)
            {
                return font.DisplayText;
            }
        }
    }
}
