using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace FontSettings.Framework
{
    internal static class Textures
    {
        private static IModContentHelper _modContent;
        public static void Init(IModContentHelper modContent)
        {
            _modContent = modContent;
        }

        public static Texture2D FontTab => LoadTexture("assets/font-tab.png");
        public static Texture2D Refresh => LoadTexture("assets/refresh.png");
        public static Texture2D Save => LoadTexture("assets/save.png");
        public static Texture2D Delete => LoadTexture("assets/delete.png");
        public static Texture2D FontPreviewNormal => LoadTexture("assets/font-preview-normal.png");
        public static Texture2D FontPreviewCompare => LoadTexture("assets/font-preview-compare.png");
        public static Texture2D SectionBox => LoadTexture("assets/section-box.png");
        public static Texture2D TitleFontButton => LoadTexture("assets/font-button.png");

        private static Texture2D LoadTexture(string relativePath)
        {
            return _modContent.Load<Texture2D>(relativePath);
        }
    }
}
