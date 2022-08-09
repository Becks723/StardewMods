using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.GameData;

namespace FontSettings.Framework
{
    internal static class FontHelpers
    {
        public static IEnumerable<CharacterRange> GetCharRange(string? s, char? defaultCharacter = null)
        {
            if (s is null || s.Length is 0)
            {
                if (defaultCharacter is null)
                    return Enumerable.Empty<CharacterRange>();
                else
                    return new[] { new CharacterRange() { Start = defaultCharacter.Value, End = defaultCharacter.Value } };
            }

            char[] chars = defaultCharacter != null
                ? s.ToCharArray().Append(defaultCharacter.Value).Distinct().ToArray()
                : s.ToCharArray().Distinct().ToArray();
            Array.Sort(chars);
            List<CharacterRange> result = new();
            char start = chars[0];
            for (int i = 1; i < chars.Length; i++)
            {
                char last = chars[i - 1];
                char current = chars[i];
                if (i < chars.Length - 1)
                {
                    if (last + 1 == current)
                        continue;
                    else
                    {
                        result.Add(new CharacterRange(start, last));
                        start = current;
                    }
                }
                else  // 最后一个
                {
                    if (last + 1 == current)
                        result.Add(new CharacterRange(start, current));
                    else
                    {
                        result.Add(new CharacterRange(last, last));
                        result.Add(new CharacterRange(current, current));
                    }
                }
            }
            return result;
        }

        public static bool IsLatinLanguage(LanguageInfo language)
        {
            return language.Code is LocalizedContentManager.LanguageCode.en
                or LocalizedContentManager.LanguageCode.pt
                or LocalizedContentManager.LanguageCode.es
                or LocalizedContentManager.LanguageCode.de
                or LocalizedContentManager.LanguageCode.fr
                or LocalizedContentManager.LanguageCode.it
                or LocalizedContentManager.LanguageCode.tr
                or LocalizedContentManager.LanguageCode.hu;
        }

        public static bool IsLatinLanguage(LocalizedContentManager.LanguageCode code)
        {
            return code is LocalizedContentManager.LanguageCode.en
                or LocalizedContentManager.LanguageCode.pt
                or LocalizedContentManager.LanguageCode.es
                or LocalizedContentManager.LanguageCode.de
                or LocalizedContentManager.LanguageCode.fr
                or LocalizedContentManager.LanguageCode.it
                or LocalizedContentManager.LanguageCode.tr
                or LocalizedContentManager.LanguageCode.hu;
        }

        public static LanguageInfo GetCurrentLanguage()
        {
            return new LanguageInfo(LocalizedContentManager.CurrentLanguageCode, GetCurrentLocale());
        }

        public static string GetCurrentLocale()
        {
            if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.mod)
                return FontHelpers.GetLocale(LocalizedContentManager.CurrentLanguageCode);
            else
                return FontHelpers.GetModLocale(LocalizedContentManager.CurrentModLanguage);
        }

        /// <returns>如果是mod语言，报错。</returns>
        public static string GetLocale(LocalizedContentManager.LanguageCode languageCode)
        {
            switch (languageCode)
            {
                case LocalizedContentManager.LanguageCode.en: return "en";
                case LocalizedContentManager.LanguageCode.ja: return "ja-JP";
                case LocalizedContentManager.LanguageCode.ru: return "ru-RU";
                case LocalizedContentManager.LanguageCode.zh: return "zh-CN";
                case LocalizedContentManager.LanguageCode.pt: return "pt-BR";
                case LocalizedContentManager.LanguageCode.es: return "es-ES";
                case LocalizedContentManager.LanguageCode.de: return "de-DE";
                case LocalizedContentManager.LanguageCode.th: return "th-TH";
                case LocalizedContentManager.LanguageCode.fr: return "fr-FR";
                case LocalizedContentManager.LanguageCode.ko: return "ko-KR";
                case LocalizedContentManager.LanguageCode.it: return "it-IT";
                case LocalizedContentManager.LanguageCode.tr: return "tr-TR";
                case LocalizedContentManager.LanguageCode.hu: return "hu-HU";
                default: throw new NotSupportedException();
            }
        }

        public static string GetModLocale(ModLanguage modLanguage)
        {
            return modLanguage?.LanguageCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Absolute path to the text file generated.</param>
        /// <param name="charRanges"></param>
        public static void GenerateTextFile(string path, IEnumerable<CharacterRange> charRanges)
        {
            using FileStream stream = File.Create(path);
            using StreamWriter sw = new StreamWriter(stream) { AutoFlush = true };
            int lineChars = 0;
            foreach (CharacterRange range in charRanges)
            {
                for (int c = range.Start; c <= range.End; c++)
                {
                    sw.Write((char)c);
                    lineChars++;
                    if (lineChars >= 100)
                    {
                        sw.Write('\n');
                        lineChars = 0;
                    }
                }
            }
        }
    }
}
