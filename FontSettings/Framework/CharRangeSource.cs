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
        private static readonly Dictionary<LanguageCode, IEnumerable<CharacterRange>> _builtInCharRanges = new();

        public static void RecordBuiltInCharRange()
        {
            RecordBuiltInCharRange((LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode);
        }

        public static IEnumerable<CharacterRange> GetBuiltInCharRange()
        {
            LanguageCode code = (LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode;
            return GetBuiltInCharRange(code);
        }

        public static void RecordBuiltInCharRange(LanguageCode code)
        {
            if (!_builtInCharRanges.ContainsKey(code))
            {
                _builtInCharRanges[code] = InternalGetBuiltInCharRange(Game1.smallFont);
            }
        }

        public static IEnumerable<CharacterRange> GetBuiltInCharRange(LanguageCode code)
        {
            if (_builtInCharRanges.TryGetValue(code, out IEnumerable<CharacterRange> range))
                return range;
            else
            {
                range = InternalGetBuiltInCharRange(Game1.smallFont);
                _builtInCharRanges[code] = range;
                return range;
            }
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
