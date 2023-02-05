using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Menus.ViewModels
{
    internal class FontViewModel
    {
        public string? FontFilePath { get; }

        public int FontIndex { get; }

        public string DisplayText { get; }

        public FontViewModel(string? fontFilePath, int fontIndex, string displayText)
        {
            this.FontFilePath = fontFilePath;
            this.FontIndex = fontIndex;
            this.DisplayText = displayText;
        }
    }
}
