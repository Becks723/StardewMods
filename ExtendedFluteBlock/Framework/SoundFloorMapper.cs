using System;
using FluteBlockExtension.Framework.Models;
using StardewModdingAPI;
using StardewValley;

#nullable enable

namespace FluteBlockExtension.Framework
{
    internal class SoundFloorMapper
    {
        private readonly Func<SoundFloorMap> _map;
        private readonly IMonitor _monitor;

        private readonly SoundResolver _resolver = new();

        public SoundFloorMapper(Func<SoundFloorMap> map, IMonitor monitor)
        {
            this._map = map;
            this._monitor = monitor;
        }

        /// <summary>Map sound from floor data.</summary>
        public MappedSound Map(FloorData floor)
        {
            SoundData? sound = this.MapForSound(floor);
            return this._resolver.ResolveSoundData(sound);
        }

        /// <summary>Map sound data.</summary>
        public SoundData? MapForSound(FloorData floor)
        {
            return this._map().FindSound(floor);
        }
    }
}