using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    internal static class I18n
    {
        private static ITranslationHelper _translation;

        public static void Init(ITranslationHelper translation)
        {
            _translation = translation;
        }

        public static string TuningStick_Name => _translation.Get("tuning_stick_name");

        public static string TuningStick_Description => _translation.Get("tuning_stick_description");

        public static string KeyboardsName => _translation.Get("keyboards_name");

        public static string KeyboardsDescription => _translation.Get("keyboards_description");

        public static string GuitarName => _translation.Get("guitar_name");

        public static string GuitarDescription => _translation.Get("guitar_description");

        public static string StringsName => _translation.Get("strings_name");

        public static string StringsDescription => _translation.Get("strings_description");

        public static string PercussionName => _translation.Get("percussion_name");

        public static string PercussionDescription => _translation.Get("percussion_description");

        public static string WoodwindName => _translation.Get("woodwind_name");

        public static string WoodwindDescription => _translation.Get("woodwind_description");

        public static string BrassName => _translation.Get("brass_name");

        public static string BrassDescription => _translation.Get("brass_description");

        public static string NoteBlock_Name => _translation.Get("note_block_name");

        public static string NoteBlock_Description => _translation.Get("note_block_description");

    }
}
