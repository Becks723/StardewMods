using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Menus.Controls;
using JunimoStudio.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace JunimoStudio.Menus
{
    internal class NotePropertiesMenu : IClickableMenu
    {
        private RootElement _root;

        private readonly INote _note;

        public NotePropertiesMenu(INote coreNote)
        {
            _note = coreNote /*?? throw new ArgumentNullException(nameof(coreNote))*/;

            ResetComponents();
        }

        public override void update(GameTime gameTime)
        {
            _root.Update(gameTime);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
            _root.Draw(b);
        }

        private void ResetComponents()
        {
            xPositionOnScreen = Game1.viewport.Width / 2 - 150;
            yPositionOnScreen = Game1.viewport.Height / 2 - 150;
            width = 300;
            height = 300;

            _root = new RootElement();
        }
    }
}
