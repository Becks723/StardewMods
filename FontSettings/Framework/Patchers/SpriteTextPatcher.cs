using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
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

        /// <summary>Raise when game temporarily sets <see cref="SpriteText.fontPixelZoom"/> to a fixed value. We don't want to override pixelZoom in this state.</summary>
        public static event EventHandler PixelZoomFixed;
        /// <summary>Raise when game releases <see cref="SpriteText.fontPixelZoom"/>. Now we can override pixelZoom.</summary>
        public static event EventHandler PixelZoomUnfixed;
        public static bool IsPixelZoomFixed;

        public SpriteTextPatcher(ModConfig config)
        {
            _config = config;

            PixelZoomFixed += (_, _) => IsPixelZoomFixed = true;
            PixelZoomUnfixed += (_, _) => IsPixelZoomFixed = false;
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
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), nameof(SpriteText.drawString)),
                transpiler: new HarmonyMethod(typeof(SpriteTextPatcher), nameof(SpriteText_drawString_Transpiler)),
                finalizer: new HarmonyMethod(typeof(SpriteTextPatcher), nameof(SpriteText_drawString_Finalizer)));
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
            bool useBmFont = _config.EnableLatinDialogueFont || !LocalizedContentManager.CurrentLanguageLatin;
            if (!IsPixelZoomFixed)
            {
                if (!useBmFont)
                    SpriteText.fontPixelZoom = 3f;
                else
                {
                    if (_pixelZoomLookup.TryGetValue(FontHelpers.GetCurrentLanguage(), out float pixelZoom))
                        SpriteText.fontPixelZoom = pixelZoom;
                }
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

        private static IEnumerable<CodeInstruction> SpriteText_drawString_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var oldInstructions = codeInstructions.ToArray();
            for (int i = 0; i < oldInstructions.Length; i++)
            {
                var instruction = oldInstructions[i];

                /*
                 * float num5 = SpriteText.fontPixelZoom;
                 *                                       /
                 *                                      <    RaisePixelZoomFixed();
                 *                                       \
                 * if ((SpriteText.IsSpecialCharacter(s[i]) | junimoText) || SpriteText.forceEnglishFont)
                 * {
                 *     SpriteText.fontPixelZoom = 3f;
                 * }
                 */
                if (i >= 1
                    && oldInstructions[i - 1].opcode == OpCodes.Ldsfld
                    && oldInstructions[i - 1].operand is FieldInfo { Name: nameof(SpriteText.fontPixelZoom) }
                    && instruction.opcode == OpCodes.Stloc_S)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(SpriteTextPatcher), nameof(RaisePixelZoomFixed)));
                }

                /*
                 * SpriteText.fontPixelZoom = num5; /
                 *                                 <    RaisePixelZoomUnfixed();
                 * ...                              \
                 */
                // 三处
                else if (i >= 1
                    && oldInstructions[i - 1].opcode == OpCodes.Ldloc_S
                    && instruction.opcode == OpCodes.Stsfld
                    && instruction.operand is FieldInfo { Name: nameof(SpriteText.fontPixelZoom) }
                    && oldInstructions[i + 1].opcode == OpCodes.Br)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(SpriteTextPatcher), nameof(RaisePixelZoomUnfixed)));
                }

                else
                    yield return instruction;
            }
        }

        private static void SpriteText_drawString_Finalizer(Exception? __exception)
        {
            IsPixelZoomFixed = false;  // 确保不会永远开启，如果抛异常了（其实这里不管抛没抛），就关掉。
        }

        private static void RaisePixelZoomFixed()
        {
            PixelZoomFixed?.Invoke(null, EventArgs.Empty);
        }

        private static void RaisePixelZoomUnfixed()
        {
            PixelZoomUnfixed?.Invoke(null, EventArgs.Empty);
        }
    }
}
