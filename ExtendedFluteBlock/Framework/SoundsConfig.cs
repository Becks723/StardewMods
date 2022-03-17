using System.IO;
using System.Linq;
using FluteBlockExtension.Framework.Models;
using static StardewModdingAPI.Constants;
using static FluteBlockExtension.Framework.Models.FloorData;

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
#pragma warning disable format
            InGame(floor: NonFloor,         name: "flute",              cue: "flute",               rawPitch: 12),
            InGame(floor: StoneFloor,       name: "crystal",            cue: "crystal",             rawPitch: 36),

            Custom(floor: WoodFloor,        name: "piano",              cue: "piano",               paths: "piano.wav"),
            Custom(floor: WeatheredFloor,   name: "acoustic guitar",    cue: "acoustic_guitar",     paths: "acoustic guitar.wav")
#pragma warning restore format
        };

        private static SoundFloorMapItem InGame(FloorData floor, string name, string cue, int rawPitch, string notes = null)
        {
            return new SoundFloorMapItem
            {
                Sound = SoundData.GameSound(name, cue, rawPitch, notes),
                Floor = floor
            };
        }

        private static SoundFloorMapItem Custom(FloorData floor, string name, string cue, int rawPitch = 0, string notes = null, params string[] paths)  // relative paths
        {
            return new SoundFloorMapItem
            {
                Sound = SoundData.CustomSound(name, cue, rawPitch, notes, paths.Select(p => FilePath.With(p)).ToArray()),
                Floor = floor
            };
        }
    }
}