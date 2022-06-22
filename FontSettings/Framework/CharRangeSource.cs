using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FontSettings.Framework
{
    internal static class CharRangeSource
    {
        private static readonly Dictionary<LanguageInfo, IEnumerable<CharacterRange>> _builtInCharRanges = new();

        public static void RecordBuiltInCharRange()
        {
            RecordBuiltInCharRange(FontHelpers.GetCurrentLanguage());
        }

        public static void RecordBuiltInCharRange(SpriteFont spriteFont)
        {
            RecordBuiltInCharRange(FontHelpers.GetCurrentLanguage(), spriteFont);
        }

        public static IEnumerable<CharacterRange> GetBuiltInCharRange()
        {
            return GetBuiltInCharRange(FontHelpers.GetCurrentLanguage());
        }

        public static void RecordBuiltInCharRange(LanguageInfo language)
        {
            if (!_builtInCharRanges.ContainsKey(language))
            {
                _builtInCharRanges[language] = InternalGetBuiltInCharRange(Game1.smallFont);
            }
        }

        public static void RecordBuiltInCharRange(LanguageInfo language, SpriteFont spriteFont)
        {
            if (!_builtInCharRanges.ContainsKey(language))
            {
                _builtInCharRanges[language] = InternalGetBuiltInCharRange(spriteFont);
            }
        }

        public static IEnumerable<CharacterRange> GetBuiltInCharRange(LanguageInfo language)
        {
            if (_builtInCharRanges.TryGetValue(language, out IEnumerable<CharacterRange> range))
                return range;

            throw new KeyNotFoundException();
        }

        private static IEnumerable<CharacterRange> InternalGetBuiltInCharRange(SpriteFont gameFont)
        {
            BindingFlags nonPublic = BindingFlags.Instance | BindingFlags.NonPublic;
            BindingFlags isPublic = BindingFlags.Instance | BindingFlags.Public;

            var regions = typeof(SpriteFont).GetField("_regions", nonPublic).GetValue(gameFont) as Array;
            FieldInfo startField = null;
            FieldInfo endField = null;

            for (int i = 0; i < regions.Length; i++)
            {
                object region = regions.GetValue(i);
                if (i == 0)
                {
                    startField = region.GetType().GetField("Start", isPublic);
                    endField = region.GetType().GetField("End", isPublic);
                }
                char start = (char)startField.GetValue(region);
                char end = (char)endField.GetValue(region);
                yield return new CharacterRange(start, end);
            }
        }
    }
}
