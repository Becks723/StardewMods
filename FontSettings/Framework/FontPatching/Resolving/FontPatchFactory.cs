using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontPatching.Editors;
using FontSettings.Framework.FontPatching.Loaders;
using FontSettings.Framework.FontPatching.Replacers;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontPatching.Resolving
{
    internal class FontPatchFactory
    {
        private readonly BmFontLoadHelper _bmFontLoadHelper = new();

        public IFontPatch ForBypassSpriteFont()
            => this.CreatePatch();

        public IFontPatch ForLoadSpriteFont(FontConfig config)
            => this.CreatePatch(new SpriteFontLoader(config));

        public IFontPatch ForLoadSpriteFont(SpriteFont spriteFont)
            => this.CreatePatch(new SimpleFontLoader(spriteFont));

        public IFontPatch ForEditSpriteFont(FontConfig config)
            => this.CreatePatch(new SpriteFontEditor(config));

        public IFontPatch ForReplaceSpriteFont(SpriteFont spriteFont)
            => this.CreatePatch(new FontReplacer(spriteFont));

        public IBmFontPatch ForBypassBmFont()  // TODO：当enabled为false时，pixelZoom应该是原版设置，不一定是1f。
            => this.CreateBmPatch();

        public IBmFontPatch ForLoadBmFont(BmFontData bmFont, float fontPixelZoom)
        {
            this._bmFontLoadHelper.GetLoaders(bmFont,
                out IFontLoader fontFileLoader,
                out IDictionary<string, IFontLoader> pageLoaders);
            return this.CreateBmPatch(fontFileLoader, pageLoaders, fontPixelZoom);
        }

        public IBmFontPatch ForReplaceBmFont(BmFontData bmFont, float fontPixelZoom)
        {
            this._bmFontLoadHelper.GetLoaders(bmFont,
                out IFontReplacer fontFileReplacer,
                out IDictionary<string, IFontLoader> pageLoaders);
            return this.CreateBmPatch(fontFileReplacer, pageLoaders, fontPixelZoom);
        }

        public IBmFontPatch ForEditBmFont(FontConfig config)
        {
            float pixelZoom = config.Supports<IWithPixelZoom>()
                ? config.GetInstance<IWithPixelZoom>().PixelZoom
                : 1f;
            return this.CreateBmPatch(new BmFontFileEditor(config), pixelZoom);
        }

        private IFontPatch CreatePatch() => new FontPatch(null, null);
        private IFontPatch CreatePatch(IFontLoader loader) => new FontPatch(loader, null);
        private IFontPatch CreatePatch(IFontEditor editor) => new FontPatch(null, editor);
        private IFontPatch CreatePatch(IFontLoader loader, IFontEditor editor) => new FontPatch(loader, editor);

        private IBmFontPatch CreateBmPatch() => new BmFontPatch(null, null, null);
        private IBmFontPatch CreateBmPatch(IFontLoader loader, IDictionary<string, IFontLoader> pageLoaders, float fontPixelZoom) => new BmFontPatch(loader, null, pageLoaders, fontPixelZoom);
        private IBmFontPatch CreateBmPatch(IFontEditor editor, float fontPixelZoom) => new BmFontPatch(null, editor, null, fontPixelZoom);
        private IBmFontPatch CreateBmPatch(IFontReplacer replacer, IDictionary<string, IFontLoader> pageLoaders, float fontPixelZoom) => new BmFontPatch(null, replacer, pageLoaders, fontPixelZoom);
    }
}
