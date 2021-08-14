using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Input.Gestures;
using Microsoft.Xna.Framework;

namespace JunimoStudio.Input
{
    /// <summary>
    /// Manages a list of <see cref="MouseGestureBase"/> to avoid conflicts.
    /// </summary>
    public class MouseGestureManager
    {
        public IList<MouseGestureBase> Gestures { get; }

        public MouseGestureManager()
        {
            Gestures = new List<MouseGestureBase>();
        }

        public void Update(GameTime gameTime)
        {
            foreach (MouseGestureBase ges in Gestures)
            {
                ges.Update(gameTime);
            }
        }
    }
}
