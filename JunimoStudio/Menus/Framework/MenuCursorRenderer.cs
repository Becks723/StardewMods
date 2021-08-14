using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace JunimoStudio.Menus.Framework
{
    internal class MenuCursorRenderer : ICursorRenderer
    {
        private readonly Rectangle _move = new Rectangle(0, 0, 25, 25);

        private readonly Rectangle _size1 = new Rectangle(25, 0, 25, 25);

        private readonly Rectangle _size2 = new Rectangle(50, 0, 25, 25);

        private readonly Rectangle _size3 = new Rectangle(75, 0, 25, 25);

        private readonly Rectangle _size4 = new Rectangle(100, 0, 25, 25);

        private readonly Texture2D _curExpandedSheet;

        public Cursors Current { get; set; }

        public bool IgnoreTransparency { get; set; }

        public MenuCursorRenderer(Texture2D curExpandedSheet)
        {
            this._curExpandedSheet = curExpandedSheet;
            this.Current = Cursors.Arrow;
            this.IgnoreTransparency = false;
        }

        public void DrawCursor(SpriteBatch b)
        {
            bool gamepadMode = (Game1.options.snappyMenus && Game1.options.gamepadControls);
            if (this.Current == Cursors.Arrow && gamepadMode)
                this.Current = Cursors.Gamepad_Arrow;
            if (this.Current == Cursors.Gamepad_Arrow && !gamepadMode)
                this.Current = Cursors.Arrow;
            this.DrawCursor(b, this.Current);
        }

        public void DrawCursor(SpriteBatch b, Cursors cur)
        {
            if (Game1.options.hardwareCursor)
                return;

            bool isExpand = this.IsExpand(cur);

            Rectangle sourceRect = !isExpand
                ? Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, (int)cur, 16, 16)
                : this.GetSourceRectForExpandCursors(cur);

            float transparency = this.IgnoreTransparency ? 1f : Game1.mouseCursorTransparency;
            float scale = this.HandleDisplayScale(cur);
            Vector2 origin = this.HandleVertexOffset(cur, sourceRect);

            // draw.
            b.Draw(
                !isExpand ? Game1.mouseCursors : this._curExpandedSheet,
                new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                sourceRect,
                Color.White * transparency,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                1f);
        }


        private bool IsExpand(Cursors cur)
        {
            return cur != Cursors.Arrow
                && cur != Cursors.Busy
                && cur != Cursors.Busy
                && cur != Cursors.Hand
                && cur != Cursors.Gift
                && cur != Cursors.Dialogue
                && cur != Cursors.Zoom
                && cur != Cursors.Grab
                && cur != Cursors.Heart
                && cur != Cursors.Gamepad_Arrow
                && cur != Cursors.Gamepad_A
                && cur != Cursors.Gamepad_X
                && cur != Cursors.Gamepad_B
                && cur != Cursors.Gamepad_Y;
        }

        private Rectangle GetSourceRectForExpandCursors(Cursors cur)
        {
            return cur switch {
                Cursors.Move => this._move,
                Cursors.Size1 => this._size1,
                Cursors.Size2 => this._size2,
                Cursors.Size3 => this._size3,
                Cursors.Size4 => this._size4,
                _ => Rectangle.Empty,
            };
        }

        /// <summary>
        /// Gets a proper scale to display on screen.
        /// </summary>
        /// <param name="cur"></param>
        /// <param name="sourceRect"></param>
        /// <returns></returns>
        private float HandleDisplayScale(Cursors cur)
        {
            if (!this.IsExpand(cur))
                return 4f;

            return cur switch {
                Cursors.Move => 48f / 25f,
                Cursors.Size1 => 48f / 25f,
                Cursors.Size2 => 48f / 25f,
                Cursors.Size3 => 48f / 25f,
                Cursors.Size4 => 48f / 25f,
                _ => 4f,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The origin in Sritebatch.Draw().</returns>
        private Vector2 HandleVertexOffset(Cursors cur, Rectangle sourceRect)
        {
            if (!this.IsExpand(cur))
                return Vector2.Zero;

            return new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f);
        }
    }
}
