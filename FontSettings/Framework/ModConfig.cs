using System.Collections.Generic;
using System.IO;
using System.Linq;
using FontSettings.Framework.DataAccess.Models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FontSettings.Framework
{
    internal class ModConfig
    {
        public string ExampleText { get; set; }

        internal SampleData Sample { get; set; }  // 这个不要储存在config.json里，故设为internal。

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

        public bool DisableTextShadow { get; set; }

        public KeybindList OpenFontSettingsMenu { get; set; }

        public IList<string> CustomFontFolders { get; set; }

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
        private readonly bool DEFAULT_DisableTextShadow = false;
        private readonly KeybindList DEFAULT_OpenFontSettingsMenu = KeybindList.Parse($"{nameof(SButton.LeftAlt)} + {nameof(SButton.F)}");
        private readonly string[] DEFAULT_CustomFontFolders = GetDefaultCustomFontFolders().ToArray();

        public ModConfig()
        {
            this.ResetToDefault();
        }

        public void ResetToDefault()
        {
            this.ExampleText = string.Empty;
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
            this.DisableTextShadow = this.DEFAULT_DisableTextShadow;
            this.OpenFontSettingsMenu = this.DEFAULT_OpenFontSettingsMenu;
            this.CustomFontFolders = new List<string>(this.DEFAULT_CustomFontFolders);
            foreach (string folder in this.CustomFontFolders)
                Directory.CreateDirectory(folder);
        }

        private static IEnumerable<string> GetDefaultCustomFontFolders()
        {
            switch (Constants.TargetPlatform)
            {
                case GamePlatform.Android:
                    yield return Path.Combine(Constants.GamePath, "FontSettings", "fonts");
                    break;

                default:
                    yield break;
            }
        }
    }
}
