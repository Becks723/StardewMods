using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace CodeShared.Integrations.GenericModConfigMenu.Options
{
    internal class FilePathPicker : BaseCustomOption
    {
        public override int Height()
        {
            return 40;
        }

        public override void Draw(SpriteBatch b, Vector2 drawOrigin)
        {
            IClickableMenu.drawTextureBox(b, (int)drawOrigin.X, (int)drawOrigin.Y, 100, this.Height(), Color.White);
        }
    }
}
