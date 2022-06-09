using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal static class CharsFileManager
    {
        private static readonly IDictionary<LanguageCode, string> _files = new Dictionary<LanguageCode, string>();

        private static string _dir;

        public static void Initialize(string directory)
        {
            _dir = directory;

            Directory.CreateDirectory(directory);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns>Absolute path to the chars text file.</returns>
        public static string Get(LanguageCode code)
        {
            if (_files.TryGetValue(code, out string path))
                return path;
            else
            {
                path = Path.Combine(_dir, $"{code}.txt");
                try
                {
                    if (!File.Exists(path))
                        FontHelpers.GenerateTextFile(path, CharRangeSource.GetBuiltInCharRange(code));
                    return path;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
