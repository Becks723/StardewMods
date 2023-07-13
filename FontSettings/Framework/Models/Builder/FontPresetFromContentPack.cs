using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;
using StardewModdingAPI;

namespace FontSettings.Framework.Models.Builder
{
    internal class FontPresetFromContentPack : FontPresetDecorator, IPresetFromContentPack
    {
        public IContentPack SContentPack { get; }

        public FontPresetFromContentPack(FontPresetDecorator preset, IContentPack sContentPack)
            : base(preset)
        {
            this.SContentPack = sContentPack;
        }
    }
}
