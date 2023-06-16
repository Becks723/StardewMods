using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models
{
    internal class FontPresetModel : IExtensible
    {
        public FontContext Context { get; }
        public FontConfigModel Settings { get; }

        public FontPresetModel(FontContext context, FontConfigModel settings)
        {
            this.Settings = settings;
            this.Context = context;
        }

        #region IExtensible implementation
        public virtual bool Supports<T>() where T : class => this is T;
        public virtual T GetInstance<T>() where T : class => this as T;
        #endregion
    }
}
