namespace FluteBlockExtension.Framework
{
    internal static class Constants
    {
        public const int MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE = -3900;  // A1

        public const int MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE = 4800;  // C9

        public const string FluteBlockName = "Flute Block";

        public const int FluteBlockSheetIndex = 464;

        public const int FluteBlockTrackIndex = 0x00000070;  // 112

        public static string FluteBlockModData_ExtraPitch;

        public static int ToMidiNote(int pitch)
        {
            return pitch / 100 + 60;
        }

        public static int FromMidiNote(int note)
        {
            return (note - 60) * 100;
        }
    }
}
