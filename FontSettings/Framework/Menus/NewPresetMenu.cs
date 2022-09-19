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
    internal class NewPresetMenu : BaseMenu<NewPresetMenuModel>
    {
        private readonly NewPresetMenuModel _viewModel;

        private TextureBox _background;
        private Label _label_title;
        private Textbox _textbox_name;
        private TextureButton _button_ok;
        private TextureButton _button_cancel;
        private Label _label_invalidNameMessage;

        protected override bool ManualInitializeComponents => true;

        public bool IsFinished { get; set; }

        public event EventHandler<string> Accepted;

        public NewPresetMenu(FontPresetManager presetManager, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            this.ResetComponents();

            this._viewModel = new NewPresetMenuModel(presetManager);
            this._viewModel.Accepted += (_, _) => this.RaisedAccepted(this._viewModel.Name);
            this.DataContext = this._viewModel;
        }

        protected override void ResetComponents(RootElement root, IBindingContext context)
        {
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

            root.AddChildren(
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
                .AddBinding(() => this._viewModel.IsFinished, () => this.IsFinished, BindingMode.OneWay);
        }

        public override void update(GameTime time)
        {
            base.update(time);

            // 错误信息标签的内容改变，标签的高度也会变。所以每次刷新时更新相关的UI位置。
            Thickness margin = new Thickness(borderWidth / 2) + this._background.BorderThickness;
            this._label_invalidNameMessage.LocalPosition = new Vector2(margin.Left, this.height - margin.Bottom - this._label_invalidNameMessage.Height);
            this._textbox_name.LocalPosition = new Vector2(margin.Left, this._label_invalidNameMessage.LocalPosition.Y - 4 - this._textbox_name.Height);
        }

        protected override bool CanClose()
        {
            return !this._textbox_name.Focused;
        }

        private void OnNameChanged(object sender, EventArgs e)
        {
            this._viewModel.Name = this._textbox_name.String;
            this._viewModel.CheckNameValid();
        }

        private void OnOKClicked(object sender, EventArgs e)
        {
            Game1.playSound("money");

            this._viewModel.OnOk();
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            Game1.playSound("bigDeSelect");

            this._viewModel.OnCancel();
        }

        protected virtual void RaisedAccepted(string presetName)
        {
            Accepted?.Invoke(this, presetName);
        }
    }
}
