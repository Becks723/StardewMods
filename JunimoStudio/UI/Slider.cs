//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using StardewValley;
//using StardewValley.Menus;

//namespace JunimoStudio.UI
//{
//    class Slider : Element
//    {

//        public Slider(string label, int x, int y, int minValue, int maxValue, int value, Action<int> setValue, bool withLabel = true, Func<int, string> format = null, bool isGreyedOut = false) : base(label, x, y, 192, 24, minValue, maxValue, value, setValue, format)
//        {
//            this.label = label;
//            this.withLabel = withLabel;
//            greyedOut = isGreyedOut;
//        }
//        public override void LeftClickHeld(int x, int y)
//        {
//            if (!greyedOut && isHeld)
//            {
//                if (x < bounds.X)
//                    this.value = minValue;
//                else if (x > bounds.Right - 40)
//                    this.value = maxValue;
//                else
//                    this.value = (int)((x - this.bounds.X) / (float)(this.bounds.Width - 40) * (maxValue - minValue));
//            }
//        }
//        public override void LeftClickReleased(int x, int y)
//        {
//            base.LeftClickReleased(x, y);
//            setValue(value);
//            Mouse.SetPosition(Game1.getMouseX(), Game1.getMouseY());
//        }
//        public override void ReceiveScrollWheelAction(int direction)
//        {
//            if (!greyedOut && performHoverAction)
//            {
//                if (direction < 0)
//                {
//                    value++;
//                    if (value > maxValue)
//                        value = maxValue;
//                    setValue(value);
//                    return;
//                }
//                if (direction > 0)
//                {
//                    value--;
//                    if (value < minValue)
//                        value = minValue;
//                    setValue(value);
//                }
//            }
//        }
//        public override void Draw(SpriteBatch b)
//        {
//            base.Draw(b);
//            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsSlider.sliderBGSource, this.bounds.X, this.bounds.Y, this.bounds.Width, this.bounds.Height, Color.White, 4f, false);
//            b.Draw(Game1.mouseCursors, new Vector2(this.bounds.X + (this.bounds.Width - 40) * this.value / (maxValue - minValue), this.bounds.Y), new Rectangle?(OptionsSlider.sliderButtonRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
//            if (greyedOut)
//            {
//                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsSlider.sliderBGSource, this.bounds.X, this.bounds.Y, this.bounds.Width, this.bounds.Height, Color.Black * 0.3f, 4f, false);
//                b.Draw(Game1.mouseCursors, new Vector2(this.bounds.X + (this.bounds.Width - 40) * this.value / (maxValue - minValue), this.bounds.Y), new Rectangle?(OptionsSlider.sliderButtonRect), Color.Black * 0.3f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
//            }
//        }
//        public override void Draw(SpriteBatch b, float labelX, float labelY)
//        {
//            if (withLabel)
//            {
//                base.Draw(b, labelX, labelY);
//                Utility.drawTextWithShadow(b, this.Format(this.value), Game1.smallFont, new Vector2(labelX, labelY + 25), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
//            }
//        }
//    }
//}
