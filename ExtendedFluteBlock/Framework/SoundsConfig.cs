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

        internal static readonly SoundFloorMap BuiltInSoundFloorPairs = new()
        {
#pragma warning disable format
            InGame(floor: NonFloor,         name: "flute",              cue: "flute",               rawPitch: 12),  // C6
            InGame(floor: CrystalFloor,     name: "crystal",            cue: "crystal",             rawPitch: 36),  // C8
            InGame(floor: StoneFloor,       name: "clam",               cue: "clam_tone",           rawPitch: 11),  // B5
            InGame(floor: StrawFloor,       name: "toy piano",          cue: "toyPiano",            rawPitch: 24),  // C7

            Custom(floor: WoodFloor,        name: "piano",              cue: "piano",               rawPitch: 0,    paths: "piano.wav"),
            Custom(floor: WeatheredFloor,   name: "acoustic guitar",    cue: "acoustic_guitar",     rawPitch: 0,    paths: "acoustic guitar.wav")
#pragma warning restore format
        };

        /// <summary>Sound-Floor pairs.</summary>
        public SoundFloorMap SoundFloorPairs { get; set; } = BuiltInSoundFloorPairs;

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