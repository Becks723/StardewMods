using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace JunimoStudio.Menus
{
    internal class TracksMenu2 : ClickableMenuBase
    {
        private readonly IList<TrackInfo> _tracks;

        private readonly string _locationName;

        private string _initalLocationName;

        private ClickableTextureComponent _cancelButton;

        public TracksMenu2(IList<TrackInfo> tracks, string locationName)
        {
            this._tracks = tracks;
            this._locationName = locationName;
            this.ResetComponents();
            this.Prepare();
        }

        public override void draw(SpriteBatch b)
        {
            string msg = "TestString";
            SpriteText.drawStringWithScrollBackground(b, msg, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(msg) / 2, 16);
            this._cancelButton.draw(b);
            this.drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this._cancelButton.containsPoint(x, y))
            {
                this.exitThisMenu(false);
                Game1.playSound("smallSelect");
                return;
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (Game1.IsFading())
                return;

            base.receiveKeyPress(key);

            // WASD也能移动视窗。
            if (!Game1.options.SnappyMenus)
            {
                if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
                {
                    Game1.panScreen(0, 8);
                }
                else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                {
                    Game1.panScreen(8, 0);
                }
                else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
                {
                    Game1.panScreen(0, -8);
                }
                else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
                {
                    Game1.panScreen(-8, 0);
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            this._cancelButton.tryHover(x, y);
            base.performHoverAction(x, y);
        }

        public override void update(GameTime time)
        {
            if (!Game1.IsFading())
            {
                // 鼠标移动视窗（鼠标处在屏幕边缘自动移动视窗）。
                int mouseX = Game1.getOldMouseX(false) + Game1.viewport.X;
                int mouseY = Game1.getOldMouseY(false) + Game1.viewport.Y;

                if (mouseX - Game1.viewport.X < 64)
                {
                    Game1.panScreen(-8, 0);
                }
                else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -64)
                {
                    Game1.panScreen(8, 0);
                }

                if (mouseY - Game1.viewport.Y < 64)
                {
                    Game1.panScreen(0, -8);
                }
                else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
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

        public override bool readyToClose()
        {
            return (Game1.locationRequest == null);
        }

        protected override void cleanupBeforeExit()
        {
            this.OnClose();
        }

        private void Prepare()
        {
            this._initalLocationName = Game1.currentLocation.NameOrUniqueName;
            Game1.currentLocation.cleanupBeforePlayerExit();
            Game1.currentLocation = Game1.getLocationFromName(this._locationName);
            Game1.player.viewingLocation.Value = this._locationName;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear();
            Game1.displayHUD = false;
            Game1.viewportFreeze = true;
            Game1.panScreen(0, 0);
            Game1.displayFarmer = false;
            this._cancelButton.bounds.X = Game1.uiViewport.Width - 128;
            this._cancelButton.bounds.Y = Game1.uiViewport.Height - 128;
        }

        private void OnClose()
        {
            LocationRequest request = Game1.getLocationRequest(this._initalLocationName);
            request.OnWarp += () =>
            {
                Game1.player.viewingLocation.Value = null;
                Game1.displayHUD = true;
                Game1.viewportFreeze = false;
                Game1.displayFarmer = true;
                if (Game1.options.SnappyMenus)
                {
                    this.populateClickableComponentList();
                    this.snapToDefaultClickableComponent();
                }
            };

            Game1.warpFarmer(request,
                Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
        }

        protected override void ResetComponents()
        {
            this._cancelButton = 
                new ClickableTextureComponent(
                name: "OK",
                bounds: new Rectangle(Game1.uiViewport.Width - 128, Game1.uiViewport.Height - 128, 64, 64),
                label: null,
                hoverText: null,
                texture: Game1.mouseCursors,
                sourceRect: Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47),
                scale: 1f);
        }
    }
}
