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
//    class CheckBox : Element
//    {
//        public bool isChecked;
//        public new Action<bool> setValue;

//        public CheckBox(string label, int x, int y, bool value, Action<bool> setValue, bool withLabel = true) : base(label, x, y, 36, 36)
//        {
//            isChecked = value;
//            this.setValue = setValue;
//            this.withLabel = withLabel;
//        }
//        public override void ReceiveLeftClick(int x, int y)
//        {
//            isChecked = !isChecked;
//            this.setValue(isChecked);
//            Game1.playSound("drumkit6");
//        }
//        public override void Draw(SpriteBatch b)
//        {
//            if (withLabel)
//                base.Draw(b);
//            b.Draw(Game1.mouseCursors, new Vector2(this.bounds.X, this.bounds.Y), new Rectangle?(this.isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
//        }
//        public override void Draw(SpriteBatch b, float labelX, float labelY)
//        {
//            if (withLabel)
//            {
//                base.Draw(b, labelX, labelY);
//                Utility.drawTextWithShadow(b, isChecked.ToString(), Game1.smallFont, new Vector2(labelX, labelY + 25), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
//            }
//        }
//    }
//}
