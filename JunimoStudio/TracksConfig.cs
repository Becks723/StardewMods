using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    internal class TracksConfig
    {
        public IList<TrackInfo> Tracks { get; set; } = new List<TrackInfo>();

        public bool EnableGlobal { get; set; } = false;
    }
}
