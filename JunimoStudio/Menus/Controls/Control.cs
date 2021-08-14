using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JunimoStudio.Menus.Controls
{
    public abstract class Control : Element
    {
        public Vector2 Size { get; set; }

        public override int Width => (int)Size.X;

        public override int Height => (int)Size.Y;
    }
}
