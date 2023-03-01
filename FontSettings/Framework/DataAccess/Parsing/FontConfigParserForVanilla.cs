using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.DataAccess.Parsing
{
    internal class FontConfigParserForVanilla : FontConfigParser
    {
        public FontConfigParserForVanilla(IFontFileProvider fontFileProvider, IVanillaFontProvider vanillaFontProvider)
            : base(fontFileProvider, vanillaFontProvider)
        {
        }

        protected override string ParseFontFilePath(string path, LanguageInfo language, GameFontType fontType)
        {
            return this._fontFilePathParseHelper.ParseFontFilePathOrNull(path, this._fontFileProvider.FontFiles);
        }
    }
}
