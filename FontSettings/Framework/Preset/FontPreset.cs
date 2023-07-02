using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.Preset
{
    internal class FontPreset : IExtensible
    {
        [Obsolete]
        public LanguageInfo Language => this.Context.Language;

        [Obsolete]
        public GameFontType FontType => this.Context.FontType;

        public FontContext Context { get; }

        public FontConfig Settings { get; }

        [Obsolete]
        public FontPreset(LanguageInfo language, GameFontType fontType, FontConfig settings)
            : this(new FontContext(language, fontType), settings)
        {
        }

        public FontPreset(FontContext context, FontConfig settings)
        {
            this.Context = context;
            this.Settings = settings;
        }

        public FontPreset(FontPreset copy)
            : this(copy.Context, copy.Settings)
        {
        }

        public virtual T? GetInstance<T>() where T : class => this as T;

        public virtual bool Supports<T>() where T : class => this is T;
    }
}
