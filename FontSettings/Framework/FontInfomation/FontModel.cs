using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FontSettings.Framework.FontInfomation
{
    internal class FontModel
    {
        public string FullPath { get; set; }

        public string FamilyName { get; set; }

        public string Name { get; set; }

        public string SubfamilyName { get; set; }

        public int FontIndex { get; set; }
    }

    internal class FontEqualityComparer : EqualityComparer<FontModel>
    {
        public override bool Equals(FontModel x, FontModel y)
        {
            static bool IsEmpty(FontModel font) => font.FullPath is null;

            if (IsEmpty(x))
                return IsEmpty(y);

            string file1 = System.IO.Path.GetFileName(x.FullPath);
            string file2 = System.IO.Path.GetFileName(y.FullPath);
            return file1 == file2 && x.FontIndex == y.FontIndex;
        }

        public override int GetHashCode([DisallowNull] FontModel obj)
        {
            return (obj.FullPath ?? string.Empty).GetHashCode() ^ obj.FontIndex.GetHashCode();
        }
    }
}
