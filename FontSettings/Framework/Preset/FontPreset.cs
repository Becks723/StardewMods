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
        public LanguageInfo Language { get; }

        public GameFontType FontType { get; }

        public FontConfig Settings { get; }

        public FontPreset(LanguageInfo language, GameFontType fontType, FontConfig settings)
        {
            this.Language = language;
            this.FontType = fontType;
            this.Settings = settings;
        }

        public FontPreset(FontPreset copy)
            : this(copy.Language, copy.FontType, copy.Settings)
        {
        }

        public virtual T? GetInstance<T>() where T : class => this as T;

        public virtual bool Supports<T>() where T : class => this is T;
    }
}
