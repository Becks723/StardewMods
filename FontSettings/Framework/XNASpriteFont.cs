using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework
{
    internal class XNASpriteFont : ISpriteFont
    {
        private bool _disposed;

        public SpriteFont InnerFont { get; }

        public bool IsDisposed => this._disposed;

        public XNASpriteFont(SpriteFont innerFont)
        {
            this.InnerFont = innerFont ?? throw new ArgumentNullException(nameof(innerFont));
        }

        public void Draw(SpriteBatch b, string text, Vector2 position, Color color)
        {
            b.DrawString(this.InnerFont, text, position, color);
        }

        public Vector2 MeasureString(string text)
        {
            return this.InnerFont.MeasureString(text);
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this.InnerFont.Texture?.Dispose();
                this._disposed = true;
            }
        }
    }
}
