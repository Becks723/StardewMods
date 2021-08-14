using System;
using JunimoStudio.Menus.Controls;
using JunimoStudio.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using JunimoStudio.Menus.Framework;
using JunimoStudio.Menus.Framework.ScrollViewers;
using StardewModdingAPI;
using GuiLabs.Undo;

namespace JunimoStudio.Menus
{
    internal class ChannelRackMenu : ClickableMenuBase
    {
        private readonly IMonitor _monitor;

        private readonly IChannelManager _channelManager;

        private readonly ModConfig _config;

        private readonly ITimeBasedObject _timeSettings;

        private readonly ActionManager _actionManager;

        private RootElement _root;

        /// <summary></summary>
        private ChannelRackViewer _viewer;

        public ChannelRackMenu(IMonitor monitor, IChannelManager channelManager, ModConfig config, ITimeBasedObject timeSettings, ActionManager actionManager, ICursorRenderer cursorRenderer)
        {
            this._monitor = monitor;
            this._channelManager = channelManager;
            this._config = config;
            this._timeSettings = timeSettings;
            this._actionManager = actionManager;
            this._cursorRenderer = cursorRenderer;

            this.ResetComponents();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            this._viewer.OnMouseWheel(direction);
        }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);
            this._root.Update(gameTime);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            this._root.Draw(b);
            this.drawMouse(b);
        }

        protected override void ResetComponents()
        {
            this.xPositionOnScreen = Game1.viewport.Width / 2 - 400;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - 250;
            this.width = 800;
            this.height = 500;

            this._root = new RootElement();

            Action<string> openPianoRoll = (channelName) =>
            {
                Game1.activeClickableMenu = new PianoRollMenu(this._monitor, channelName, this._channelManager, this._config, this._timeSettings, this._actionManager, this._cursorRenderer);
            };
            this._viewer = new ChannelRackViewer(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height), this._channelManager, openPianoRoll);
            this._root.AddChild(this._viewer);
        }
    }
}
