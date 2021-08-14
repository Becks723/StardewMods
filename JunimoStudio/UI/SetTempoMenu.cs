//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using StardewValley;
//using StardewValley.Menus;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace JunimoStudio.UI
//{
//    public class SetTempoMenu : IClickableMenu
//    {
//        private List<Element> elements = new List<Element>();
//        public ModConfig Config;

//        public SetTempoMenu(ModConfig config) : base(Game1.viewport.Width / 2 - 212, Game1.viewport.Height / 2 - 125, 425, 250, false)
//        {
//            Config = config;
//            Initialize();
//        }
//        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
//        {
//            base.gameWindowSizeChanged(oldBounds, newBounds);
//            Initialize();
//        }
//        private void Initialize()
//        {
//            elements.Clear();
//            elements.Add(new SmallScroll("", xPositionOnScreen + borderWidth, yPositionOnScreen + borderWidth, 10, 522, Config.musicTempo, val =>
//            {
//                Config.musicTempo = val;
//                Config.playerAbsoluteTempo = (4 * (float)val + 3) / 60f;
//                Config.multiplier = Config.playerAbsoluteTempo / 5;
//            }, null, false));
//        }
//        public override void receiveLeftClick(int x, int y, bool playSound = true)
//        {
//            for (int i = 0; i < elements.Count; i++)
//                elements[i].ReceiveLeftClick(x, y);
//        }
//        public override void leftClickHeld(int x, int y)
//        {
//            for (int i = 0; i < elements.Count; i++)
//                elements[i].LeftClickHeld(x, y);
//        }
//        public override void releaseLeftClick(int x, int y)
//        {
//            for (int i = 0; i < elements.Count; i++)
//                elements[i].LeftClickReleased(x, y);
//        }
//        public override void receiveScrollWheelAction(int direction)
//        {
//            for (int i = 0; i < elements.Count; i++)
//                elements[i].ReceiveScrollWheelAction(direction);
//        }
//        public override void performHoverAction(int x, int y)
//        {
//            for (int i = 0; i < elements.Count; i++)
//                elements[i].performHoverAction = elements[i].bounds.Contains(x, y) ? true : false;
//        }
//        public override void draw(SpriteBatch b)
//        {
//            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
//            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f, true);
//            for (int i = 0; i < elements.Count; i++)
//                elements[i].Draw(b);
//            SetSoundMenu.DrawMouse(b, elements, null);
//        }
//    }
//}
