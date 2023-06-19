using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.DataAccess.Models
{
    internal class FontContentPack
    {
        public string FontFile { get; set; }
        public int Index { get; set; }
        public string Type { get; set; }  // 以逗号隔开：small, medium, dialogue
        public string Language { get; set; }  // 以逗号隔开；内置语言如en、zh，自定义语言如th、pl；如两个及以上，则不会使用用户规定的字符范围，而是各自语言下的默认范围
        public float Size { get; set; }
        public float Spacing { get; set; }
        public float LineSpacing { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float PixelZoom { get; set; }
        public string Character { get; set; }  // 自定义字符范围
        public string CharacterAppend { get; set; }  // 在原有字符的基础上，增加的字符
        public string CharacterRemove { get; set; }  // 在原有字符的基础上，移除的字符

        public string Name { get; set; }  // 支持翻译，格式：{{i18n: XXX}}
        public string Notes { get; set; }  // 支持翻译
    }
}
