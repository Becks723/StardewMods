using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace FontSettings.Framework.Legacy
{
    internal class FontConfig_0_1_0
    {
        public bool Enabled { get; set; } = false;

        public LocalizedContentManager.LanguageCode Lang { get; set; }

        public GameFontType InGameType { get; set; }
        
        [Obsolete("自0.5.0版本起正式废除。")]
        public string ExistingFontPath { get; set; }

        public string FontFilePath { get; set; }

        /// <summary>字体在合集文件（.ttc、.otc）中的索引。</summary>
        public int FontIndex { get; set; } = 0;

        /// <summary>字体大小，单位为像素px。</summary>
        public float FontSize { get; set; }

        public float Spacing { get; set; }

        /// <summary>行间距，即两个相邻基准线的距离。（ascent - descent + lineGap）</summary>
        public int LineSpacing { get; set; }

        public int? TextureWidth { get; set; }

        public int? TextureHeight { get; set; }

        public IEnumerable<CharacterRange> CharacterRanges { get; set; }
    }
}
