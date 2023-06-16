using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;

namespace FontSettings.Framework.Models
{
    internal class FontContentPackModel
    {
        public FontContentPack Raw { get; }
        public ISet<LanguageInfo> Languages { get; }
        public ISet<GameFontType> Types { get; }
        public IEnumerable<CharacterRange> CharacterOverride { get; }
        public IEnumerable<CharacterRange> CharacterAppend { get; }
        public IEnumerable<CharacterRange> CharacterRemove { get; }

        public FontContentPackModel(FontContentPack raw, IEnumerable<LanguageInfo> languages, IEnumerable<GameFontType> types, IEnumerable<CharacterRange> characterOverride, IEnumerable<CharacterRange> characterAppend, IEnumerable<CharacterRange> characterRemove)
        {
            this.Raw = raw;
            this.Languages = new HashSet<LanguageInfo>(languages);
            this.Types = new HashSet<GameFontType>(types);
            this.CharacterOverride = characterOverride;
            this.CharacterAppend = characterAppend;
            this.CharacterRemove = characterRemove;
        }
    }
}
