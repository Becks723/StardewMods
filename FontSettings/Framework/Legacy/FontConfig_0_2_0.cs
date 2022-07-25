using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Legacy
{
    /// <summary>0.2.0用到0.3.2，于0.4.0版本归档。</summary>
    internal class FontConfig_0_2_0 : FontConfig_0_1_0
    {
        public string Locale { get; set; }

        internal virtual LanguageInfo GetLanguage()
        {
            return new LanguageInfo(this.Lang, this.Locale);
        }
    }
}
