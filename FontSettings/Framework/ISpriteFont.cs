using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework
{
    internal interface ISpriteFont : IDisposable
    {
        bool IsDisposed { get; }

        void Draw(SpriteBatch b, string text, Vector2 position, Color color);

        void DrawBounds(SpriteBatch b, string text, Vector2 position, Color color);

        Vector2 MeasureString(string text);

        internal string LineBreak { get; }
    }
}
