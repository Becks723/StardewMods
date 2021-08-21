using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace JunimoStudio.Menus
{
    internal abstract class ClickableMenuBase : IClickableMenu
    {
        /// <summary>绘制光标的。</summary>
        protected ICursorRenderer _cursorRenderer;

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.ResetComponents();
            this.GetChildMenu()?.gameWindowSizeChanged(oldBounds, newBounds);
        }

        protected abstract void ResetComponents();
    }
}
