using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FontSettings.Framework.FontInfomation
{
    internal class FontModel : IEquatable<FontModel>
    {
        public string FullPath { get; set; }

        public string FamilyName { get; set; }

        public string Name { get; set; }

        public string SubfamilyName { get; set; }

        public int FontIndex { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as FontModel);
        }

        public bool Equals(FontModel other)
        {
            static bool IsEmpty(FontModel font) => font.FullPath is null;

            if (IsEmpty(this))
                return IsEmpty(other);

            string file1 = System.IO.Path.GetFileName(this.FullPath);
            string file2 = System.IO.Path.GetFileName(other.FullPath);
            return file1 == file2 && this.FontIndex == other.FontIndex;
        }

        public override int GetHashCode()
        {
            return (this.FullPath ?? string.Empty).GetHashCode() ^ this.FontIndex.GetHashCode();
        }

        public static bool operator ==(FontModel left, FontModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FontModel left, FontModel right)
        {
            return !(left == right);
        }
    }
}
