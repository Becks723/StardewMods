using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework
{
    internal abstract class BitmapSpriteFont : ISpriteFont
    {
        private bool _disposed;

        public virtual FontFile FontFile { get; init; }

        public List<Texture2D> Pages { get; init; } = new();

        public bool IsDisposed => this._disposed;

        public void Dispose()
        {
            if (!this._disposed)
            {
                foreach (Texture2D texture in this.Pages)
                {
                    texture.Dispose();
                }
                this.Pages.Clear();

                this._disposed = true;
            }
        }

        public abstract void Draw(SpriteBatch b, string text, Vector2 position, Color color);

        public abstract Vector2 MeasureString(string text);
    }
}
