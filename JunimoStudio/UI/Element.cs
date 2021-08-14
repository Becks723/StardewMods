//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using StardewValley;
//using StardewValley.BellsAndWhistles;

//namespace JunimoStudio.UI
//{
//    public class Element
//    {
//        public string label;
//        public Rectangle bounds;
//        public bool greyedOut;
//        public bool isHeld;
//        public bool performHoverAction;
//        public int value;
//        public int minValue;
//        public int maxValue;
//        public Func<int, string> Format;
//        public Action<int> setValue;
//        public bool withLabel;

//        public Element(string label)
//        {
//            this.label = label;
//        }
//        public Element(string label, int x, int y, int width, int height) : this(label)
//        {
//            bounds = new Rectangle(x, y, width, height);
//        }
//        public Element(string label, int x, int y, int width, int height, int minValue, int maxValue, int value, Action<int> setValue, Func<int, string> format = null) : this(label, x, y, width, height)
//        {
//            this.minValue = minValue;
//            this.maxValue = maxValue;
//            this.value = value;
//            this.setValue = setValue;
//            this.Format = format;
//            if (this.Format == null)
//                this.Format = new Func<int, string>(val => { return val.ToString(); });
//        }
//        public virtual void ReceiveLeftClick(int x, int y) { isHeld = true; }
//        public virtual void LeftClickHeld(int x, int y)
//        {
//        }
//        public virtual void LeftClickReleased(int x, int y) { isHeld = false; }
//        public virtual void ReceiveScrollWheelAction(int direction)
//        {
//        }
//        public virtual void ReceiveKeyPress(Keys key)
//        {
//        }
//        public virtual void Draw(SpriteBatch b)
//        {
//            Utility.drawTextWithShadow(b, this.label, Game1.smallFont, new Vector2(this.bounds.Right + 8, this.bounds.Y - 2), this.greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f, -1, -1, 1f, 3);
//        }
//        public virtual void Draw(SpriteBatch b, float labelX, float labelY)
//        {
//            Utility.drawTextWithShadow(b, this.label, Game1.smallFont, new Vector2(labelX, labelY), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
//        }
//    }
//}
