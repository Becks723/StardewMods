using FluteBlockExtension.Framework.Models;
using static FluteBlockExtension.Framework.Models.FloorData;

namespace FluteBlockExtension.Framework
{
    internal class SoundsConfig
    {
        /// <summary>Sound-Floor pairs.</summary>
        public SoundFloorMap SoundFloorPairs { get; set; } = new()
        {
            new() { Sound = SoundData.Flute, Floor = NonFloor }
        };
    }
}