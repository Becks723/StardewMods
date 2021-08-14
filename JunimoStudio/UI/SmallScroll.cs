//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using JunimoStudio.Instruments;
//using StardewValley;
//using StardewValley.Menus;

//namespace JunimoStudio.UI
//{
//    class SmallScroll : Element
//    {
//        private Rectangle upArrowForSmallScroll = new Rectangle(480, 96, 24, 32);
//        private Rectangle downArrowForSmallScroll = new Rectangle(448, 96, 24, 32);
//        private Vector2 oldMousePos;
//        private Vector2 currentMousePos;
//        private int currentValue;

//        public SmallScroll(string label, int x, int y, int minValue, int maxValue, int value, Action<int> setValue, Func<int, string> format = null, bool withLabel = false) : base(label, x, y, 80, 40, minValue, maxValue, value, setValue, format)
//        {
//            currentValue = value;
//            this.withLabel = withLabel;
//        }
//        public override void LeftClickHeld(int x, int y)
//        {
//            if (isHeld)
//            {
//                currentMousePos = new Vector2(Game1.getMouseX(), Game1.getMouseY());
//                if (currentMousePos != oldMousePos)
//                {
//                    if (y <= oldMousePos.Y - (currentValue - minValue))
//                        value = minValue;
//                    else if (y >= oldMousePos.Y + (maxValue - currentValue))
//                        value = maxValue;
//                    else
//                        value = currentValue + y - (int)oldMousePos.Y;
//                    value = (int)MathHelper.Clamp(value, minValue, maxValue);
//                }
//            }
//        }
//        public override void LeftClickReleased(int x, int y)
//        {
//            base.LeftClickReleased(x, y);
//            setValue(value);
//            currentValue = value;
//            Mouse.SetPosition((int)oldMousePos.X, (int)oldMousePos.Y);
//            SetSoundMenu.isMouseVisible = true;
//        }
//        public override void ReceiveLeftClick(int x, int y)
//        {
//            base.ReceiveLeftClick(x, y);
//            oldMousePos = new Vector2(Game1.getMouseX(), Game1.getMouseY());
//            SetSoundMenu.isMouseVisible = false;
//        }
//        public override void ReceiveScrollWheelAction(int direction)
//        {
//            if (!greyedOut)
//            {
//                if (performHoverAction)
//                {
//                    if (direction < 0)
//                    {
//                        value++;
//                        if (value > maxValue)
//                            value = maxValue;
//                        setValue(value);
//                        return;
//                    }
//                    if (direction > 0)
//                    {
//                        value--;
//                        if (value < minValue)
//                            value = minValue;
//                        setValue(value);
//                    }
//                }
//                else
//                    setValue(value);
//            }
//        }
//        public override void Draw(SpriteBatch b)
//        {
//            if (withLabel)
//                base.Draw(b);
//            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), this.bounds.X, this.bounds.Y, this.bounds.Width, this.bounds.Height, isHeld ? Color.White : Color.WhiteSmoke, 0.6f, false);
//            b.Draw(Game1.mouseCursors, new Vector2(this.bounds.X + 18, this.bounds.Y + 20 - (performHoverAction || isHeld ? 2 : 0)), upArrowForSmallScroll, Color.White, MathHelper.PiOver2, new Vector2(24, 16.5f), 0.3f, SpriteEffects.None, 0.7f);
//            b.Draw(Game1.mouseCursors, new Vector2(this.bounds.X + 18, this.bounds.Y + 22 + (performHoverAction || isHeld ? 2 : 0)), downArrowForSmallScroll, Color.White, MathHelper.PiOver2, new Vector2(0, 16.5f), 0.3f, SpriteEffects.None, 0.7f);
//            Utility.drawTextWithShadow(b, this.Format(value), Game1.smallFont, new Vector2(this.bounds.X + 28, this.bounds.Y + 5), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
//        }
//        public override void Draw(SpriteBatch b, float labelX, float labelY)
//        {
//            if (withLabel)
//            {
//                base.Draw(b, labelX, labelY);
//                Utility.drawTextWithShadow(b, this.Format(this.value) + " BPM", Game1.smallFont, new Vector2(labelX, labelY + 25), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
//            }
//        }
//    }
//}
