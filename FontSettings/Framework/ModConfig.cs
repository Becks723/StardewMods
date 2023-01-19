using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FontSettings.Framework
{
    internal class ModConfig
    {
        public string ExampleText { get; set; }

        internal FontConfigs Fonts { get; set; }  // 这个不要储存在config.json里，故设为internal。

        internal SampleData Sample { get; set; }  // 同上

        internal VanillaFontData VanillaFont { get; set; }  // 同上

        public int MinFontSize { get; set; }

        public int MaxFontSize { get; set; }

        public int MinSpacing { get; set; }

        public int MaxSpacing { get; set; }

        public int MinLineSpacing { get; set; }

        public int MaxLineSpacing { get; set; }

        public int MinCharOffsetX { get; set; }

        public int MaxCharOffsetX { get; set; }

        public int MinCharOffsetY { get; set; }

        public int MaxCharOffsetY { get; set; }

        public float MinPixelZoom { get; set; }

        public float MaxPixelZoom { get; set; }

        public bool DisableTextShadow { get; set; } = false;

        public KeybindList OpenFontSettingsMenu { get; set; } = KeybindList.Parse($"{nameof(SButton.LeftAlt)} + {nameof(SButton.F)}");

        public bool FontSettingsInGameMenu { get; set; } = true;

        private readonly int DEFAULT_MinFontSize = 5;
        private readonly int DEFAULT_MaxFontSize = 75;
        private readonly int DEFAULT_MinSpacing = -10;
        private readonly int DEFAULT_MaxSpacing = 10;
        private readonly int DEFAULT_MinLineSpacing = 5;
        private readonly int DEFAULT_MaxLineSpacing = 75;
        private readonly int DEFAULT_MinCharOffsetX = -10;
        private readonly int DEFAULT_MaxCharOffsetX = 10;
        private readonly int DEFAULT_MinCharOffsetY = -10;
        private readonly int DEFAULT_MaxCharOffsetY = 10;
        private readonly float DEFAULT_MinPixelZoom = 0.5f;
        private readonly float DEFAULT_MaxPixelZoom = 5f;

        public ModConfig()
        {
            this.ResetToDefault();
        }

        public void ResetToDefault()
        {
            this.ExampleText = string.Empty;
            this.Fonts = new FontConfigs();
            this.MinFontSize = this.DEFAULT_MinFontSize;
            this.MaxFontSize = this.DEFAULT_MaxFontSize;
            this.MinSpacing = this.DEFAULT_MinSpacing;
            this.MaxSpacing = this.DEFAULT_MaxSpacing;
            this.MinLineSpacing = this.DEFAULT_MinLineSpacing;
            this.MaxLineSpacing = this.DEFAULT_MaxLineSpacing;
            this.MinCharOffsetX = this.DEFAULT_MinCharOffsetX;
            this.MaxCharOffsetX = this.DEFAULT_MaxCharOffsetX;
            this.MinCharOffsetY = this.DEFAULT_MinCharOffsetY;
            this.MaxCharOffsetY = this.DEFAULT_MaxCharOffsetY;
            this.MinPixelZoom = this.DEFAULT_MinPixelZoom;
            this.MaxPixelZoom = this.DEFAULT_MaxPixelZoom;
            this.DisableTextShadow = false;
            this.OpenFontSettingsMenu = KeybindList.Parse($"{nameof(SButton.LeftAlt)} + {nameof(SButton.F)}");
            this.FontSettingsInGameMenu = true;
        }
    }
}
