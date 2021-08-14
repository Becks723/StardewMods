using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    internal class TrackInfo
    {
        public Rectangle TileBounds { get; set; }

        public bool Horizontal { get; set; }

        /// <summary>地点名称，见<see cref="GameLocation.NameOrUniqueName"/>。</summary>
        public string Location { get; set; }

        public TrackInfo(GameLocation location, Rectangle tileBounds, bool horizontal)
        {
            Location = location.NameOrUniqueName;
            TileBounds = tileBounds;
            Horizontal = horizontal;
        }

        public override string ToString()
        {
            return $"{nameof(Location)}: {Location} {nameof(TileBounds)}: {TileBounds} Direction: {(Horizontal ? "Horizonal" : "Vertical")}";
        }
    }
}
