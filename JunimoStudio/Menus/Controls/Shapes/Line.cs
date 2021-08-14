using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace JunimoStudio.Menus.Controls.Shapes
{
    internal class Line : Shape
    {
        private Texture2D _pen;

        /// <summary>Gets or sets whether the <see cref="Line"/> instance is horizontal or vertical.</summary>
        /// <remarks><see langword="true"/> for horizontal and <see langword="false"/> for vertical.</remarks>
        public bool Horizontal { get; set; }

        public int Length { get; set; }

        public int Thickness { get; set; }

        public Color Color { get; set; }

        public override int Width => Horizontal ? Length : Thickness;

        public override int Height => Horizontal ? Thickness : Length;

        public Line()
        {
            Horizontal = true;
            Thickness = 1;
            Color = Color.Black;
            _pen = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _pen.SetData(new Color[] { Color });
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(_pen, Bounds, Color.White);
        }
    }
}
