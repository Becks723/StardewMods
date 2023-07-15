using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models.Builder
{
    internal record FontConfigSpriteFont : FontConfigDecorator, IWithDefaultCharacter
    {
        public char? DefaultCharacter { get; }

        public FontConfigSpriteFont(FontConfigDecorator config, char? defaultCharacter)
            : base(config)
        {
            this.DefaultCharacter = defaultCharacter;
        }

        public override bool Supports<T>()
        {
            return typeof(T).IsAssignableFrom(typeof(IWithDefaultCharacter))
                || base.Supports<T>();
        }

        public override T GetInstance<T>()
        {
            if (typeof(T).IsAssignableFrom(typeof(IWithDefaultCharacter)))
                return this as T;

            return base.GetInstance<T>();
        }
    }
}
