using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SConstants = StardewModdingAPI.Constants;
using SGamePlatform = StardewModdingAPI.GamePlatform;

namespace FontSettings.Framework.FontInfomation
{
    internal class InstalledFonts
    {
        private static IEnumerable<FontModel> _allFonts;

        private static readonly IFontInfoSource _fontSource = new FontInfoFromOtherLibs();

        private static string[] SystemFontsFolders => NormalizeSystemFolders(GetSystemFontFolders());

        public static IEnumerable<FontModel> GetAll()
        {
            if (_allFonts is null)
                Rescan();
            return _allFonts;
        }

        public static void Rescan()
        {
            _allFonts = ScanInstalledFonts();
        }

        public static string SimplifyPath(string fullPath)
        {
            foreach (string dir in SystemFontsFolders)
            {
                string relativePath = Path.GetRelativePath(dir, fullPath);
                if (relativePath != fullPath && fullPath.EndsWith(relativePath))
                    return relativePath;
            }

            return fullPath;
        }

        public static string GetFullPath(string simplifiedPath)
        {
            if (TryGetFullPath(simplifiedPath, out string fullPath))
                return fullPath;

            throw new FileNotFoundException();
        }

        public static bool TryGetFullPath(string simplifiedPath, out string fullPath)
        {
            if (simplifiedPath is null)
            {
                fullPath = null;
                return false;
            }

            foreach (string dir in SystemFontsFolders)
            {
                fullPath = Path.GetFullPath(simplifiedPath, dir);
                if (File.Exists(fullPath))
                    return true;
                else
                    fullPath = null;
            }

            // if input is already full path.
            if (File.Exists(simplifiedPath))
            {
                fullPath = simplifiedPath;
                return true;
            }

            fullPath = null;
            return false;
        }

        private static IEnumerable<FontModel> ScanInstalledFonts()
        {
            string[] searchDirs = SystemFontsFolders;

            string[] extensions = { ".ttf", ".ttc", ".otf" };

            var files = from dir in searchDirs
                        from file in Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly)  // 需排除windows下localappdata中的Deleted文件夹
                        where extensions.Any(file.ToLowerInvariant().EndsWith)
                        select file;

            List<FontModel> result = new();
            foreach (string path in files)
                try
                {
                    FontModel[] fonts = _fontSource.GetFontInfo(path);
                    result.AddRange(fonts);
                }
                catch (Exception ex)
                {
                    ILog.Warn($"无法加载字体。路径：{path}");
                    ILog.Trace($"{ex.Message}\n堆栈信息：\n{ex.StackTrace}");
                }

            return result;
        }

        // https://github.com/SixLabors/Fonts/blob/main/src/SixLabors.Fonts/SystemFontCollection.cs#L29
        private static string[] GetSystemFontFolders()
        {
            switch (SConstants.TargetPlatform)
            {
                case SGamePlatform.Windows:
                    return new[]
                    {
                        @"%SYSTEMROOT%\Fonts",
                        @"%APPDATA%\Microsoft\Windows\Fonts",
                        @"%LOCALAPPDATA%\Microsoft\Windows\Fonts"
                    };
                case SGamePlatform.Linux:
                    return new[]
                    {
                        "%HOME%/.fonts/",
                        "/usr/local/share/fonts",
                        "/usr/share/fonts"
                    };
                case SGamePlatform.Mac:
                    // As documented on "Mac OS X: Font locations and their purposes"
                    // https://web.archive.org/web/20191015122508/https://support.apple.com/en-us/HT201722
                    return new[]
                    {
                        "%HOME%/Library/Fonts/",
                        "/Library/Fonts/",
                        "/System/Library/Fonts/",
                        "/Network/Library/Fonts/"
                    };
                case SGamePlatform.Android:
                    return Array.Empty<string>();   // TODO
                default:
                    throw new PlatformNotSupportedException();
            }
        }

        private static string[] NormalizeSystemFolders(string[] raw)
        {
            return raw.Select(path => Environment.ExpandEnvironmentVariables(path))
                .Where(dir => Directory.Exists(dir))
                .ToArray();
        }

        private static FontStyle ParseStyle(SixLabors.Fonts.FontStyle style)
        {
            return style switch
            {
                SixLabors.Fonts.FontStyle.Regular => FontStyle.Regular,
                SixLabors.Fonts.FontStyle.Bold => FontStyle.Bold,
                SixLabors.Fonts.FontStyle.Italic => FontStyle.Italic,
                SixLabors.Fonts.FontStyle.BoldItalic => FontStyle.Bold | FontStyle.Italic,
                _ => FontStyle.Regular
            };
        }
    }
}
