using JunimoStudio.Menus.Controls;
using JunimoStudio.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using GuiLabs.Undo;
using JunimoStudio.Menus.Framework;
using StardewModdingAPI;
using JunimoStudio.Menus.Framework.ScrollViewers;

namespace JunimoStudio.Menus
{
    internal class PianoRollMenu : ClickableMenuBase
    {
        private readonly IMonitor _monitor;

        private readonly IChannelManager _channelManager;

        private readonly IChannel _channel;

        private readonly ModConfig _config;

        private readonly ITimeBasedObject _timeSettings;

        private readonly ActionManager _actionManager;

        private RootElement _root;

        /// <summary>钢琴卷帘主界面。</summary>
        private ScrollViewer _mainViewer;

        /// <summary>钢琴卷帘下方音符属性编辑窗口。</summary>
        private ScrollViewer _eventEditorViewer;

        /// <summary>钢琴卷帘左侧的钢琴，仅支持纵向滚动。</summary>
        private KeyboardViewer _keysViewer;

        /// <summary>钢琴卷帘上方的小节标号，仅支持横向滚动。</summary>
        private BarNumbersViewer _barNumsViewer;

        /// <summary>单位tick的长度。</summary>
        private readonly float _tickLength = 1;

        /// <summary>每个音符的高度。</summary>
        private readonly float _noteHeight = 20;

        /// <summary>钢琴卷帘左侧钢琴的宽度。</summary>
        private readonly float _keyboardWidth = 100;

        public PianoRollMenu(IMonitor monitor, string channelName, IChannelManager channelManager, ModConfig config, ITimeBasedObject timeSettings, ActionManager actionManager, ICursorRenderer cursorRenderer)
        {
            this._monitor = monitor;
            this._cursorRenderer = cursorRenderer;
            this._channelManager = channelManager;
            this._channel = channelManager.Channels[channelName];
            this._config = config;
            this._timeSettings = timeSettings;
            this._actionManager = actionManager ?? new ActionManager();

            this.ResetComponents();
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
            this._cursorRenderer.DrawCursor(b);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            this._mainViewer?.OnMouseWheel(direction);
            this._keysViewer?.OnMouseWheel(direction);
            this._barNumsViewer?.OnMouseWheel(direction);
            this._eventEditorViewer?.OnMouseWheel(direction);
        }

        protected override void ResetComponents()
        {
            this.xPositionOnScreen = Game1.viewport.Width / 2 - 400;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - 250;
            this.width = 800;
            this.height = 500;

            this._root = new RootElement();

            int mainHeight = (int)(this.height * 0.75);
            int eventEditorHeight = this.height - mainHeight;

            this._mainViewer = new ScrollViewer(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, mainHeight));
            this._mainViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            this._mainViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            var main = new PianoRollMainScrollContent(this._monitor, this._mainViewer, this._tickLength, this._noteHeight, this._keyboardWidth, this._config, this._timeSettings, this._channel.Notes, this._actionManager, this._cursorRenderer);
            this._mainViewer.Content = main;

            this._eventEditorViewer = new ScrollViewer(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + mainHeight, this.width, eventEditorHeight));
            this._eventEditorViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            this._eventEditorViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            this._eventEditorViewer.Content
                = new EventEditorScrollContent(this._eventEditorViewer, main, this._tickLength, this._keyboardWidth, this._config, this._timeSettings);
            this._mainViewer.BindHorizontalOffsetTo(this._eventEditorViewer);

            this._keysViewer = new KeyboardViewer(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, 100, mainHeight), this._noteHeight, this._keyboardWidth);
            this._keysViewer.BindVerticalOffsetTo(this._mainViewer);

            this._barNumsViewer = new BarNumbersViewer(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen - 33, this.width, 33), this._config, this._timeSettings, this._tickLength);
            this._barNumsViewer.BindHorizontalOffsetTo(this._mainViewer);

            this._root.AddChild(this._eventEditorViewer);
            this._root.AddChild(this._mainViewer);
            this._root.AddChild(this._keysViewer);
            this._root.AddChild(this._barNumsViewer);
        }
    }
}
