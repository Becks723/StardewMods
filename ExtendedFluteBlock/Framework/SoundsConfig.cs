using System.IO;
using System.Linq;
using FluteBlockExtension.Framework.Models;
using static StardewModdingAPI.Constants;
using static FluteBlockExtension.Framework.Models.FloorData;
using System;

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
            InGame(floor: NonFloor,          name: I18n.Sound_Flute_Name,             cue: "flute",              rawPitch: 12, /*C6*/    description: I18n.Sound_Flute_Desc),  
            InGame(floor: CrystalFloor,      name: I18n.Sound_Crystal_Name,           cue: "crystal",            rawPitch: 36, /*C8*/    description: I18n.Sound_Crystal_Desc),  
            InGame(floor: StoneFloor,        name: I18n.Sound_Clam_Name,              cue: "clam_tone",          rawPitch: 11, /*B5*/    description: I18n.Sound_Clam_Desc),  
            InGame(floor: StrawFloor,        name: I18n.Sound_ToyPiano_Name,          cue: "toyPiano",           rawPitch: 24, /*C7*/    description: I18n.Sound_ToyPiano_Desc),  

            Custom(floor: WoodFloor,         name: I18n.Sound_Piano_Name,             cue: "piano",              rawPitch: 0,            description: I18n.Sound_Piano_Desc,             paths: "piano.wav"),
            Custom(floor: WeatheredFloor,    name: I18n.Sound_AcousticGuitar_Name,    cue: "acoustic_guitar",    rawPitch: 0,            description: I18n.Sound_AcousticGuitar_Desc,    paths: "acoustic guitar.wav")
#pragma warning restore format
        };

        /// <summary>Sound-Floor pairs.</summary>
        public SoundFloorMap SoundFloorPairs { get; set; } = BuiltInSoundFloorPairs;

        private static SoundFloorMapItem InGame(FloorData floor, Func<string> name, string cue, int rawPitch, Func<string> description = null)
        {
            return new SoundFloorMapItem
            {
                Sound = SoundData.GameSound(name, cue, rawPitch, description),
                Floor = floor
            };
        }

        private static SoundFloorMapItem Custom(FloorData floor, Func<string> name, string cue, int rawPitch = 0, Func<string> description = null, params string[] paths)  // relative paths
        {
            return new SoundFloorMapItem
            {
                Sound = SoundData.CustomSound(name, cue, rawPitch, description, paths.Select(p => FilePath.With(p)).ToArray()),
                Floor = floor
            };
        }
    }
}