using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework.Patchers
{
    internal class SpriteTextPatcher
    {
        private static readonly IDictionary<LanguageInfo, float> _pixelZoomLookup = new Dictionary<LanguageInfo, float>();
        private static ModConfig _config;

        public SpriteTextPatcher(ModConfig config)
        {
            _config = config;
        }

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), "setUpCharacterMap"),
                postfix: new HarmonyMethod(typeof(SpriteTextPatcher), nameof(SpriteText_setUpCharacterMap_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), "OnLanguageChange"),
                postfix: new HarmonyMethod(typeof(SpriteTextPatcher), nameof(SpriteText_OnLanguageChange_Postfix)),
                finalizer: new HarmonyMethod(typeof(SpriteTextPatcher), nameof(SpriteText_OnLanguageChange_Finalizer))
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
            if (_config.EnableLatinDialogueFont || !LocalizedContentManager.CurrentLanguageLatin)
            {
                if (_pixelZoomLookup.TryGetValue(FontHelpers.GetCurrentLanguage(), out float pixelZoom))
                    SpriteText.fontPixelZoom = pixelZoom;
            }
        }

        private static Exception SpriteText_OnLanguageChange_Finalizer(Exception __exception)
        {
            /*
             * Suppress exception:
             * Happens when changing from BmFont-supported to latin (i.e. non-BmFont-supported) language, such as zh to en.
             * 
             * at SpriteText::OnLanguageChange:
             * 	foreach (FontPage current2 in SpriteText.FontFile.Pages)
			 *  {
			 *	   SpriteText.fontPages.Add(Game1.content.Load<Texture2D>("Fonts\\" + current2.File));
			 *  }
             *
             * `FontFile` field remains zh's, and Font Settings changes the pages' name. 
             * So a `FileNotFoundException` wrapped in a `ContentLoadException` is thrown.
             * We just need to suppress it, since en does not use these fields.
             */

            if (__exception is ContentLoadException contentLoadException
                && contentLoadException.InnerException is FileNotFoundException)
                return null;

            return __exception;
        }
    }
}
