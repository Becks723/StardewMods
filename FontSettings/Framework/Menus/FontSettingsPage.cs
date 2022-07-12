using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace FontSettings.Framework.Menus
{
    internal class FontSettingsPage : IClickableMenu
    {
        private readonly ClickableTextureComponent _leftArrow, _rightArrow;

        private Rectangle _titleBounds;

        private Rectangle _exampleBoardBounds;

        private readonly OptionsCheckBox _box_enabledFont;

        private readonly OptionsDropDown _dropDown_font;

        private readonly OptionsSlider<int> _slider_fontSize, _slider_spacing, _slider_lineSpacing;

        private readonly OKButton _okButton;

        private readonly OptionsCheckBox _seperateExampleButton;  // TODO: 改成图标？

        private readonly OptionsCheckBox _box_showTextBounds, _box_showText;

        private readonly Rectangle _vanillaColorBlockBounds, _vanillaLabelBounds,
                                   _customColorBlockBounds, _customLabelBounds;

        private static readonly StateManager _states = new();

        private GameFontType CurrentFontType { get; set; }

        public FontSettingsPage(int x, int y, int width, int height, bool showUpperRightCloseButton = false)
            : base(x, y, width, height, showUpperRightCloseButton)
        {
            this._leftArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen - 96 - 48, this.yPositionOnScreen + height / 2, 48, 44), Game1.mouseCursors, new(352, 495, 12, 11), 4f);
            this._rightArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 96, this.yPositionOnScreen + height / 2, 48, 44), Game1.mouseCursors, new(365, 495, 12, 11), 4f);

            string titleText = this.CurrentFontType.LocalizedName();
            Vector2 titleSize = new Vector2(
                SpriteText.getWidthOfString(titleText),
                SpriteText.getHeightOfString(titleText));
            this._titleBounds = new Rectangle(this.xPositionOnScreen + this.width / 2 - (int)(titleSize.X / 2), this.yPositionOnScreen + 108, (int)titleSize.X, (int)titleSize.Y);

            int boardX = this.xPositionOnScreen + spaceToClearSideBorder + borderWidth;
            this._exampleBoardBounds = new Rectangle(boardX, this._titleBounds.Bottom, this.xPositionOnScreen + width - spaceToClearSideBorder - borderWidth - boardX, height / 3);

            this._seperateExampleButton = new OptionsCheckBox("合并示例", false, this._exampleBoardBounds.X + borderWidth / 3);
            this._box_showTextBounds = new OptionsCheckBox("显示边框", false, this._exampleBoardBounds.X + borderWidth / 3);
            this._box_showText = new OptionsCheckBox("显示文字", false, this._exampleBoardBounds.X + borderWidth / 3);
            int gap = (this._exampleBoardBounds.Height - this._seperateExampleButton.bounds.Height - this._box_showTextBounds.bounds.Height - this._box_showText.bounds.Height) / 4;
            this._seperateExampleButton.bounds.Y = this._exampleBoardBounds.Y + gap;
            this._box_showTextBounds.bounds.Y = this._seperateExampleButton.bounds.Bottom + gap;
            this._box_showText.bounds.Y = this._box_showTextBounds.bounds.Bottom + gap;

            string vanillaText = I18n.OptionsPage_OriginalExample();
            string customText = I18n.OptionsPage_CustomExample();
            Point colorBlockSize = new Point(20, 20);
            Point vanillaLabelTextSize = Game1.dialogueFont.MeasureString(vanillaText).ToPoint();
            Point customLabelTextSize = Game1.dialogueFont.MeasureString(customText).ToPoint();
            this._customLabelBounds = new Rectangle(this._exampleBoardBounds.Right - borderWidth / 2 - Math.Max(vanillaLabelTextSize.X, customLabelTextSize.X), this._exampleBoardBounds.Bottom - borderWidth / 2 - customLabelTextSize.Y, customLabelTextSize.X, customLabelTextSize.Y);
            this._customColorBlockBounds = new Rectangle(this._customLabelBounds.X - borderWidth / 5 - colorBlockSize.X, this._customLabelBounds.Y + customLabelTextSize.Y / 2 - colorBlockSize.Y / 2, colorBlockSize.X, colorBlockSize.Y);
            this._vanillaColorBlockBounds = new Rectangle(this._customColorBlockBounds.X, this._customLabelBounds.Y - borderWidth / 5 - vanillaLabelTextSize.Y / 2 - colorBlockSize.Y / 2, colorBlockSize.X, colorBlockSize.Y);
            this._vanillaLabelBounds = new Rectangle(this._customLabelBounds.X, this._customLabelBounds.Y - borderWidth / 5 - vanillaLabelTextSize.Y, vanillaLabelTextSize.X, vanillaLabelTextSize.Y);

            gap = (int)(borderWidth / 1.2);
            this._box_enabledFont = new OptionsCheckBox(I18n.OptionsPage_Enable(), false, this._exampleBoardBounds.X, this._exampleBoardBounds.Bottom + gap);
            this._dropDown_font = new OptionsDropDown(string.Empty);
            this._dropDown_font.bounds.X = this._exampleBoardBounds.Right - this._dropDown_font.bounds.Width;
            this._dropDown_font.bounds.Y = this._exampleBoardBounds.Bottom + gap;

            this._slider_fontSize = new OptionsSlider<int>(I18n.OptionsPage_FontSize(), this._box_enabledFont.bounds.X, this._box_enabledFont.bounds.Bottom + gap);
            this._slider_spacing = new OptionsSlider<int>(I18n.OptionsPage_Spacing(), this._box_enabledFont.bounds.X, this._slider_fontSize.bounds.Bottom + gap);
            this._slider_lineSpacing = new OptionsSlider<int>(I18n.OptionsPage_LineSpacing(), this._box_enabledFont.bounds.X, this._slider_spacing.bounds.Bottom + gap);

            this._okButton = new OKButton(this.xPositionOnScreen + width - spaceToClearSideBorder - borderWidth - 64, this.yPositionOnScreen + height - spaceToClearSideBorder - borderWidth - 64);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (this._leftArrow.containsPoint(x, y))
            {
                this.ChangeFontType(this.CurrentFontType.Previous(LocalizedContentManager.CurrentLanguageLatin));
                Game1.playSound("smallSelect");
            }
            else if (this._rightArrow.containsPoint(x, y))
            {
                this.ChangeFontType(this.CurrentFontType.Next(LocalizedContentManager.CurrentLanguageLatin));
                Game1.playSound("smallSelect");
            }
            else if (this._seperateExampleButton.bounds.Contains(x, y))
                this._seperateExampleButton.receiveLeftClick(x, y);
            else if (this._box_showTextBounds.bounds.Contains(x, y))
                this._box_showTextBounds.receiveLeftClick(x, y);
            else if (this._box_showText.bounds.Contains(x, y))
                this._box_showText.receiveLeftClick(x, y);
            else if (this._box_enabledFont.bounds.Contains(x, y))
                this._box_enabledFont.receiveLeftClick(x, y);
            else if (this._okButton.bounds.Contains(x, y))
                this._okButton.receiveLeftClick(x, y);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this._leftArrow.tryHover(x, y);
            this._rightArrow.tryHover(x, y);
            this._okButton.PerformHoverAction(x, y);
        }

        public override void update(GameTime time)
        {
            base.update(time);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            this._leftArrow.draw(b);
            this._rightArrow.draw(b);
            SpriteText.drawString(b, this.CurrentFontType.LocalizedName(), this._titleBounds.X, this._titleBounds.Y);
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new(64, 320, 60, 60), this._exampleBoardBounds.X, this._exampleBoardBounds.Y, this._exampleBoardBounds.Width, this._exampleBoardBounds.Height, Color.White, drawShadow: false);
            this._seperateExampleButton.draw(b, 0, 0);
            this._box_showTextBounds.draw(b, 0, 0);
            this._box_showText.draw(b, 0, 0);
            this._box_enabledFont.draw(b, 0, 0);
            this._dropDown_font.draw(b, 0, 0);
            this._slider_fontSize.draw(b, 0, 0);
            this._slider_spacing.draw(b, 0, 0);
            this._slider_lineSpacing.draw(b, 0, 0);
            this._okButton.draw(b, 0, 0);

            // 图例标注
            b.Draw(Game1.staminaRect, this._vanillaColorBlockBounds, Color.Gray * 0.33f);
            b.Draw(Game1.staminaRect, this._customColorBlockBounds, Game1.textColor);
            b.DrawString(Game1.dialogueFont, I18n.OptionsPage_OriginalExample(), this._vanillaLabelBounds.Location.ToVector2(), Game1.textColor);
            b.DrawString(Game1.dialogueFont, I18n.OptionsPage_CustomExample(), this._customLabelBounds.Location.ToVector2(), Game1.textColor);
        }

        private void ChangeFontType(GameFontType newFontType)
        {

        }

        private class StateManager
        {
            private readonly Dictionary<GameFontType, bool> _states = new();

            public StateManager()
            {
                foreach (GameFontType key in Enum.GetValues<GameFontType>())
                    this._states[key] = false;
            }

            public bool IsOn(GameFontType fontType)
            {
                return this._states[fontType];
            }

            public void On(GameFontType fontType)
            {
                if (!this._states[fontType])
                    this._states[fontType] = true;
            }

            public void Off(GameFontType fontType)
            {
                if (this._states[fontType])
                    this._states[fontType] = false;
            }
        }
    }
}
