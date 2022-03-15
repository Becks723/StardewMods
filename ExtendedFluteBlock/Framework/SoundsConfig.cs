using System.IO;
using FluteBlockExtension.Framework.Models;
using static StardewModdingAPI.Constants;

namespace FluteBlockExtension.Framework
{
    internal class SoundsConfig
    {
        internal static string DefaultSoundsFolderPath;

        /// <summary>Absolute path to sounds folder.</summary>
        public string SoundsFolderPath { get; set; } = DefaultSoundsFolderPath; /*= Path.Combine(DataPath, ".smapi", "mod-data", ModEntry.ModID, "sounds");*/

        /// <summary>Sound-Floor pairs.</summary>
        public SoundFloorMap SoundFloorPairs { get; set; } = new()
        {
            new() { Sound = SoundData.GameSound(name: "flute", cueName: "flute", rawPitch: 12), Floor = FloorData.NonFloor },
            new() { Sound = SoundData.CustomSound(name: "piano", cueName: "piano", paths: FilePath.With("piano.wav")), Floor = FloorData.WoodFloor },
            new() { Sound = SoundData.GameSound(name: "crystal", cueName: "crystal", rawPitch: 36), Floor = FloorData.StoneFloor },
        };
    }
}