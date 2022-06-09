using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StbTrueTypeSharp.StbTrueType;
using SConstants = StardewModdingAPI.Constants;
using SGamePlatform = StardewModdingAPI.GamePlatform;
using static StardewModdingAPI.Utilities.PathUtilities;

namespace FontSettings.Framework
{
    internal partial class BmFontGenerator
    {
        private static readonly string _fontbmEntry = SConstants.TargetPlatform switch
        {
            SGamePlatform.Android => string.Empty,
            SGamePlatform.Linux => NormalizePath(@"linux\fontbm"),
            SGamePlatform.Mac => string.Empty,
            SGamePlatform.Windows => NormalizePath(@"win\fontbm.exe"),
            _ => string.Empty
        };

        /// <param name="outputDir">相对于<paramref name="baseDir"/>的输出文件夹。</param>
        /// <param name="outputName">输出文件（.fnt）的名称，不带扩展名。如abc表示输出abc.fnt、abc_0.png……</param>
        /// <param name="outputPath">输出文件的完整路径，唯独不带扩展名。</param>
        /// <param name="fontSize">字体大小，单位为像素（px）。</param>
        /// <param name="charsFiles">一或多个包含所需字符的文本文件（相对于<paramref name="baseDir"/>）的路径。</param>
        private static void InternalGenerateFile(string fontFilePath, string baseDir, string outputDir, string outputName, out string outputPath,
            int fontIndex, int fontSize, IEnumerable<CharacterRange> charRanges, string[] charsFiles,
            int paddingUp, int paddingRight, int paddingDown, int paddingLeft,
            int spacingHoriz, int spacingVert)
        {
            const string defaultFormat = "xml";
            const string defaultKerning = "disabled";

            fontSize = (int)(fontSize * 4 / 3.0);  // fontbm使用的是pt，因此需转换。1px = 1pt * dpi / 72（这里dpi看作96）
            outputPath = Path.Combine(baseDir, outputDir, outputName);
            charRanges = CheckCharRanges(fontFilePath, charRanges);  // 检查字符集，排除未定义字符。

            string[] options =
            {
                $"--font-file \"{fontFilePath}\"",
                $"--output \"{Path.Combine(outputDir, outputName)}\"",
                $"--font-size {fontSize}",
                $"--chars {FormatCharRanges(charRanges)}",
                $"--chars-file {string.Join(' ', charsFiles.Select(file => $"\"{file}\""))}",
                $"--data-format {defaultFormat}",
                /*$"--kerning-pairs {defaultKerning}",*/
                /*$"--texture-size 1024x1024",*/
                $"--padding-up {paddingUp}",
                $"--padding-left {paddingLeft}",
                $"--padding-down {paddingDown}",
                $"--padding-right {paddingRight}",
                $"--spacing-horiz {spacingHoriz}",
                $"--spacing-vert {spacingVert}",
            };

            string args = string.Join(' ', options);

            if (string.IsNullOrWhiteSpace(_fontbmEntry))
                throw new PlatformNotSupportedException($"fontbm不支持当前操作系统：{SConstants.TargetPlatform}，如需帮助，请联系作者。");
            string fontbm = Path.Combine(baseDir, "fontbm", _fontbmEntry);
            CreateBmFontFiles(fontbm, args);
        }

        private static readonly Stopwatch _stopwatch = new Stopwatch();
        private static void CreateBmFontFiles(string fontbmPath, string args)
        {
            if (!File.Exists(fontbmPath))
                throw new FileNotFoundException("未找到fontbm，可能缺失文件，请重新下载该mod。", fontbmPath);

            ILog.Trace($"开始生成位图字体……");
            _stopwatch.Restart();
            Process process = new Process()
            {
                StartInfo = new()
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.StandardInput.AutoFlush = true;

            string targetDir = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory,
                Path.GetDirectoryName(fontbmPath));
            targetDir = StardewModdingAPI.Utilities.PathUtilities.NormalizePath(targetDir);
            process.StandardInput.WriteLine($"cd {targetDir}");
            process.StandardInput.WriteLine($"fontbm {args}");
            process.StandardInput.Close();  // 需要这一行，否则主程序会卡死，实际上是控制台一直在等待用户输入。
            process.WaitForExit();
            process.Close();

            _stopwatch.Stop();
            ILog.Trace($"已生成位图字体。耗时：{_stopwatch.Elapsed.Minutes}分{_stopwatch.Elapsed.Seconds}秒");

            //Process process = new Process()
            //{
            //    StartInfo = new()
            //    {
            //        FileName = fontbmPath,
            //        Arguments = $"fontbm {args}",
            //        UseShellExecute = false,
            //        //CreateNoWindow = true
            //    }
            //};
            //process.Start();
            //process.WaitForExit();
            //process.Close();
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            //throw new Exception(e.Data);
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
        }

        private static string FormatCharRanges(IEnumerable<CharacterRange> charRanges)
        {
            StringBuilder sb = new();
            sb.AppendJoin(',', charRanges.Select(range =>
            {
                if (range.Start != range.End)
                    return $"{(int)range.Start}-{(int)range.End}";
                else
                    return ((int)range.Start).ToString();
            }));
            return sb.ToString();
        }

        private static IEnumerable<CharacterRange> CheckCharRanges(string fontFilePath,
            IEnumerable<CharacterRange> charRanges)
        {
            stbtt_fontinfo fontInfo = new stbtt_fontinfo();
            byte[] ttf = File.ReadAllBytes(fontFilePath);
            unsafe
            {
                fixed (byte* ptr = ttf)
                    if (stbtt_InitFont(fontInfo, ptr, stbtt_GetFontOffsetForIndex(ptr, 0)) == 0)
                        throw new Exception("初始化字体失败。");
            }

            return CheckCharRanges(fontInfo, charRanges);
        }

        private static IEnumerable<CharacterRange> CheckCharRanges(stbtt_fontinfo fontinfo,
            IEnumerable<CharacterRange> charRanges)
        {
            return charRanges
                .Select(range => CheckCharRange(fontinfo, range))
                .SelectMany(ranges => ranges);
        }

        // 检查是否有字符没有在给定字体中定义。(fontbm版本0.5.0中未定义的字符会报错)
        private static IEnumerable<CharacterRange> CheckCharRange(stbtt_fontinfo fontinfo, CharacterRange range)
        {
            CharacterRange tempRange = default;
            bool inRange = false;
            for (int code = range.Start; code <= range.End; code++)
            {
                if (!IsCodePointDefined(fontinfo, code))
                {
                    if (!inRange) continue;

                    tempRange.End = (char)(code - 1);
                    inRange = false;
                    yield return tempRange;
                }
                else
                {
                    if (!inRange)
                    {
                        inRange = true;
                        tempRange.Start = (char)code;
                    }

                    if (code == range.End)
                    {
                        tempRange.End = (char)code;
                        yield return tempRange;
                    }
                }
            }
        }

        private static bool IsCodePointDefined(stbtt_fontinfo fontinfo, int unicode)
        {
            return stbtt_FindGlyphIndex(fontinfo, unicode) != 0;
        }
    }
}
