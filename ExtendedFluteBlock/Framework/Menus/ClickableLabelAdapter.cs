using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValleyUI.Controls;

namespace FluteBlockExtension.Framework.Menus
{
    internal class ClickableLabelAdapter : Element
    {
        private readonly ClickableComponent _component;

        private readonly ClickablePositionWatcher _positionWatcher;

        public SpriteFont Font { get; set; } = Game1.dialogueFont;

        public Color LabelColor { get; set; } = Game1.textColor;

        public float Scale { get; set; } = 1f;

        public ClickableLabelAdapter(ClickableComponent component)
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
            Utility.drawTextWithShadow(b, this._component.label ?? string.Empty, this.Font, this.Position, this.LabelColor, this.Scale);
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
