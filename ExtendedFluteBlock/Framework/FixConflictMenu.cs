using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace FluteBlockExtension.Framework
{
    internal class FixOptionSelectedEventArgs : EventArgs
    {
        public bool Always { get; }

        public FixOption Option { get; }

        public SObject FluteBlock { get; }

        public FixOptionSelectedEventArgs(FixOption option, bool always, SObject fluteBlock)
        {
            this.Option = option;
            this.Always = always;
            this.FluteBlock = fluteBlock;
        }
    }

    internal class FixConflictMenu : IClickableMenu
    {
        private readonly SObject _fluteBlock;

        private readonly OptionsButton _fixOption_UseCurrent_Button;
        private readonly OptionsButton _fixOption_ExtraPitch_Button;
        private readonly OptionsCheckbox _rememberMyChoice_Checkbox;
        private readonly OptionsElement _titleLabel;
        private readonly OptionsElement _fluteBlockLocation;
        private readonly OptionsElement _fluteBlockCoordinate;
        private bool _showTitleTooltip;

        public event EventHandler<FixOptionSelectedEventArgs> OptionSelected;

        public FixConflictMenu(ProblemFluteBlock fluteBlock)
            : base(Game1.uiViewport.Width / 2 - 300, Game1.uiViewport.Height / 2 - 250, 600, 500)
        {
            this._fluteBlock = fluteBlock.Core;
            this._fixOption_UseCurrent_Button = new OptionsButton(I18n.FixMenu_Labels_ApplyOriginalGame(), this.OnApplyCurrentButtonClicked);
            this._fixOption_ExtraPitch_Button = new OptionsButton(I18n.FixMenu_Labels_ApplyExtraPitch(), this.OnApplyExtraPitchButtonClicked);
            this._rememberMyChoice_Checkbox = new OptionsCheckbox(I18n.FixMenu_Labels_RememberMyChoice(), -1);
            string titleStr = I18n.FixMenu_Labels_Title();
            this._titleLabel = new OptionsElement(titleStr);
            this._titleLabel.style = OptionsElement.Style.OptionLabel;
            this._titleLabel.labelOffset = new Vector2(20, 0);
            var strSize = Game1.dialogueFont.MeasureString(titleStr).ToPoint();
            this._titleLabel.bounds.Width = strSize.X + (int)this._titleLabel.labelOffset.X + 20;
            this._titleLabel.bounds.Height = strSize.Y + (int)this._titleLabel.labelOffset.Y + 20;

            Point tilePos = fluteBlock.TilePosition.ToPoint();
            string coordStr = I18n.FixMenu_Labels_FluteBlockCoord(tilePos.X, tilePos.Y);
            this._fluteBlockCoordinate = new OptionsElement(coordStr);
            this._fluteBlockCoordinate.style = OptionsElement.Style.OptionLabel;
            this._fluteBlockCoordinate.bounds.Height = 51;

            string locStr = I18n.FixMenu_Labels_FluteBlockLocation(fluteBlock.Location.Name);
            this._fluteBlockLocation = new OptionsElement(locStr);
            this._fluteBlockLocation.style = OptionsElement.Style.OptionLabel;
            this._fluteBlockLocation.bounds.Height = 51;

            this.UpdatePositions();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            this.UpdatePositions();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            this._fixOption_UseCurrent_Button.receiveLeftClick(x, y);
            this._fixOption_ExtraPitch_Button.receiveLeftClick(x, y);
            if (this._rememberMyChoice_Checkbox.bounds.Contains(x, y))
                this._rememberMyChoice_Checkbox.receiveLeftClick(x, y);
        }

        public override void performHoverAction(int x, int y)
        {
            if (this._titleLabel.bounds.Contains(x, y))
            {
                this._showTitleTooltip = true;
            }
            else
            {
                this._showTitleTooltip = false;
            }
        }

        public override bool isWithinBounds(int x, int y)
        {
            return base.isWithinBounds(x, y) || this._titleLabel.bounds.Contains(x, y);
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            }

            drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White);

            drawTextureBox(b, this._titleLabel.bounds.X, this._titleLabel.bounds.Y,
                this._titleLabel.bounds.Width,
                this._titleLabel.bounds.Height, Color.White);
            this._titleLabel.draw(b, 0, 0, this);
            this._fluteBlockCoordinate.draw(b, 0, 0, this);
            this._fluteBlockLocation.draw(b, 0, 0, this);
            this._rememberMyChoice_Checkbox.draw(b, 0, 0, this);
            this._fixOption_ExtraPitch_Button.draw(b, 0, 0, this);
            this._fixOption_UseCurrent_Button.draw(b, 0, 0, this);

            if (this._showTitleTooltip)
            {
                drawToolTip(b, I18n.FixMenu_Labels_Title_Tooltip(), "", null);
            }

            this.drawMouse(b);
        }

        public override bool readyToClose()
        {
            return false;
        }

        private void OnApplyExtraPitchButtonClicked()
        {
            this.exitThisMenu();
            this.RaiseOptionSelected(FixOption.ApplyExtraPitch);
        }

        private void OnApplyCurrentButtonClicked()
        {
            this.exitThisMenu();
            this.RaiseOptionSelected(FixOption.ApplyGamePitch);
        }

        private void RaiseOptionSelected(FixOption option)
        {
            this.RaiseOptionSelected(option, this._rememberMyChoice_Checkbox.isChecked);
        }

        private void RaiseOptionSelected(FixOption option, bool always)
        {
            OptionSelected?.Invoke(this, new FixOptionSelectedEventArgs(option, always, this._fluteBlock));
        }

        private void UpdatePositions()
        {
            this._titleLabel.bounds.X = this.xPositionOnScreen + this.width / 2 - this._titleLabel.bounds.Width / 2;
            this._titleLabel.bounds.Y = this.yPositionOnScreen - borderWidth - this._titleLabel.bounds.Height;

            this._fixOption_UseCurrent_Button.bounds.X = this.xPositionOnScreen + borderWidth;
            this._fixOption_UseCurrent_Button.bounds.Y = this.yPositionOnScreen + this.height - borderWidth - this._fixOption_UseCurrent_Button.bounds.Height;

            this._fixOption_ExtraPitch_Button.bounds.X = this.xPositionOnScreen + borderWidth;
            this._fixOption_ExtraPitch_Button.bounds.Y = this._fixOption_UseCurrent_Button.bounds.Y - borderWidth - this._fixOption_ExtraPitch_Button.bounds.Height;

            this._rememberMyChoice_Checkbox.bounds.X = this.xPositionOnScreen + borderWidth;
            this._rememberMyChoice_Checkbox.bounds.Y = this._fixOption_ExtraPitch_Button.bounds.Y - borderWidth - this._rememberMyChoice_Checkbox.bounds.Height;
            this._rememberMyChoice_Checkbox.labelOffset = new Vector2(40, -5);

            this._fluteBlockLocation.bounds.X = this.xPositionOnScreen + borderWidth;
            this._fluteBlockLocation.bounds.Y = this._rememberMyChoice_Checkbox.bounds.Y - borderWidth - this._fluteBlockLocation.bounds.Height;

            this._fluteBlockCoordinate.bounds.X = this.xPositionOnScreen + borderWidth;
            this._fluteBlockCoordinate.bounds.Y = this._fluteBlockLocation.bounds.Y - borderWidth - this._fluteBlockCoordinate.bounds.Height;
        }
    }
}
