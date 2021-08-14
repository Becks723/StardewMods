//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using StardewValley;
//using StardewValley.Menus;

//namespace JunimoStudio.UI
//{
//    public class InputBox : TextBox
//    {
//        public Rectangle Bounds { get; set; }
//        public double Target { get; set; }
//        public bool greyedOut;
//        public string Label { get; set; }
//        public bool hoverAction;
//        public InputBox(string label, double target, int x, int y, int width, int height, bool greyedOut) : base(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
//        {
//            Label = label;
//            Target = target;
//            X = x;
//            Y = y;
//            Width = width;
//            Height = height;
//            Bounds = new Rectangle(X, Y, Width, Height);
//            this.greyedOut = greyedOut;
//        }
//        public override void Draw(SpriteBatch b, bool drawShadow = true)
//        {
//            base.Draw(b, drawShadow);
//            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), X + Width + 40, Y + 4, 60, Height - 8, Color.White, 0.5f, false);
//            Utility.drawTextWithShadow(b, double.Parse(Target.ToString("#0.00")).ToString(), Game1.smallFont, new Vector2(X + Width + 48, Y + 8), Game1.textColor * (greyedOut ? 0.2f : 1f), 1f, -1, -1, -1, 1, 3);
//            if (!Selected)
//                Utility.drawTextWithShadow(b, Label, Game1.smallFont, new Vector2(X + 16, Y + 8), Game1.textColor * 0.2f, 1, -1, -1, -1, 1, 3);
//            if (greyedOut)
//            {
//                IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), X + Width + 40, Y + 4, 60, Height - 8, Color.Black * 0.3f, 0.5f, false);
//                b.Draw(_textBoxTexture, new Rectangle(X, Y, Width, Height), new Rectangle?(new Rectangle(0, 0, 192, 48)), Color.Black * 0.2f);
//            }
//        }
//    }
//}
