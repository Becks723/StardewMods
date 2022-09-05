using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FontSettings.Framework
{
    internal class ModConfig
    {
        public string ExampleText { get; set; } = "AaBbYyZz\n测试用例";

        internal FontConfigs Fonts { get; set; }  // 这个不要储存在config.json里，故设为internal。

        internal int MinFontSize { get; } = 1;  // 只读属性pulic的话会显示在json文件里。

        public int MaxFontSize { get; set; } = 100;

        public int MinSpacing { get; set; } = -10;

        public int MaxSpacing { get; set; } = 10;

        internal int MinLineSpacing { get; } = 1;

        public int MaxLineSpacing { get; set; } = 100;

        public int MinCharOffsetX { get; set; } = -10;

        public int MaxCharOffsetX { get; set; } = 10;

        public int MinCharOffsetY { get; set; } = -10;

        public int MaxCharOffsetY { get; set; } = 10;

        public bool DisableTextShadow { get; set; } = false;

        public KeybindList OpenFontSettingsMenu { get; set; } = KeybindList.Parse($"{nameof(SButton.LeftAlt)} + {nameof(SButton.F)}");

        public bool FontSettingsInGameMenu { get; set; } = true;

        public void ResetToDefault()
        {
            this.ExampleText = "AaBbYyZz\n测试用例";
            this.Fonts = new FontConfigs();
            this.MaxFontSize = 100;
            this.MinSpacing = -10;
            this.MaxSpacing = 10;
            this.MaxLineSpacing = 100;
            this.MinCharOffsetX = -10;
            this.MaxCharOffsetX = 10;
            this.MinCharOffsetY = -10;
            this.MaxCharOffsetY = 10;
            this.DisableTextShadow = false;
            this.OpenFontSettingsMenu = KeybindList.Parse($"{nameof(SButton.LeftAlt)} + {nameof(SButton.F)}");
            this.FontSettingsInGameMenu = true;
        }
    }
}
