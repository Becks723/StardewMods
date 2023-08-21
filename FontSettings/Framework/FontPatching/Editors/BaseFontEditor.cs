using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontPatching.Editors
{
    internal abstract class BaseFontEditor<T> : IFontEditor
    {
        private readonly FontConfig _config;

        public int Priority { get; }

        public void Edit(object data)
        {
            if (data is T tData)
                this.Edit(tData, this._config);
        }

        protected abstract void Edit(T data, FontConfig config);

        protected BaseFontEditor(FontConfig config, int priority)
        {
            this._config = config;
            this.Priority = priority;
        }
    }
}
