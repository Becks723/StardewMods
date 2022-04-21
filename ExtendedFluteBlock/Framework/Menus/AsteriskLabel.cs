using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValleyUI.Controls;

namespace FluteBlockExtension.Framework.Menus
{
    /// <summary>A label with an element option. When option data is moditfied, a "*" will show up beside the label.</summary>
    internal class AsteriskLabel<TElement> : Label2
        where TElement : Element
    {
        private readonly TElement _option;

        public bool Edited { get; private set; } = false;

        public Func<TElement, bool> IsEditedObserver { get; set; }

        public AsteriskLabel(TElement option)
        {
            this._option = option;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.Edited = this.IsEditedObserver?.Invoke(this._option) == true;
        }

        public override void Draw(SpriteBatch b)
        {
            base.Draw(b);

            // edit char '*'
            if (this.Edited)
            {
                var font = this.Font ?? Game1.dialogueFont;
                var origin = new Vector2(this.Position.X + font.MeasureString(this.Text ?? string.Empty).X + font.Spacing, this.Position.Y);
                b.DrawString(this.Font, "*", origin, this.IdleTextColor, 0f, Vector2.Zero, this.NonBoldScale, SpriteEffects.None, 0f);
            }
        }
    }
}