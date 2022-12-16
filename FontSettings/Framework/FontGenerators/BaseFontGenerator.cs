using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontGenerators
{
    internal abstract class BaseFontGenerator<TParameter> : IFontGenerator, IAsyncFontGenerator
        where TParameter : FontGeneratorParameter
    {
        public ISpriteFont GenerateFont(FontGeneratorParameter param)
        {
            TParameter tParam = this.CastParameterType(param);

            return this.GenerateFontCore(tParam);
        }

        public async Task<ISpriteFont> GenerateFontAsync(FontGeneratorParameter param)
        {
            TParameter tParam = this.CastParameterType(param);

            return await this.GenerateFontAsyncCore(tParam);
        }

        public async Task<ISpriteFont> GenerateFontAsync(FontGeneratorParameter param, CancellationToken cancellationToken)
        {
            TParameter tParam = this.CastParameterType(param);

            return await this.GenerateFontAsyncCore(tParam, cancellationToken);
        }

        protected abstract ISpriteFont GenerateFontCore(TParameter param);
        protected abstract Task<ISpriteFont> GenerateFontAsyncCore(TParameter param);
        protected virtual async Task<ISpriteFont> GenerateFontAsyncCore(TParameter param, CancellationToken cancellationToken)
        {
            return await this.GenerateFontAsyncCore(param);
        }

        private TParameter CastParameterType(FontGeneratorParameter param, string message = null)
        {
            if (param is TParameter tParam)
                return tParam;
            else
            {
                if (message != null)
                    throw new InvalidCastException(message);
                else
                    throw new InvalidCastException();
            }
        }

    }
}
