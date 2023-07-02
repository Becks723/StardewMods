using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class FontViewModelComparer : IEqualityComparer
    {
        bool IEqualityComparer.Equals(object? x, object? y)
        {
            var fontX = x as FontViewModel;
            var fontY = y as FontViewModel;

            if (fontX == null)
                return fontY == null;
            else if (fontY == null)
                return fontX == null;
            else
            {
                return fontX.DisplayText == fontY.DisplayText
                    && fontX.FontFilePath == fontX.FontFilePath
                    && fontX.FontIndex == fontY.FontIndex;
            }
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();  // interface contract

            var font = (FontViewModel)obj;
            return font.GetHashCode();
        }
    }
}
