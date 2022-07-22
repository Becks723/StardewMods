using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FontSettings.Framework.Menus
{
    internal class OptionsFontExample : OptionsElement
    {
        public ISpriteFont ExampleFont { get; set; }

        public string ExampleText { get; set; }

        public OptionsFontExample(string label, int slotWidth, int slotHeight, int x = -1, int y = -1)
            : base(label, x, 0, slotWidth - x, slotHeight)
        {
        }

        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            Vector2 lablSize = Game1.smallFont.MeasureString(this.label);
            b.DrawString(Game1.smallFont, this.label, new(slotX + this.bounds.X, slotY + this.bounds.Center.Y - lablSize.Y / 2), Game1.textColor);

            if (this.ExampleFont != null)
            {
                string exampleText = this.ExampleText ?? string.Empty;
                Vector2 exampleSize = this.ExampleFont.MeasureString(exampleText);
                Rectangle totalBounds = new(slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width, this.bounds.Height);
                Vector2 examplePos = new(totalBounds.Center.X - exampleSize.X / 2, totalBounds.Center.Y - exampleSize.Y / 2);
                this.ExampleFont.Draw(b, exampleText, examplePos, Game1.textColor);
            }
        }
    }
}
