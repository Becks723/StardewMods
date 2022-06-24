namespace FontSettings.Framework
{
    internal class ModConfig
    {
        public string ExampleText { get; set; } = "AaBbYyZz\n测试用例";

        internal FontConfigs Fonts { get; set; }  // 这个不要储存在config.json里，故设为internal。
    }
}
