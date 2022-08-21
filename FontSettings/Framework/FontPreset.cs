using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace FontSettings.Framework
{
    internal class FontPreset
    {
        internal string Name { get; set; }  // 不写入文件，故internal。

        public FontPresetPrecondition Requires { get; set; }

        public FontPresetFontType FontType { get; set; }

        public int FontIndex { get; set; }

        public LocalizedContentManager.LanguageCode Lang { get; set; }

        public string Locale { get; set; }

        public float FontSize { get; set; }

        public float Spacing { get; set; }

        public int LineSpacing { get; set; }

        public float CharOffsetX { get; set; }

        public float CharOffsetY { get; set; }
    }
}
