using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValleyUI;
using StardewValleyUI.Controls;
using StardewValleyUI.Data;
using StardewValleyUI.Data.Converters;
using StardewValleyUI.Menus;

namespace FontSettings.Framework.Menus
{
    internal class NewPresetMenu : BaseMenu<NewPresetMenuModel>, IOverlayMenu
    {
        private readonly NewPresetMenuModel _viewModel;
        private readonly Action<NewPresetMenu> _onOpened;
        private readonly Action<NewPresetMenu> _onClosed;
        private Textbox _textbox;

        public event EventHandler<OverlayMenuClosedEventArgs> Closed;

        public NewPresetMenu(FontPresetManager presetManager, Action<NewPresetMenu> onOpened, Action<NewPresetMenu> onClosed)
        {
            this._onOpened = onOpened;
            this._onClosed = onClosed;

            this.ResetComponents();

            this._viewModel = new NewPresetMenuModel(presetManager);
            this.DataContext = this._viewModel;
        }

        protected override void ResetComponents(MenuInitializationContext context)
        {
            Grid grid = new Grid();
            grid.SuggestedWidth = 400 + borderWidth;
            grid.SuggestedHeight = 300 + borderWidth;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            {
                ScrollViewer scrollViewer = new ScrollViewer();
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
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
                grid.Children.Add(scrollViewer);
                grid.SetRow(scrollViewer, 0);
                {
                    var stack = new StackContainer();
                    stack.Orientation = Orientation.Vertical;
                    stack.Margin = new Thickness(borderWidth / 2);
                    scrollViewer.Content = stack;
                    {
                        var titleLabel = new Label();
                        titleLabel.Text = I18n.Ui_NewPresetMenu_Title();
                        titleLabel.Font = FontType.SpriteText;
                        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
                        titleLabel.Margin = new Thickness(0, 0, 0, borderWidth / 2);
                        stack.Children.Add(titleLabel);

                        var nameLabel = new Label();
                        nameLabel.Text = I18n.Ui_NewPresetMenu_Name();
                        nameLabel.Font = FontType.DialogueFont;
                        nameLabel.HorizontalAlignment = HorizontalAlignment.Left;
                        nameLabel.Margin = new Thickness(0, 0, 0, borderWidth / 6);
                        stack.Children.Add(nameLabel);

                        var textbox = this._textbox = new Textbox();
                        textbox.HorizontalAlignment = HorizontalAlignment.Left;
                        textbox.Margin = new Thickness(0, 0, 0, 0);
                        textbox.SuggestedWidth = 300;
                        textbox.TextChanged += this.OnNameChanged;
                        context.OneWayBinds(() => textbox.Text, () => this._viewModel.Name);
                        stack.Children.Add(textbox);

                        var invalidMsgLabel = new Label();
                        invalidMsgLabel.Font = FontType.SmallFont;
                        invalidMsgLabel.Forground = Color.Red;
                        invalidMsgLabel.HorizontalAlignment = HorizontalAlignment.Left;
                        invalidMsgLabel.Margin = new Thickness(0, 0, 0, 0);
                        context.OneWayBinds(() => this._viewModel.InvalidNameMessage, () => invalidMsgLabel.Text);
                        stack.Children.Add(invalidMsgLabel);
                    }
                }

                Grid buttonsGrid = new Grid();
                buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                buttonsGrid.Margin = new Thickness(0, borderWidth / 5, 0, 0);
                grid.Children.Add(buttonsGrid);
                grid.SetRow(buttonsGrid, 1);
                {
                    var okButton = new TextureButton(Game1.mouseCursors, new Rectangle(128, 256, 64, 64));
                    okButton.ClickSound = "money";
                    okButton.Margin = new Thickness(0, 0, borderWidth / 5, 0);
                    context.OneWayBinds(() => this._viewModel.OkCommand, () => okButton.Command);
                    context.OneWayBinds(() => this, () => okButton.CommandParameter);
                    context.OneWayBinds(() => this._viewModel.CanOk, () => okButton.GreyedOut, new TrueFalseConverter());
                    buttonsGrid.Children.Add(okButton);
                    buttonsGrid.SetColumn(okButton, 1);

                    var cancelButton = new TextureButton(Game1.mouseCursors, new Rectangle(192, 256, 64, 64));
                    cancelButton.ClickSound = "bigDeSelect";
                    cancelButton.Margin = new Thickness(0, 0, 0, 0);
                    context.OneWayBinds(() => this._viewModel.CancelCommand, () => cancelButton.Command);
                    context.OneWayBinds(() => this, () => cancelButton.CommandParameter);
                    buttonsGrid.Children.Add(cancelButton);
                    buttonsGrid.SetColumn(cancelButton, 2);
                }
            }
            context.SetRootElement(grid);
        }

