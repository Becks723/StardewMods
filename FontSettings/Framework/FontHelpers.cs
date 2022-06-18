using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StardewValley;

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

        public static LanguageCode CurrentLanguageCode => ConvertLanguageCode(LocalizedContentManager.CurrentLanguageCode);

        public static LanguageCode ConvertLanguageCode(LocalizedContentManager.LanguageCode code)
        {
            return code switch
            {
                LocalizedContentManager.LanguageCode.en => LanguageCode.en,
                LocalizedContentManager.LanguageCode.ja => LanguageCode.ja,
                LocalizedContentManager.LanguageCode.ru => LanguageCode.ru,
                LocalizedContentManager.LanguageCode.zh => LanguageCode.zh,
                LocalizedContentManager.LanguageCode.pt => LanguageCode.pt,
                LocalizedContentManager.LanguageCode.es => LanguageCode.es,
                LocalizedContentManager.LanguageCode.de => LanguageCode.de,
                LocalizedContentManager.LanguageCode.th => LanguageCode.th,
                LocalizedContentManager.LanguageCode.fr => LanguageCode.fr,
                LocalizedContentManager.LanguageCode.ko => LanguageCode.ko,
                LocalizedContentManager.LanguageCode.it => LanguageCode.it,
                LocalizedContentManager.LanguageCode.tr => LanguageCode.tr,
                LocalizedContentManager.LanguageCode.hu => LanguageCode.hu,
                LocalizedContentManager.LanguageCode.mod => LanguageCode.mod,
                _ => throw new NotSupportedException()
            };
        }

        public static LocalizedContentManager.LanguageCode ConvertLanguageCode(LanguageCode code)
        {
            return code switch
            {
                LanguageCode.en => LocalizedContentManager.LanguageCode.en,
                LanguageCode.ja => LocalizedContentManager.LanguageCode.ja,
                LanguageCode.ru => LocalizedContentManager.LanguageCode.ru,
                LanguageCode.zh => LocalizedContentManager.LanguageCode.zh,
                LanguageCode.pt => LocalizedContentManager.LanguageCode.pt,
                LanguageCode.es => LocalizedContentManager.LanguageCode.es,
                LanguageCode.de => LocalizedContentManager.LanguageCode.de,
                LanguageCode.th => LocalizedContentManager.LanguageCode.th,
                LanguageCode.fr => LocalizedContentManager.LanguageCode.fr,
                LanguageCode.ko => LocalizedContentManager.LanguageCode.ko,
                LanguageCode.it => LocalizedContentManager.LanguageCode.it,
                LanguageCode.tr => LocalizedContentManager.LanguageCode.tr,
                LanguageCode.hu => LocalizedContentManager.LanguageCode.hu,
                LanguageCode.mod => LocalizedContentManager.LanguageCode.mod,
                _ => throw new NotSupportedException()
            };
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

        public static bool IsLatinLanguage(LanguageCode code)
        {
            return IsLatinLanguage(ConvertLanguageCode(code));
        }

        /// <returns>如果是mod语言，那么返回一个空字符串。</returns>
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
                case LocalizedContentManager.LanguageCode.mod: return string.Empty;
                default: throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">不带扩展名的完整路径。</param>
        public static void DeleteBmFont(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            string fakePath = path + ".txt";
            string dir = Path.GetDirectoryName(fakePath);
            string name = Path.GetFileNameWithoutExtension(fakePath);

            string fnt = Path.Combine(dir, name + ".fnt");
            string[] pngs = Directory.EnumerateFiles(dir, $"{name}_*.png", SearchOption.TopDirectoryOnly).ToArray();
            File.Delete(fnt);
            foreach (string png in pngs)
                File.Delete(png);
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
