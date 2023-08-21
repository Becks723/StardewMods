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

        public int Priority { get; }

        public SimpleFontLoader(object data, int priority = int.MaxValue)
        {
            this._data = data;
            this.Priority = priority;
        }


        public object Load()
        {
            return this._data;
        }
    }
}
