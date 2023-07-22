using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.Models
{
    internal class FontSettingsMenuContextModel
    {
        public GameFontType FontType { get; set; } = GameFontType.SmallFont;
        public PreviewMode PreviewMode { get; set; } = PreviewMode.Normal;
        public bool ShowBounds { get; set; } = false;
        public bool ShowText { get; set; } = true;
        public bool OffsetTuning { get; set; } = false;
        public Dictionary<GameFontType, FontSettingsMenuPresetContextModel> Presets { get; } = new();
        public Dictionary<GameFontType, ExportContextModel> Exporting { get; } = new();
        public FontSettingsMenuContextModel()
        {
            foreach (var fontType in Enum.GetValues<GameFontType>())
            {
                this.Presets.Add(fontType, new());
                this.Exporting.Add(fontType, new());
            }
        }
    }

    internal class FontSettingsMenuPresetContextModel
    {
        /// <summary>0 for no selected preset.</summary>
        public int PresetIndex { get; set; }
    }

    internal class ExportContextModel
    {
        public bool IsFirstTime { get; set; } = true;  // 第一次传入时赋默认值
        public string OutputDirectory { get; set; }
        public string OutputName { get; set; }
        public bool InXnb { get; set; }
        public XnbPlatform XnbPlatform { get; set; }
        public GameFramework GameFramework { get; set; }
        public GraphicsProfile GraphicsProfile { get; set; }
        public bool IsCompressed { get; set; }
        public int PageWidth { get; set; }
        public int PageHeight { get; set; }
    }
}
