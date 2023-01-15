using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontChangers;
using StardewModdingAPI;

namespace FontSettings.Framework
{
    internal class GameFontChangerImpl : IGameFontChanger, IAsyncGameFontChanger
    {
        private readonly IGameFontChanger _smallFontChanger;
        private readonly IGameFontChanger _dialogueFontChanger;
        private readonly IGameFontChanger _spriteTextChanger;

        private readonly IAsyncGameFontChanger _asyncSmallFontChanger;
        private readonly IAsyncGameFontChanger _asyncDialogueFontChanger;
        private readonly IAsyncGameFontChanger _asyncSpriteTextChanger;

        public GameFontChangerImpl(IModHelper helper, ModConfig config, Func<LanguageInfo, GameFontType, string> getVanillaFontFile)
        {
            var smallFontChanger = new SmallFontChanger(helper, config, getVanillaFontFile);
            this._smallFontChanger = smallFontChanger;
            this._asyncSmallFontChanger = smallFontChanger;

            var dialogueFontChanger = new DialogueFontChanger(helper, config, getVanillaFontFile);
            this._dialogueFontChanger = dialogueFontChanger;
            this._asyncDialogueFontChanger = dialogueFontChanger;

            var spriteTextChanger = new SpriteTextChanger(helper, config, getVanillaFontFile);
            this._spriteTextChanger = spriteTextChanger;
            this._asyncSpriteTextChanger = spriteTextChanger;
        }

        public bool ChangeGameFont(FontConfig font)
        {
            switch (font.InGameType)
            {
                case GameFontType.SmallFont:
                    return this._smallFontChanger.ChangeGameFont(font);

                case GameFontType.DialogueFont:
                    return this._dialogueFontChanger.ChangeGameFont(font);

                case GameFontType.SpriteText:
                    return this._spriteTextChanger.ChangeGameFont(font);

                default:
                    throw new NotSupportedException();
            }
        }

        public async Task<bool> ChangeGameFontAsync(FontConfig font)
        {
            switch (font.InGameType)
            {
                case GameFontType.SmallFont:
                    return await this._asyncSmallFontChanger.ChangeGameFontAsync(font);

                case GameFontType.DialogueFont:
                    return await this._asyncDialogueFontChanger.ChangeGameFontAsync(font);

                case GameFontType.SpriteText:
                    return await this._asyncSpriteTextChanger.ChangeGameFontAsync(font);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
