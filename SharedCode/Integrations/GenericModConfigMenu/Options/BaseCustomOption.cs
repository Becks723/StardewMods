using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CodeShared.Integrations.GenericModConfigMenu.Options
{
    internal abstract class BaseCustomOption
    {
        public abstract int Height { get; }

        public abstract void Draw(SpriteBatch b, Vector2 drawOrigin);

        public virtual void OnMenuOpening() { }

        public virtual void OnMenuClosing() { }

        public virtual void OnSaving() { }

        public virtual void OnSaved() { }

        public virtual void OnReseting() { }

        public virtual void OnReset() { }
    }
}