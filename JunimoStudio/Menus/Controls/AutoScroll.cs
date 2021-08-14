using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace JunimoStudio.Menus.Controls
{
    public class AutoScroll
    {
        private readonly IScrollable _scrollControl;

        private bool _scrolling = false;

        public Directions Direction { get; set; }

        public int Speed { get; set; }

        public AutoScroll(IScrollable scrollControl)
            : this(scrollControl, Directions.Down, 1)
        {
        }

        public AutoScroll(IScrollable scrollControl, Directions direction, int speed)
        {
            _scrollControl = scrollControl ?? throw new ArgumentNullException(nameof(scrollControl));
            Direction = direction;
            Speed = speed;
        }

        public void Start()
        {
            _scrolling = true;
        }

        public void Stop()
        {
            _scrolling = false;
        }

        public void Update(GameTime gameTime)
        {
            if (_scrolling)
            {
                Orientation o =
                    (Direction == Directions.Up || Direction == Directions.Down)
                    ? Orientation.Vertical
                    : Orientation.Horizontal;
                int delta =
                    (Direction == Directions.Down || Direction == Directions.Right)
                    ? Speed
                    : -Speed;
                _scrollControl.ScrollBy(delta, o);
            }
        }
    }
}
