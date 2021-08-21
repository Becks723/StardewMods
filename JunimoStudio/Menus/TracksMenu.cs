using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace JunimoStudio.Menus
{
    internal class TracksMenu : CarpenterMenu
    {
        private readonly IList<TrackInfo> _tracks;

        public TracksMenu(IList<TrackInfo> tracks)
        {
            this._tracks = tracks;
        }

        public override void draw(SpriteBatch b)
        {
            string msg = "TestString";
            SpriteText.drawStringWithScrollBackground(b, msg, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(msg) / 2, 16);
            this.drawMouse(b);
        }

        public override void update(GameTime time)
        {
            if (!Game1.IsFading())
            {
                // 移动视窗（鼠标处在屏幕边缘自动移动视窗）。
                var viewport = Game1.viewport;
                int mouseX = Game1.getOldMouseX(false) + viewport.X;
                int mouseY = Game1.getOldMouseY(false) + viewport.Y;
                if (mouseX - Game1.viewport.X < 64)
                {
                    Game1.panScreen(-8, 0);
                }
                else if (mouseX - (viewport.X + viewport.Width) >= -128)
                {
                    Game1.panScreen(8, 0);
                }
                if (mouseY - viewport.Y < 64)
                {
                    Game1.panScreen(0, -8);
                }
                else if (mouseY - (viewport.Y + viewport.Height) >= -64)
                {
                    Game1.panScreen(0, 8);
                }
                //if (!Game1.IsMultiplayer)
                //{
                //    Farm farm = Game1.getFarm();
                //    foreach (FarmAnimal value in farm.animals.Values)
                //    {
                //        value.MovePosition(Game1.currentGameTime, Game1.viewport, farm);
                //    }
                //}
            }
        }
    }
}
