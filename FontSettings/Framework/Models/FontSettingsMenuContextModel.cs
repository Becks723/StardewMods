using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models
{
    internal class FontSettingsMenuContextModel
    {
        public GameFontType FontType { get; set; } = GameFontType.SmallFont;
        public PreviewMode PreviewMode { get; set; } = PreviewMode.Normal;
        public bool ShowBounds { get; set; } = false;
        public bool ShowText { get; set; } = true;
        public bool OffsetTuning { get; set; } = false;
        public Dictionary<GameFontType, FontSettingsMenuPresetContextModel> Presets { get; }
        public FontSettingsMenuContextModel()
        {
            this.Presets = new();
            foreach (var fontType in Enum.GetValues<GameFontType>())
                this.Presets.Add(fontType, new());
        }
    }

    internal class FontSettingsMenuPresetContextModel
    {
        /// <summary>0 for no selected preset.</summary>
        public int PresetIndex { get; set; }
    }
}
