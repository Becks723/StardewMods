using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Controls;
using StardewValley.Menus;

namespace FluteBlockExtension.Framework.Menus
{
    internal class ClickableTextureAdapter : Element
    {
        private readonly ClickableTextureComponent _component;

        private readonly ClickablePositionWatcher _positionWatcher;

        public ClickableTextureAdapter(ClickableTextureComponent component)
        {
            this._component = component;

            this._positionWatcher = new(component);
            this._positionWatcher.PositionChanged += this.OnComponentPositionChanged;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this._positionWatcher.Update();
        }

        public override void Draw(SpriteBatch b)
        {
            this._component.draw(b);
        }

        protected override void OnPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        {
            base.OnPositionChanged(oldPosition, newPosition);

            this._component.bounds.Location = newPosition.ToPoint();
        }

        private void OnComponentPositionChanged(object sender, EventArgs e)
        {
            Vector2 position = this._component.bounds.Location.ToVector2();
            if (this.Parent is null)
                this.LocalPosition = position;
            else
                this.LocalPosition = position - this.Parent.Position;
        }

        private class ClickablePositionWatcher
        {
            private readonly ClickableComponent _component;
            public Vector2 _lastPosition;

            public event EventHandler PositionChanged;

            public ClickablePositionWatcher(ClickableComponent component)
            {
                this._component = component;
            }

            public void Update()
            {
                Vector2 position = this._component.bounds.Location.ToVector2();

                if (this._lastPosition != position)
                {
                    PositionChanged?.Invoke(this, EventArgs.Empty);
                }

                this._lastPosition = position;
            }
        }
    }
}