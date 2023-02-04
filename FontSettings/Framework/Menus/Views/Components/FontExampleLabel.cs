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

namespace FontSettings.Framework.Menus.Views.Components
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

        public Color BoundsColor { get; set; } = Color.Transparent;

        protected override void DrawOverride(SpriteBatch b)
        {
            if (this.Font != null)
            {
                string text = this.TextForDraw;

                // 先画背景
                if (this.ShowBounds)
                    this.Font.DrawBounds(b, text, this.Position, this.BoundsColor);

                // 再画文字
                if (this.ShowText)
                    this.Font.Draw(b, text, this.Position, this.Forground);
            }
        }

        protected override Vector2 MeasureString(string s)
        {
            var font = this.Font;

            if (font == null)
                return Vector2.Zero;

            return font.MeasureString(s ?? string.Empty);
        }

        protected override string WrapString(string text, float constrain, Func<string, float> measureString)
        {
            return FontHelpers.WrapString(text, constrain, this.Font);
        }
    }
}
