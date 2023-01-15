using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.FontChangers
{
    internal class DialogueFontChanger : SpriteFontChanger
    {
        protected override string AssetName => LocalizeBaseAssetName("Fonts/SpriteFont1");

        public DialogueFontChanger(IModHelper helper, ModConfig config, Func<LanguageInfo, GameFontType, string> getVanillaFontFile)
            : base(helper, config, getVanillaFontFile)
        {
        }
    }
}
