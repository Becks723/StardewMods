using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeShared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus
{
    internal class FontExampleBorder : TextureBox
    {
        public override void Draw(SpriteBatch b)
        {
            Rectangle scissor = new Rectangle(
                (int)(this.Position.X),
                (int)(this.Position.Y),
                (int)(this.ActualWidth),
                (int)(this.ActualHeight));

            b.InNewScissoredState(scissor, Vector2.Zero, () =>
            {
                base.Draw(b);
            });
        }
    }
}
