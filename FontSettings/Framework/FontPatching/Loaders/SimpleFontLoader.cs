using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching.Loaders
{
    internal class SimpleFontLoader : IFontLoader
    {
        private readonly object _data;

        public SimpleFontLoader(object data)
        {
            this._data = data;
        }

        public object Load()
        {
            return this._data;
        }
    }
}
