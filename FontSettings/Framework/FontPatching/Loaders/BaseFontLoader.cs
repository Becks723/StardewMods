﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontPatching.Loaders
{
    internal abstract class BaseFontLoader<T> : IFontLoader
    {
        private readonly FontConfig _config;

        public int Priority { get; }

        protected BaseFontLoader(FontConfig config, int priority)
        {
            this._config = config;
            this.Priority = priority;
        }

        protected abstract T Load(FontConfig config);

        public object Load()
        {
            return this.Load(this._config);
        }
    }
}
