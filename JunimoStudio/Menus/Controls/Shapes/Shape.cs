using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace JunimoStudio.Menus.Controls.Shapes
{
    public abstract class Shape : Element
    {
        protected GraphicsDevice _graphicsDevice;

        public Shape(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        }

        public Shape() 
            : this(Game1.graphics.GraphicsDevice)
        {

        }
    }
}
