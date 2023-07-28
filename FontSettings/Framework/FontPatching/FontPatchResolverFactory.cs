using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontPatching.Resolving;

namespace FontSettings.Framework.FontPatching
{
    internal class FontPatchResolverFactory
    {
        private readonly ModConfig _config;

        public FontPatchResolverFactory(ModConfig config)
        {
            this._config = config;
        }

        public IFontPatchResolver CreateResolver(GameFontType fontType)
        {
            return fontType switch
            {
                GameFontType.SmallFont => new SpriteFontPatchResolver(this.PatchModeInfo),
                GameFontType.DialogueFont => new SpriteFontPatchResolver(this.PatchModeInfo),
                GameFontType.SpriteText => new BmFontPatchResolver(this.PatchModeInfo),
                _ => throw new NotSupportedException(),
            };
        }

        private PatchModeInfo PatchModeInfo()
        {
            return new PatchModeInfo(
                loadOrReplace: !this._config.EditMode,
                loadPriority: int.MaxValue,
                editPriority: this._config.EditPriority);
        }
    }
}
