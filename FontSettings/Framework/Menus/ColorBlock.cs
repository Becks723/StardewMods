using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus
{
    internal class ColorBlock : Image
    {
        private readonly int _side;

        public override int Width => this._side;

        public override int Height => this._side;

        public ColorBlock(Color color, int side)
        {
            this.ColorMask = color;
            this.Texture = Game1.staminaRect;
            this.SourceRect = null;
            this._side = side;
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(this.Texture, this.Bounds, this.ColorMask);
        }
    }
}
