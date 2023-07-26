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
    }
}
