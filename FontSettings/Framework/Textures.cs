using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FontSettings.Framework
{
    internal static class Textures
    {
        private static readonly ISet<string> _textureAssetKeys = new HashSet<string>();

        private static IManifest _manifest;
        public static void Init(IManifest manifest)
        {
            _manifest = manifest;
        }

        public static Texture2D FontTab => LoadTextureAsAsset("font-tab");
        public static Texture2D Refresh => LoadTextureAsAsset("refresh");
        public static Texture2D Save => LoadTextureAsAsset("save");
        public static Texture2D Delete => LoadTextureAsAsset("delete");
        public static Texture2D FontPreviewNormal => LoadTextureAsAsset("font-preview-normal");
        public static Texture2D FontPreviewCompare => LoadTextureAsAsset("font-preview-compare");
        public static Texture2D SectionBox => LoadTextureAsAsset("section-box");
        public static Texture2D TitleFontButton => LoadTextureAsAsset("font-button");

        private const string _fontMenuIconKey = "icon-for-toolbar-icons";
        public static string FontMenuIconKey
        {
            get
            {
                _textureAssetKeys.Add(_fontMenuIconKey);
                return NormalizeKey(_fontMenuIconKey);
            }
        }

        private static Texture2D LoadTextureAsAsset(string key)
        {
            _textureAssetKeys.Add(key);

            string assetName = NormalizeKey(key);
            return Game1.content.Load<Texture2D>(assetName);
        }

        /// <summary>Usage: Attach to `AssetRequested` event.</summary>
        public static void OnAssetRequested(AssetRequestedEventArgs e)
        {
            string key = _textureAssetKeys
                .Where(key => e.NameWithoutLocale.IsEquivalentTo(NormalizeKey(key)))
                .FirstOrDefault();
            if (key != null)
            {
                e.LoadFromModFile<Texture2D>($"assets/{key}.png", AssetLoadPriority.Exclusive);
            }
        }

        private static string NormalizeKey(string key)
        {
            return $"Mods/{_manifest.UniqueID}/{key}";
        }
    }
}
