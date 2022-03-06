namespace FluteBlockExtension.Framework
{
    internal static class Constants
    {
        /// <summary>Min in-game pitch after extended. A1.</summary>
        public const int MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE = -3900;

        /// <summary>Max in-game pitch after extended. C9.</summary>
        public const int MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE = 4800;

        public const string FluteBlockName = "Flute Block";

        public const int FluteBlockSheetIndex = 464;

        /// <summary>"flute" soundbank index in XACT.</summary>
        /// <remarks>Unpacked sounds on <see href="https://stardewvalleywiki.com/Modding:Audio#Sound">wiki</see>.</remarks>
        public const int FluteTrackIndex = 0x00000070;  // 112

        /// <summary>"extraPitch" key of a flute block, stored in <see cref="StardewValley.Item.modData"/>.</summary>
        /// <remarks>Init in ModEntry point.</remarks>
        public static string FluteBlockModData_ExtraPitch;

        /// <summary>In-game pitch -> midi note</summary>
        public static int ToMidiNote(int pitch)
        {
            return pitch / 100 + 60;
        }

        /// <summary>Midi note -> in-game pitch</summary>
        public static int FromMidiNote(int note)
        {
            return (note - 60) * 100;
        }
    }
}
