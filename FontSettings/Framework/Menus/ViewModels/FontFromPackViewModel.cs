using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class FontFromPackViewModel : FontViewModel
    {
        public IManifest PackManifest { get; }

        public FontFromPackViewModel(string? fontFilePath, int fontIndex, string displayText, IManifest packManifest)
            : base(fontFilePath, fontIndex, displayText)
        {
            this.PackManifest = packManifest;
        }
    }
}
