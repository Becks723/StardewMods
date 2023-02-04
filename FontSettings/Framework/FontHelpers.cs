using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BmFont;
using StardewValley;
using StardewValley.GameData;
using static System.Net.Mime.MediaTypeNames;

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

        public static float GetDefaultFontPixelZoom()
        {
            var code = LocalizedContentManager.CurrentLanguageCode;
            if (code != LocalizedContentManager.LanguageCode.mod)
                return GetDefaultFontPixelZoom(code);
            else
                return GetDefaultFontPixelZoom(LocalizedContentManager.CurrentModLanguage);
        }

        public static float GetDefaultFontPixelZoom(LocalizedContentManager.LanguageCode code)
        {
            return code switch
            {
                LocalizedContentManager.LanguageCode.ja => 1.75f,
                LocalizedContentManager.LanguageCode.ru => 3f,
                LocalizedContentManager.LanguageCode.zh => 1.5f,
                LocalizedContentManager.LanguageCode.th => 1.5f,
                LocalizedContentManager.LanguageCode.ko => 1.5f,
                not LocalizedContentManager.LanguageCode.mod => 3f,
                LocalizedContentManager.LanguageCode.mod => throw new NotSupportedException($"Not for mod language. Use the overload method instead.")
            };
        }

        public static float GetDefaultFontPixelZoom(ModLanguage modLanguage)
        {
            return modLanguage.FontPixelZoom;
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
                case LocalizedContentManager.LanguageCode.en: return string.Empty;
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

        public static LanguageInfo GetLanguage(LocalizedContentManager.LanguageCode code)
        {
            return new LanguageInfo(code, GetLocale(code));
        }

        public static LanguageInfo LanguageEn => GetLanguage(LocalizedContentManager.LanguageCode.en);
        public static LanguageInfo LanguageJa => GetLanguage(LocalizedContentManager.LanguageCode.ja);
        public static LanguageInfo LanguageRu => GetLanguage(LocalizedContentManager.LanguageCode.ru);
        public static LanguageInfo LanguageZh => GetLanguage(LocalizedContentManager.LanguageCode.zh);
        public static LanguageInfo LanguagePt => GetLanguage(LocalizedContentManager.LanguageCode.pt);
        public static LanguageInfo LanguageEs => GetLanguage(LocalizedContentManager.LanguageCode.es);
        public static LanguageInfo LanguageDe => GetLanguage(LocalizedContentManager.LanguageCode.de);
        public static LanguageInfo LanguageTh => GetLanguage(LocalizedContentManager.LanguageCode.th);
        public static LanguageInfo LanguageFr => GetLanguage(LocalizedContentManager.LanguageCode.fr);
        public static LanguageInfo LanguageKo => GetLanguage(LocalizedContentManager.LanguageCode.ko);
        public static LanguageInfo LanguageIt => GetLanguage(LocalizedContentManager.LanguageCode.it);
        public static LanguageInfo LanguageTr => GetLanguage(LocalizedContentManager.LanguageCode.tr);
        public static LanguageInfo LanguageHu => GetLanguage(LocalizedContentManager.LanguageCode.hu);

        public static string GetFontFileAssetName()  // under game current language context
        {
            return GetFontFileAssetName(
                code: LocalizedContentManager.CurrentLanguageCode,
                modLanguage: LocalizedContentManager.CurrentModLanguage);
        }

        public static string GetFontFileAssetName(LocalizedContentManager.LanguageCode code, ModLanguage? modLanguage = null)
        {
            return code switch
            {
                LocalizedContentManager.LanguageCode.ja => "Fonts/Japanese",
                LocalizedContentManager.LanguageCode.ru => "Fonts/Russian",
                LocalizedContentManager.LanguageCode.zh => "Fonts/Chinese",
                LocalizedContentManager.LanguageCode.th => "Fonts/Thai",
                LocalizedContentManager.LanguageCode.ko => "Fonts/Korean",
                LocalizedContentManager.LanguageCode.mod when !modLanguage.UseLatinFont => modLanguage.FontFile,
                _ => null
            };
        }

        public static XmlSource ParseFontFile(FontFile fontFile)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(FontFile));
            using var writer = new StringWriter();
            xmlSerializer.Serialize(writer, fontFile);

            string xml = writer.ToString();
            return new XmlSource(xml);
        }

        public static string LocalizeAssetName(string assetName)
        {
            var language = FontHelpers.GetCurrentLanguage();
            return LocalizeAssetName(assetName, language.Code, language.Locale);
        }

        public static string LocalizeAssetName(string assetName, LocalizedContentManager.LanguageCode code, string locale)
        {
            var enLanguage = FontHelpers.LanguageEn;
            if (enLanguage.Code == code
                && enLanguage.Locale == locale)
            {
                return assetName;
            }

            return $"{assetName}.{locale}";
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

        public static string WrapString(string? text, float constrain, ISpriteFont? font)
        {
            if (font == null)
                return string.Empty;

            string newLine = font.LineBreak;
            float measureString(string s) => font.MeasureString(s).X;
            return WrapString(text, constrain, measureString, newLine);
        }

        public static string WrapString(string? text, float constrain, Func<string?, float> measureString, string newLine)
        {
            // Copied from Game1.parseText
            if (text == null)
            {
                return "";
            }
            string line = string.Empty;
            string returnString = string.Empty;
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja
                || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh
                || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
            {
                string text2 = text;
                for (int i = 0; i < text2.Length; i++)
                {
                    char c = text2[i];
                    if (measureString(line + c.ToString()) > constrain || c.Equals(newLine))
                    {
                        returnString = returnString + line + newLine;
                        line = string.Empty;
                    }
                    if (!c.Equals(newLine))
                    {
                        line += c.ToString();
                    }
                }
                return returnString + line;
            }

            string[] array = text.Split(' ');
            foreach (string word in array)
            {
                try
                {
                    if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr && word.StartsWith("\n-"))
                    {
                        returnString = returnString + line + newLine;
                        line = string.Empty;
                    }
                    if (measureString(line + word) > constrain || word.Equals(newLine))
                    {
                        returnString = returnString + line + newLine;
                        line = string.Empty;
                    }
                    if (!word.Equals(newLine))
                    {
                        line = line + word + " ";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception measuring string: " + e);
                }
            }
            return returnString + line;

        }
    }
}