        //protected override void ResetComponents(MenuInitializationContext context)
        //{
        //    Grid grid = new Grid();
        //    grid.SuggestedWidth = 400 + borderWidth;
        //    grid.SuggestedHeight = 300 + borderWidth;
        //    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
        //    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        //    {
        //        var backgroundBox = new TextureBoxBorder();
        //        backgroundBox.Box = TextureBoxes.ThickBorder;
        //        backgroundBox.DrawShadow = false;
        //        backgroundBox.Padding += new Thickness(borderWidth / 2);
        //        grid.Children.Add(backgroundBox);
        //        grid.SetRow(backgroundBox, 0);
        //        {
        //            Grid mainGrid = new Grid();
        //            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
        //            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
        //            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
        //            backgroundBox.Child = mainGrid;
        //            {
        //                var titleLabel = new Label();
        //                titleLabel.Text = I18n.Ui_NewPresetMenu_Title();
        //                titleLabel.Font = FontType.SpriteText;
        //                titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        //                titleLabel.VerticalAlignment = VerticalAlignment.Top;
        //                mainGrid.Children.Add(titleLabel);
        //                mainGrid.SetRow(titleLabel, 0);

        //                var textbox = this._textbox = new Textbox();
        //                textbox.TextChanged += this.OnNameChanged;
        //                context.OneWayBinds(() => textbox.Text, () => this._viewModel.Name);
        //                mainGrid.Children.Add(textbox);
        //                mainGrid.SetRow(textbox, 1);

        //                var invalidMsgLabel = new Label();
        //                invalidMsgLabel.Font = FontType.SmallFont;
        //                invalidMsgLabel.Forground = Color.Red;
        //                context.OneWayBinds(() => this._viewModel.InvalidNameMessage, () => invalidMsgLabel.Text);
        //                mainGrid.Children.Add(invalidMsgLabel);
        //                mainGrid.SetRow(invalidMsgLabel, 2);
        //            }
        //        }

        //        Grid buttonsGrid = new Grid();
        //        buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
        //        buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        //        buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        //        buttonsGrid.Margin = new Thickness(0, borderWidth / 5, 0, 0);
        //        grid.Children.Add(buttonsGrid);
        //        grid.SetRow(buttonsGrid, 1);
        //        {
        //            var okButton = new TextureButton(Game1.mouseCursors, new Rectangle(128, 256, 64, 64));
        //            okButton.ClickSound = "money";
        //            okButton.Margin = new Thickness(0, 0, borderWidth / 5, 0);
        //            context.OneWayBinds(() => this._viewModel.OkCommand, () => okButton.Command);
        //            context.OneWayBinds(() => this, () => okButton.CommandParameter);
        //            context.OneWayBinds(() => this._viewModel.CanOk, () => okButton.GreyedOut, new TrueFalseConverter());
        //            buttonsGrid.Children.Add(okButton);
        //            buttonsGrid.SetColumn(okButton, 1);

        //            var cancelButton = new TextureButton(Game1.mouseCursors, new Rectangle(192, 256, 64, 64));
        //            cancelButton.ClickSound = "bigDeSelect";
        //            cancelButton.Margin = new Thickness(0, 0, 0, 0);
        //            context.OneWayBinds(() => this._viewModel.CancelCommand, () => cancelButton.Command);
        //            context.OneWayBinds(() => this, () => cancelButton.CommandParameter);
        //            buttonsGrid.Children.Add(cancelButton);
        //            buttonsGrid.SetColumn(cancelButton, 2);
        //        }
        //    }
        //    context.SetRootElement(grid);
        //}

        protected override bool CanClose()
        {
            return !this._textbox.Focused;
        }

        private void OnNameChanged(object sender, EventArgs e)
        {
            this._viewModel.CheckNameValid();
        }

        private void RaiseClosed(object? parameter)
        {
            Closed?.Invoke(this, new OverlayMenuClosedEventArgs(parameter));
        }

        void IOverlayMenu.Open()
        {
            this._onOpened(this);
        }

        void IOverlayMenu.Close(object? parameter)
        {
            this.RaiseClosed(parameter);

            this._onClosed(this);
        }
    }
}
