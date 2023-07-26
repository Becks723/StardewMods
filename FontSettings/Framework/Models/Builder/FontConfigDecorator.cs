using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models.Builder
{
    internal record FontConfigDecorator : FontConfig
    {
        private readonly FontConfigDecorator _config;

        public FontConfigDecorator(FontConfigDecorator config)
            : base(config)
        {
            this._config = config;
        }

        protected FontConfigDecorator(FontConfig config)
            : base(config)
        {
        }

        public override bool Supports<T>()
        {
            return this._config.Supports<T>() 
                || base.Supports<T>();
        }

        public override T GetInstance<T>()
        {
            return this._config.GetInstance<T>() 
                ?? base.GetInstance<T>();
        }
    }
}
