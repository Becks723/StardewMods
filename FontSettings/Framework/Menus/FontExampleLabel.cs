using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValleyUI;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus
{
    internal class FontExampleLabel : Label
    {
        private readonly static UIPropertyInfo SpriteFontProperty
            = new UIPropertyInfo(nameof(Font), typeof(ISpriteFont), typeof(FontExampleLabel), null, affectsMeasure: true);
        public new ISpriteFont Font
        {
            get { return this.GetValue<ISpriteFont>(SpriteFontProperty); }
            set { this.SetValue(SpriteFontProperty, value); }
        }

        public bool ShowText { get; set; } = true;

        public bool ShowBounds { get; set; } = false;

        public Color BoundsColor { get; set; }

        protected override void DrawOverride(SpriteBatch b)
        {
            if (this.Font != null)
            {
                string text = this.Text ?? string.Empty;

                // 先画背景
                if (this.ShowBounds)
                    this.Font.DrawBounds(b, text, this.Position, this.BoundsColor);

                // 再画文字
                if (this.ShowText)
                    this.Font.Draw(b, text, this.Position, this.Forground);
            }
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            Vector2 measureSize = Vector2.Zero;
            var font = this.Font;

            if (font == null)
                return measureSize;

            string text = this.Text ?? string.Empty;
            measureSize = font.MeasureString(text);
            return measureSize;
        }
    }
}
