using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching
{
    internal class FontPatchChangerFactory : IGameFontChangerFactory
    {
        private readonly FontPatchResolverFactory _resolverFactory;
        private readonly FontPatchInvalidatorManager _invalidatorManager;

        public FontPatchChangerFactory(FontPatchResolverFactory resolverFactory, FontPatchInvalidatorManager invalidatorManager)
        {
            this._resolverFactory = resolverFactory;
            this._invalidatorManager = invalidatorManager;
        }

        public IAsyncGameFontChanger3 CreateAsyncChanger(GameFontType fontType)
        {
            return new FontPatchChanger(
                resolver: this.GetResolver(fontType),
                invalidator: this.GetInvalidator(fontType));
        }

        public IGameFontChanger3 CreateChanger(GameFontType fontType)
        {
            throw new NotSupportedException();
        }

        private IFontPatchResolver GetResolver(GameFontType fontType) => this._resolverFactory.CreateResolver(fontType);
        private IFontPatchInvalidator GetInvalidator(GameFontType fontType) => this._invalidatorManager.GetInvalidator(fontType);
    }
}
