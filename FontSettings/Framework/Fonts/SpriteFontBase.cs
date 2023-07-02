using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.Fonts
{
    internal abstract class SpriteFontBase : ISpriteFont
    {
        private bool _isDisposed;

        public virtual bool IsDisposed => this._isDisposed;

        protected virtual bool NeedsDispose { get; } = false;

        string ISpriteFont.LineBreak => this.LineBreak;

        protected virtual string LineBreak => Environment.NewLine;

        public abstract void Draw(SpriteBatch b, string text, Vector2 position, Color color);

        public abstract void DrawBounds(SpriteBatch b, string text, Vector2 position, Color color);

        public abstract Vector2 MeasureString(string text);

        protected virtual void Dispose(bool disposing)
        {
            if (!this._isDisposed)
                this._isDisposed = true;
        }

        public void Dispose()
        {
            if (this.NeedsDispose)
            {
                this.Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
