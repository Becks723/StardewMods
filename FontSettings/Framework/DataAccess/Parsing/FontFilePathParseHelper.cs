using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.DataAccess.Parsing
{
    internal class FontFilePathParseHelper
    {
        public string? ParseFontFilePath(string? path, IEnumerable<string> fontFiles)
        {
            if (path == null)
                return null;

            // 找完整路径。
            string mapped = (from file in fontFiles
                             where this.IsSubpath(file, path)
                             select file).FirstOrDefault();
            // 没找到，原封不动返回。
            if (mapped == null)
                return path;

            return mapped;
        }

        public string? ParseBackFontFilePath(string? path, IEnumerable<string> fontFiles)
        {
            if (path == null)
                return null;

            return
                Path.GetFileName(path);
        }

        public string? ParseFontFilePath(string? path, IEnumerable<string> fontFiles,
            IVanillaFontConfigProvider vanillaFontConfigProvider, LanguageInfo language, GameFontType fontType)
        {
            if (path == null)
                return vanillaFontConfigProvider.GetVanillaFontConfig(language, fontType).FontFilePath;

            return this.ParseFontFilePath(path, fontFiles);
        }

        public string? ParseBackFontFilePath(string? path, IEnumerable<string> fontFiles,
            IVanillaFontConfigProvider vanillaFontConfigProvider, LanguageInfo language, GameFontType fontType)
        {
            if (path == vanillaFontConfigProvider.GetVanillaFontConfig(language, fontType).FontFilePath)
                return null;

            return this.ParseBackFontFilePath(path, fontFiles);
        }

        // TODO: 优化
        private bool IsSubpath(string basePath, string path)
        {
            string safeBasePath = StardewModdingAPI.Utilities.PathUtilities.NormalizePath(basePath);
            string safePath = StardewModdingAPI.Utilities.PathUtilities.NormalizePath(path);

            return safeBasePath.EndsWith(safePath);
        }
    }
}
