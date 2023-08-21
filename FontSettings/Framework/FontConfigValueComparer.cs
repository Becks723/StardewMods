using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal class FontConfigValueComparer : IEqualityComparer<FontConfig>
    {
        public bool Equals(FontConfig? x, FontConfig? y)
        {
            if (x == null) return y == null;
            if (y == null) return x == null;

            return x.Enabled == y.Enabled
                && x.FontFilePath == y.FontFilePath
                && x.FontIndex == y.FontIndex
                && x.FontSize == y.FontSize
                && x.Spacing == y.Spacing
                && x.LineSpacing == y.LineSpacing
                && x.CharOffsetX == y.CharOffsetX
                && x.CharOffsetY == y.CharOffsetY
                && this.CharacterRangesValueEquals(x.CharacterRanges, y.CharacterRanges)

                && x.TryGetInstance(out IWithPixelZoom? bmx) == y.TryGetInstance(out IWithPixelZoom? bmy)
                && bmx?.PixelZoom == bmy?.PixelZoom;
        }

        int IEqualityComparer<FontConfig>.GetHashCode(FontConfig obj)
        {
            throw new NotImplementedException();
        }

        private bool CharacterRangesValueEquals(IEnumerable<CharacterRange>? x, IEnumerable<CharacterRange>? y)
        {
            if (x == null) return y == null;
            if (y == null) return x == null;

            if (x.Count() != y.Count())
                return false;
            IEnumerable<char> chx = FontHelpers.GetCharacters(x).OrderBy(c => c);
            IEnumerable<char> chy = FontHelpers.GetCharacters(y).OrderBy(c => c);

            var itx = chx.GetEnumerator();
            var ity = chy.GetEnumerator();
            try
            {
                bool movedNext;
                while ((movedNext = itx.MoveNext()) == ity.MoveNext()
                    && itx.Current == ity.Current
                    && movedNext) ;

                return !itx.MoveNext() && !ity.MoveNext();
            }
            finally
            {
                itx.Dispose();
                ity.Dispose();
            }
        }
    }
}
