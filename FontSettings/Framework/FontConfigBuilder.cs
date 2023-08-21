using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using FontSettings.Framework.Models.Builder;
using Microsoft.Xna.Framework;

namespace FontSettings.Framework
{
    internal class FontConfigBuilder
    {
        private FontConfigDecorator _config;

        public FontConfigBuilder()
        {
        }

        public FontConfigBuilder(FontConfig basicConfig)
        {
            this.BasicConfig(basicConfig);
        }

        public FontConfig Build()
        {
            try
            {
                return this._config;
            }
            finally
            {
                this._config = null;
            }
        }

        public FontConfigBuilder BasicConfig(FontConfig config)
        {
            this._config = new FontConfigBasic(config);
            return this;
        }

        /// <summary>Adds support to <see cref="IWithDefaultCharacter"/>.</summary>
        public FontConfigBuilder WithDefaultCharacter(char? defaultCharacter)
        {
            this._config = new FontConfigSpriteFont(this._config, defaultCharacter);
            return this;
        }

        /// <summary>Adds support to <see cref="IWithPixelZoom"/>.</summary>
        public FontConfigBuilder WithPixelZoom(float pixelZoom)
        {
            this._config = new FontConfigBm(this._config, pixelZoom);
            return this;
        }

        /// <summary>Adds support to <see cref="IWithSolidColor"/>.</summary>
        public FontConfigBuilder WithSolidColorMask(Color mask)
        {
            this._config = new FontConfigSolidColor(this._config, mask);
            return this;
        }
    }
}
