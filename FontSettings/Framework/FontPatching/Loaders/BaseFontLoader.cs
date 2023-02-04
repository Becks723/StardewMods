using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontPatching.Loaders
{
    internal abstract class BaseFontLoader<T> : IFontLoader
    {
        private readonly FontConfig_ _config;

        protected BaseFontLoader(FontConfig_ config)
        {
            this._config = config;
        }

        protected abstract T Load(FontConfig_ config);

        public object Load()
        {
            return this.Load(this._config);
        }
    }
}
