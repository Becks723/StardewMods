using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus
{
    internal class FontExampleLabel : Label
    {
        public new ISpriteFont Font { get; set; }

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

        public override void Draw(SpriteBatch b)
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
