using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontInfo.OpenType;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontInfo
{
    internal class FontInfoRetriever : IFontInfoRetriever
    {
        public IResult<FontModel[]> GetFontInfo(string fontFile)
        {
            try
            {
                FontFormat format = GetFontFormat(fontFile);

                FontModel[] info = format switch
                {
                    FontFormat.OpenType => this.LoadSingleFont(fontFile),
                    FontFormat.OpenTypeCollection => this.LoadFontCollection(fontFile),
                    FontFormat.Unknown => throw new NotSupportedException($"不支持的字体格式！文件：{fontFile}"),
                    _ => throw new NotSupportedException("..."),
                };

                return SuccessResult(info);
            }

            catch (Exception ex)
            {
                return ErrorResult(ex);
            }
        }

        private FontModel[] LoadSingleFont(string fontFile)
            => new FontModel[1] { this.LoadSingleFontCore(fontFile) };

        private FontModel[] LoadFontCollection(string fontFile)
        {
            using var reader = new OpenTypeCommonReader(File.OpenRead(fontFile));

            TTCHeader ttcHeader = new TTCHeader(reader);
            FontModel[] result = new FontModel[ttcHeader.NumFonts];
            for (int i = 0; i < result.Length; i++)
            {
                reader.Seek(ttcHeader.TableDirectoryOffsets[i], SeekOrigin.Begin);
                result[i] = this.LoadFont(fontFile, reader);
                result[i].FontIndex = i;
            }

            return result;
        }

        private FontModel LoadFont(string fontFile, OpenTypeCommonReader reader)
        {
            uint sfntVersion = reader.ReadUInt32();
            Outlines outlines = (Outlines)sfntVersion;
            ushort numTables = reader.ReadUInt16();
            ushort searchRange = reader.ReadUInt16();
            ushort entrySelector = reader.ReadUInt16();
            ushort rangeShift = reader.ReadUInt16();

            var tables = new Dictionary<string, TableRecord>(numTables);
            for (int i = 0; i < numTables; i++)
            {
                TableRecord table = new(
                    reader.ReadTag(),
                    reader.ReadUInt32(),
                    reader.ReadOffset32(),
                    reader.ReadUInt32()
                );
                tables[table.Tag] = table;
            }

            var tableReader = new TableReader(reader,
                new ReadOnlyDictionary<string, TableRecord>(tables));
            NameTable nameTable = tableReader.ReadNameTable();

            return new FontModel
            {
                FullPath = fontFile,
                FamilyName = nameTable.FontFamily(CultureInfo.InstalledUICulture),
                Name = nameTable.FontFullName(CultureInfo.InstalledUICulture),
                SubfamilyName = nameTable.FontSubfamily(CultureInfo.InstalledUICulture),
                FontIndex = 0
            };
        }

        private FontModel LoadSingleFontCore(string fontFile)
        {
            using (var reader = new OpenTypeCommonReader(File.OpenRead(fontFile)))
                return this.LoadFont(fontFile, reader);
        }

        //private bool IsFontSupported(string extension)
        //{
        //    return extension.Equals(".ttf", StringComparison.InvariantCultureIgnoreCase)
        //        || extension.Equals(".otf", StringComparison.InvariantCultureIgnoreCase)
        //        || extension.Equals(".ttc", StringComparison.InvariantCultureIgnoreCase)
        //        || extension.Equals(".otc", StringComparison.InvariantCultureIgnoreCase);
        //}

        private static FontFormat GetFontFormat(string fontFilePath)
        {
            using var reader = new OpenTypeCommonReader(File.OpenRead(fontFilePath));

            ushort s1 = reader.ReadUInt16();
            ushort s2 = reader.ReadUInt16();

            if ((s1 >> 8 & 0xFF) == (byte)'t' &&
                (s1 & 0xFF) == (byte)'t' &&
                (s2 >> 8 & 0xFF) == (byte)'c' &&
                (s2 & 0xFF) == (byte)'f')
                return FontFormat.OpenTypeCollection;
            else if ((s1 << 16 | s2) is 0x00010000 or 0x4F54544F)
                return FontFormat.OpenType;
            else
                return FontFormat.Unknown;
        }

        private static IResult<FontModel[]> SuccessResult(FontModel[] data) => new Result(true, data, null);
        private static IResult<FontModel[]> ErrorResult(Exception ex) => new Result(false, null, ex);
        private record Result(bool IsSuccess, FontModel[] Data, Exception Exception) : IResult<FontModel[]>
        {
            public FontModel[] GetData() => this.Data;

            public string GetError()
            {
                var ex = this.Exception;
                return ex != null
                    ? $"{ex.Message}\n{ex.StackTrace}"
                    : null;
            }
        }
    }
}
