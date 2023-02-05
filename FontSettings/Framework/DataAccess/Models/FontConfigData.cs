using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace FontSettings.Framework.DataAccess.Models
{
    internal class FontConfigData
    {
        public bool Enabled { get; set; } = false;

        public LocalizedContentManager.LanguageCode Lang { get; set; }

        public string Locale { get; set; }

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

        /// <summary>字符在横轴上的偏移量。</summary>
        public float CharOffsetX { get; set; }

        /// <summary>字符在纵轴上的偏移量。</summary>
        public float CharOffsetY { get; set; }

        /// <summary>缩放倍数。仅当<see cref="InGameType"/>为<see cref="GameFontType.SpriteText"/>时用到。对应<see cref="StardewValley.BellsAndWhistles.SpriteText.fontPixelZoom"/>字段。</summary>
        public float PixelZoom { get; set; }

    }
}
