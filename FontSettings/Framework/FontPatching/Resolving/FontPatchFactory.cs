using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
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

        public IBmFontPatch ForBypassBmFont(float defaultPixelZoom)
            => this.CreateBmPatch(defaultPixelZoom);

        public IBmFontPatch ForLoadBmFont(BmFontData bmFont, float fontPixelZoom, int priority = int.MaxValue)
        {
            this._bmFontLoadHelper.GetLoaders(bmFont, priority,
                out IFontLoader fontFileLoader,
                out IDictionary<string, IFontLoader> pageLoaders);
            return this.CreateBmPatch(fontFileLoader, pageLoaders, fontPixelZoom);
        }

        public IBmFontPatch ForReplaceBmFont(BmFontData bmFont, float fontPixelZoom, bool withFakeLoader = false, int priority = 0)
        {
            this._bmFontLoadHelper.GetLoaders(bmFont, priority,
                out IFontReplacer fontFileReplacer,
                out IDictionary<string, IFontLoader> pageLoaders);
            return !withFakeLoader
                ? this.CreateBmPatch(fontFileReplacer, pageLoaders, fontPixelZoom)
                : this.CreateBmPatchWithFakeLoader(fontFileReplacer, pageLoaders, fontPixelZoom);
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

        private IBmFontPatch CreateBmPatch(float fontPixelZoom = 1f) => new BmFontPatch(null, null, null, fontPixelZoom);
        private IBmFontPatch CreateBmPatch(IFontLoader loader, IDictionary<string, IFontLoader> pageLoaders, float fontPixelZoom) => new BmFontPatch(loader, null, pageLoaders, fontPixelZoom);
        private IBmFontPatch CreateBmPatch(IFontEditor editor, float fontPixelZoom) => new BmFontPatch(null, editor, null, fontPixelZoom);
        private IBmFontPatch CreateBmPatch(IFontReplacer replacer, IDictionary<string, IFontLoader> pageLoaders, float fontPixelZoom) => new BmFontPatch(null, replacer, pageLoaders, fontPixelZoom);
        private IBmFontPatch CreateBmPatchWithFakeLoader(IFontReplacer replacer, IDictionary<string, IFontLoader> pageLoaders, float fontPixelZoom) => new BmFontPatch(this.FakeBmFontLoader(), replacer, pageLoaders, fontPixelZoom);
        private IFontLoader FakeBmFontLoader()
        {
            FontFile fakeFont = new FontFile()
            {
                Info = new BmFont.FontInfo(),
                Common = new FontCommon(),
                Pages = new List<FontPage>(),
                Chars = new List<FontChar>(),
                Kernings = new List<FontKerning>()
            };
            XmlSource xml = FontHelpers.ParseFontFile(fakeFont);
            return new SimpleFontLoader(xml, int.MinValue /* a fake loader is only a placeholder, so its priority should be the lowest. */);
        }
    }
}
