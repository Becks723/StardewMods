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

        public override int Width
        {
            get
            {
                if (this.Font == null)
                    return 0;
                string text = this.Text ?? string.Empty;
                return (int)this.Font.MeasureString(text).X;
            }
        }

        public override int Height
        {
            get
            {
                if (this.Font == null)
                    return 0;
                string text = this.Text ?? string.Empty;
                return (int)this.Font.MeasureString(text).Y;
            }
        }

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
    }
}
