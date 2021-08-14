using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace JunimoStudio.Menus.Controls.Shapes
{
    public class Rectangle : Shape
    {
        private Texture2D _pen;
        private Color _fill;
        private Vector2 _size;
        private Color _stroke;
        private float _strokeThickness;

        public Rectangle()
            : this(Game1.graphics.GraphicsDevice)
        {

        }

        public Rectangle(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            _size = new Vector2(1, 1);
            _fill = Color.Red;
            _stroke = Color.Black;
            _strokeThickness = 1;
        }

        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                ResetPen();
            }
        }

        public override int Width => (int)Size.X;

        public override int Height => (int)Size.Y;

        /// <summary>Gets or sets the interior color.</summary>
        public Color Fill
        {
            get => _fill;
            set
            {
                _fill = value;
                ResetPen();
            }
        }

        /// <summary>Gets or sets the outline color.</summary>
        public Color Stroke
        {
            get => _stroke;
            set
            {
                _stroke = value;
                ResetPen();
            }
        }

        /// <summary>Gets or sets the outline color.</summary>
        public float StrokeThickness
        {
            get => _strokeThickness;
            set
            {
                if (value >= 0)
                {
                    _strokeThickness = value;
                    ResetPen();
                }
            }
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(_pen, Bounds, Color.White);
        }

        private void ResetPen()
        {
            _pen = new Texture2D(_graphicsDevice, Width, Height);
            Color[] c = new Color[Width * Height];

            int intStrokeThickness = (int)Math.Round(_strokeThickness);
            for (int i = 0; i < c.Length; i++)
            {
                // draw upper border.
                if (i < Width * intStrokeThickness)
                    c[i] = _stroke;

                // draw bottom border.
                else if (i > c.Length - Width * intStrokeThickness)
                    c[i] = _stroke;

                // draw left and right border.
                // ...

                else
                    c[i] = Fill;
            }
            _pen.SetData(c);
        }
    }
}
