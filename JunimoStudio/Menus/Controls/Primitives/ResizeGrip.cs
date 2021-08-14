using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JunimoStudio.Menus.Controls.Primitives
{
    public class ResizeGrip : Element
    {
        private Point _size;

        public override int Width => _size.X;

        public override int Height => _size.Y;

        public void SetWidth(int value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            _size.X = value;
        }

        public void SetHeight(int value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            _size.Y = value;
        }

        public override void Draw(SpriteBatch b)
        {

        }
    }
}
