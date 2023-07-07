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

        public IFontPatch ForLoadSpriteFont(FontConfig config, int priority = int.MaxValue)
            => this.CreatePatch(new SpriteFontLoader(config, priority));

        public IFontPatch ForLoadSpriteFont(SpriteFont spriteFont, int priority = int.MaxValue)
            => this.CreatePatch(new SimpleFontLoader(spriteFont, priority));

        public IFontPatch ForEditSpriteFont(FontConfig config, int priority = 0)
            => this.CreatePatch(new SpriteFontEditor(config, priority));

        public IFontPatch ForReplaceSpriteFont(SpriteFont spriteFont, int priority = 0)
            => this.CreatePatch(new FontReplacer(spriteFont, priority));

        public IBmFontPatch ForBypassBmFont()  // TODO：当enabled为false时，pixelZoom应该是原版设置，不一定是1f。
            => this.CreateBmPatch();

        public IBmFontPatch ForLoadBmFont(BmFontData bmFont, float fontPixelZoom, int priority = int.MaxValue)
        {
            this._bmFontLoadHelper.GetLoaders(bmFont, priority,
                out IFontLoader fontFileLoader,
                out IDictionary<string, IFontLoader> pageLoaders);
            return this.CreateBmPatch(fontFileLoader, pageLoaders, fontPixelZoom);
        }

        public IBmFontPatch ForReplaceBmFont(BmFontData bmFont, float fontPixelZoom, int priority = 0)
        {
            this._bmFontLoadHelper.GetLoaders(bmFont, priority,
                out IFontReplacer fontFileReplacer,
                out IDictionary<string, IFontLoader> pageLoaders);
            return this.CreateBmPatch(fontFileReplacer, pageLoaders, fontPixelZoom);
        }

        public IBmFontPatch ForEditBmFont(FontConfig config, int priority = 0)
        {
            float pixelZoom = config.Supports<IWithPixelZoom>()
                ? config.GetInstance<IWithPixelZoom>().PixelZoom
                : 1f;
            return this.CreateBmPatch(new BmFontFileEditor(config, priority), pixelZoom);
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
