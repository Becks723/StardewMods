using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;

namespace FontSettings.Framework
{
    [Obsolete]
    internal class FontPresetComparer : IEqualityComparer<FontPresetData>
    {
        public bool Equals(FontPresetData x, FontPresetData y)
        {
            if (x is null) return y is null;
            if (y is null) return x is null;

            return this.Equals(x.Name, y.Name);
        }

        public bool Equals(string presetName1, string presetName2)
        {
            if (presetName1 is null) throw new ArgumentNullException(nameof(presetName1));
            if (presetName2 is null) throw new ArgumentNullException(nameof(presetName2));

            // 当作文件名比较。
            if (StardewModdingAPI.Constants.TargetPlatform is StardewModdingAPI.GamePlatform.Windows)
                return presetName1.ToLowerInvariant() == presetName2.ToLowerInvariant();
            else
                return presetName1 == presetName2;
        }

        public int GetHashCode([DisallowNull] FontPresetData obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
