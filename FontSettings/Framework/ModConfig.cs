namespace FontSettings.Framework
{
    internal class ModConfig
    {
        public string ExampleText { get; set; } = "AaBbYyZz\n测试用例";

        internal FontConfigs Fonts { get; set; }  // 这个不要储存在config.json里，故设为internal。

        public int MinCharOffsetX { get; set; } = -10;

        public int MaxCharOffsetX { get; set; } = 10;

        public int MinCharOffsetY { get; set; } = -10;

        public int MaxCharOffsetY { get; set; } = 10;

        public int MaxFontSize { get; set; } = 100;

        public int MinFontSize { get; } = 1;

        public int MaxSpacing { get; set; } = 10;

        public int MinSpacing { get; set; } = -10;

        public int MaxLineSpacing { get; set; } = 100;

        public int MinLineSpacing { get; } = 1;

        public void ResetToDefault()
        {
            this.ExampleText = "AaBbYyZz\n测试用例";
            this.Fonts = new FontConfigs();
            this.MaxFontSize = 100;
            this.MaxSpacing = 10;
            this.MinSpacing = -10;
            this.MaxLineSpacing = 100;
            this.MaxCharOffsetX = 10;
            this.MinCharOffsetX = -10;
            this.MaxCharOffsetY = 10;
            this.MinCharOffsetY = -10;
        }
    }
}
