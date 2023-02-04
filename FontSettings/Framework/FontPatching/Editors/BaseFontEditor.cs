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
        private readonly FontConfig_ _config;

        public void Edit(object data)
        {
            if (data is T tData)
                this.Edit(tData, this._config);
        }

        protected abstract void Edit(T data, FontConfig_ config);

        protected BaseFontEditor(FontConfig_ config)
        {
            this._config = config;
        }
    }
}
