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

        public static Texture2D FontTab
            => _modContent.Load<Texture2D>("assets/font-tab.png");

        public static Texture2D Refresh
            => _modContent.Load<Texture2D>("assets/刷新.png");

        public static Texture2D Icons
            => _modContent.Load<Texture2D>("assets/icons.png");
    }
}
