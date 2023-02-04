using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

#nullable enable

namespace FontSettings.Framework.DataAccess.Parsing
{
    internal class FontConfigParserForUser : FontConfigParser
    {
        private readonly IVanillaFontConfigProvider _vanillaFontConfigProvider;

        public FontConfigParserForUser(IFontFileProvider fontFileProvider, IVanillaFontConfigProvider vanillaFontConfigProvider)
            : base(fontFileProvider)
        {
            this._vanillaFontConfigProvider = vanillaFontConfigProvider;
        }

        protected override string? ParseFontFilePath(string? path, LanguageInfo language, GameFontType fontType)
        {
            return this._fontFilePathParseHelper.ParseFontFilePath(path, this._fontFileProvider.FontFiles,
                this._vanillaFontConfigProvider, language, fontType);
        }

        protected override string? ParseBackFontFilePath(string? path, LanguageInfo language, GameFontType fontType)
        {
            return this._fontFilePathParseHelper.ParseBackFontFilePath(path, this._fontFileProvider.FontFiles,
                this._vanillaFontConfigProvider, language, fontType);
        }
    }
}
