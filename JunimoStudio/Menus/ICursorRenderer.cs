using Microsoft.Xna.Framework.Graphics;

namespace JunimoStudio.Menus
{
    /// <summary>绘制光标的。</summary>
    internal interface ICursorRenderer
    {
        /// <summary>当前光标。</summary>
        Cursors Current { get; set; }

        /// <summary>绘制当前光标。</summary>
        /// <param name="b"></param>
        void DrawCursor(SpriteBatch b);

        /// <summary>绘制指定光标。</summary>
        /// <param name="b"></param>
        /// <param name="cur">指定光标。</param>
        void DrawCursor(SpriteBatch b, Cursors cur);
    }
}
