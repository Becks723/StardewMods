using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.Preset
{
    internal class FontPresetReal : IExtensible
    {
        public LanguageInfo Language { get; }

        public GameFontType FontType { get; }

        public FontConfig_ Settings { get; }

        public FontPresetReal(LanguageInfo language, GameFontType fontType, FontConfig_ settings)
        {
            this.Language = language;
            this.FontType = fontType;
            this.Settings = settings;
        }

        public FontPresetReal(FontPresetReal copy)
            : this(copy.Language, copy.FontType, copy.Settings)
        {
        }

        public virtual T? GetInstance<T>() where T : class => this as T;

        public virtual bool Supports<T>() where T : class => this is T;
    }
}
