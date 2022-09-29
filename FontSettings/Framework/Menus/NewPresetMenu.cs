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
            this.width = 400 + borderWidth;
            this.height = 300 + borderWidth;

            context
                .PositionMode(PositionMode.Auto)
                .Aligns(HorizontalAlignment.Center, VerticalAlignment.Center);

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            {
                var backgroundBox = new TextureBox();
                backgroundBox.Kind = TextureBoxes.ThickBorder;
                backgroundBox.Scale = 4f;
                backgroundBox.DrawShadow = false;
                backgroundBox.Padding += new Thickness(borderWidth / 2);
                grid.Children.Add(backgroundBox);
                grid.SetRow(backgroundBox, 0);
                {
                    Grid mainGrid = new Grid();
                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                    backgroundBox.Content = mainGrid;
                    {
                        var titleLabel = new Label();
                        titleLabel.Text = I18n.Ui_NewPresetMenu_Title();
                        titleLabel.Font = FontType.SpriteText;
                        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
                        titleLabel.VerticalAlignment = VerticalAlignment.Top;
                        mainGrid.Children.Add(titleLabel);
                        mainGrid.SetRow(titleLabel, 0);

                        var textbox = this._textbox = new Textbox();
                        textbox.TextChanged += this.OnNameChanged;
                        context.OneWayBinds(() => textbox.String, () => this._viewModel.Name);
                        mainGrid.Children.Add(textbox);
                        mainGrid.SetRow(textbox, 1);

                        var invalidMsgLabel = new Label();
                        invalidMsgLabel.Font = FontType.SmallFont;
                        invalidMsgLabel.Forground = Color.Red;
                        context.OneWayBinds(() => this._viewModel.InvalidNameMessage, () => invalidMsgLabel.Text);
                        mainGrid.Children.Add(invalidMsgLabel);
                        mainGrid.SetRow(invalidMsgLabel, 2);
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
                    okButton.Click += (_, _) => Game1.playSound("money");
                    okButton.Margin = new Thickness(0, 0, borderWidth / 5, 0);
                    context.OneWayBinds(() => this._viewModel.OkCommand, () => okButton.Command);
                    context.OneWayBinds(() => this, () => okButton.CommandParameter);
                    context.OneWayBinds(() => this._viewModel.CanOk, () => okButton.GreyedOut, new TrueFalseConverter());
                    buttonsGrid.Children.Add(okButton);
                    buttonsGrid.SetColumn(okButton, 1);

                    var cancelButton = new TextureButton(Game1.mouseCursors, new Rectangle(192, 256, 64, 64));
                    cancelButton.Click += (_, _) => Game1.playSound("bigDeSelect");
                    cancelButton.Margin = new Thickness(0, 0, 0, 0);
                    context.OneWayBinds(() => this._viewModel.CancelCommand, () => cancelButton.Command);
                    context.OneWayBinds(() => this, () => cancelButton.CommandParameter);
                    buttonsGrid.Children.Add(cancelButton);
                    buttonsGrid.SetColumn(cancelButton, 2);
                }
            }
            context.SetContent(grid);
        }

        protected override void ResetComponents(RootElement root, IBindingContext context)
        {
            { /*
            root.LocalPosition = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen);

            this._background = new TextureBox();
            this._background.LocalPosition = Vector2.Zero;
            this._background.SettableWidth = this.width;
            this._background.SettableHeight = this.height;
            this._background.Kind = TextureBoxes.ThickBorder;
            this._background.Scale = 4f;
            this._background.DrawShadow = false;

            Thickness margin = new Thickness(borderWidth / 2) + this._background.BorderThickness;

            this._label_title = new Label();
            this._label_title.Text = I18n.Ui_NewPresetMenu_Title();
            this._label_title.Font = FontType.SpriteText;
            this._label_title.LocalPosition = new Vector2(this.width / 2 - this._label_title.Width / 2, margin.Top);

            this._label_invalidNameMessage = new Label();
            this._label_invalidNameMessage.Font = FontType.SmallFont;
            this._label_invalidNameMessage.Forground = Color.Red;
            this._label_invalidNameMessage.LocalPosition = new Vector2(margin.Left, this.height - margin.Bottom - this._label_invalidNameMessage.Height);

            this._textbox_name = new Textbox();
            this._textbox_name.LocalPosition = new Vector2(margin.Left, this._label_invalidNameMessage.LocalPosition.Y - 4 - this._textbox_name.Height);
            this._textbox_name.SettableWidth = this.width - (int)margin.Left - (int)margin.Right;
            this._textbox_name.TextChanged += this.OnNameChanged;

            this._button_cancel = new TextureButton(Game1.mouseCursors, new Rectangle(192, 256, 64, 64));
            this._button_cancel.SettableWidth = 64;
            this._button_cancel.SettableHeight = 64;
            this._button_cancel.LocalPosition = new Vector2(this.width - this._button_cancel.Width, this.height + 8);
            this._button_cancel.Click += this.OnCancelClicked;

            this._button_ok = new TextureButton(Game1.mouseCursors, new Rectangle(128, 256, 64, 64));
            this._button_ok.SettableWidth = 64;
            this._button_ok.SettableHeight = 64;
            this._button_ok.LocalPosition = new Vector2(this._button_cancel.LocalPosition.X - 2 - this._button_ok.Width, this._button_cancel.LocalPosition.Y);
            this._button_ok.Click += this.OnOKClicked;

            root.Add(
                this._background,
                this._label_title,
                this._textbox_name,
                this._label_invalidNameMessage,
                this._button_ok,
                this._button_cancel);

            context
                .AddBinding(() => this._viewModel.Name, () => this._textbox_name.String, BindingMode.OneWayReversed)
                .AddBinding(() => this._viewModel.InvalidNameMessage, () => this._label_invalidNameMessage.Text, BindingMode.OneWay)
                .AddBinding(() => this._viewModel.CanOk, () => this._button_ok.GreyedOut, BindingMode.OneWay, new TrueFalseConverter())
                .AddBinding(() => this._viewModel.IsFinished, () => this.IsFinished, BindingMode.OneWay);*/
            }
        }

        protected override bool CanClose()
        {
            return !this._textbox.Focused;
        }

        private void OnNameChanged(object sender, EventArgs e)
        {
            this._viewModel.Name = this._textbox.String;
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
