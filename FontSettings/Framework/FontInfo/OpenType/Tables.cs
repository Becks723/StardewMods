using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontInfo.OpenType
{
    /// <summary>https://docs.microsoft.com/zh-cn/typography/opentype/spec/name#platform-ids</summary>
    internal enum PlatformIDs : ushort
    {
        Unicode,
        Macintosh,
        Windows
    }

    /// <summary>https://docs.microsoft.com/zh-cn/typography/opentype/spec/name#name-ids</summary>
    internal enum NameIDs : ushort
    {
        Copyright = 0,
        FontFamily = 1,
        FontSubfamily = 2,
        UniqueIdentifier = 3,
        FullName = 4,
        VersionString = 5,
        PostScript = 6,
        Trademark = 7,
        Manufacturer = 8,
        Designer = 9,
        Description = 10,
        UrlVendor = 11,
        UrlDesigner = 12,
        LicenseDescription = 13,
        LicenseUrl = 14,
        Reserved = 15,
        TypographicFamily = 16,
        TypographicSubfamily = 17,
        CompatibleFull = 18,
        SampleText = 19,
        PostScript_CID_findfont_name = 20,
        WWS_family_name = 21,
        WWS_subfamily_name = 22,
        LightBackgroundPalette = 23,
        DarkBackgroundPalette = 24,
        VariationsPostScriptNamePrefix = 25
    }

    internal class NameRecord
    {
        public PlatformIDs PlatformID { get; }

        public ushort EncodingID { get; }

        public ushort LanguageID { get; }

        public NameIDs NameID { get; }

        public string Value { get; }

        public NameRecord(OpenTypeCommonReader reader, long offset)
        {
            this.PlatformID = (PlatformIDs)reader.ReadUInt16();
            this.EncodingID = reader.ReadUInt16();
            this.LanguageID = reader.ReadUInt16();
            this.NameID = (NameIDs)reader.ReadUInt16();
            ushort length = reader.ReadUInt16();
            ushort stringOffset = reader.ReadUInt16();

            long savedPos = reader.Position;  // 保存当前流位置。

            reader.Seek(offset + stringOffset, SeekOrigin.Begin);
            byte[] bytesStr = reader.ReadBytes(length);
            Encoding encoding = this.EncodingID is 1 or 3 ? Encoding.BigEndianUnicode : Encoding.UTF8;  // TODO: 编码这块还不大明白，为啥只考虑了PlatformID为0（Unicode）的情况。
            string value = encoding.GetString(bytesStr, 0, bytesStr.Length);
            value = value.Replace("\0", string.Empty);  // 有可能隔一个字符一个\0，如EarMasterJazz.ttf
            this.Value = value;

            reader.Seek(savedPos, SeekOrigin.Begin);  // 读完数据返回保存的流位置。
        }
    }

    internal class NameTable
    {
        public static NameTable Read(OpenTypeCommonReader reader)
        {
            long tableOffset = reader.Position;
            ushort version = reader.ReadUInt16();
            ushort count = reader.ReadUInt16();
            ushort storageOffset = reader.ReadOffset16();
            List<NameRecord> names = new List<NameRecord>(count);
            for (int i = 0; i < count; i++)
                names.Add(new NameRecord(reader, tableOffset + storageOffset));

            return new NameTable(names.ToArray());
        }

        private readonly NameRecord[] _names;

        public NameTable(NameRecord[] names)
        {
            this._names = names;
        }

        public string FontFamily(CultureInfo culture)
        {
            return this.GetName(culture.LCID, NameIDs.FontFamily);
        }

        public string FontSubfamily(CultureInfo culture)
        {
            return this.GetName(culture.LCID, NameIDs.FontSubfamily);
        }

        public string FontFullName(CultureInfo culture)
        {
            return this.GetName(culture.LCID, NameIDs.FullName);
        }

        /// <param name="langaugeID">即<see cref="CultureInfo.LCID"/>，具体ID表去微软文档找。</param>
        /// <returns>若没找到匹配的，返回空字符串<see cref="string.Empty"/>。</returns>
        public string GetName(int langaugeID, NameIDs nameID)
        {
            string bestMatch = null;  // 参数要求的语言
            string secondBest = null;  // 本机内置语言
            string thirdBest = null;  // 无关语言（culture-independent）
            string fourthBest = null;   // 英语（美国）
            string fifthBest = null;  // 跳过语言检查

            foreach (NameRecord nameRecord in this._names)
            {
                if (nameRecord.NameID != nameID)
                    continue;

                if (nameRecord.LanguageID == langaugeID)
                    bestMatch ??= nameRecord.Value;
                if (nameRecord.LanguageID == CultureInfo.InstalledUICulture.LCID)
                    secondBest ??= nameRecord.Value;
                if (nameRecord.LanguageID == CultureInfo.InvariantCulture.LCID)
                    thirdBest ??= nameRecord.Value;
                if (nameRecord.LanguageID == 0x0409)
                    fourthBest ??= nameRecord.Value;
                fifthBest ??= nameRecord.Value;
            }

            return bestMatch ?? secondBest ?? thirdBest ?? fourthBest ?? fifthBest ?? string.Empty;
        }
    }
}
