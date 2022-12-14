using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.FontChangers
{
    internal class SmallFontChanger : SpriteFontChanger
    {
        protected override string AssetName => LocalizeBaseAssetName("Fonts/SmallFont");

        public SmallFontChanger(IModHelper helper, ModConfig config)
            : base(helper, config)
        {
        }
    }
}
