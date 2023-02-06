using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework.Patchers
{
    internal class SpriteTextPatcher
    {
        private static readonly IDictionary<LanguageInfo, float> _pixelZoomLookup = new Dictionary<LanguageInfo, float>();

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), "setUpCharacterMap"),
                postfix: new HarmonyMethod(typeof(SpriteTextPatcher), nameof(SpriteText_setUpCharacterMap_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), "OnLanguageChange"),
                postfix: new HarmonyMethod(typeof(SpriteTextPatcher), nameof(SpriteText_OnLanguageChange_Postfix))
            );
        }

        public void SetOverridePixelZoom(float pixelZoom)
        {
            _pixelZoomLookup[FontHelpers.GetCurrentLanguage()] = pixelZoom;
        }

        private static void SpriteText_setUpCharacterMap_Postfix()
        {
            OverrideFontPixelZoom();
        }

        private static void SpriteText_OnLanguageChange_Postfix()
        {
            OverrideFontPixelZoom();
        }

        private static void OverrideFontPixelZoom()
        {
            if (!LocalizedContentManager.CurrentLanguageLatin)
            {
                if (_pixelZoomLookup.TryGetValue(FontHelpers.GetCurrentLanguage(), out float pixelZoom))
                    SpriteText.fontPixelZoom = pixelZoom;
            }
        }
    }
}
