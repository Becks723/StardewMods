using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework.Patchers
{
    internal class SpriteTextLatinPatcher
    {
        private static ModConfig _config;
        private static IManifest _manifest;

        public SpriteTextLatinPatcher(ModConfig config, IManifest manifest, IModHelper helper)
        {
            _config = config;
            _manifest = manifest;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(LatinFontFileAssetName()))
            {
                e.LoadFromModFile<XmlSource>("assets/fonts/Latin.fnt", AssetLoadPriority.Exclusive);
            }
            else if (IsLatinFontPage(e.NameWithoutLocale, out string pageFile))
            {
                e.LoadFromModFile<Texture2D>($"assets/fonts/{pageFile}.png", AssetLoadPriority.Exclusive);
            }
        }

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), "setUpCharacterMap"),
                postfix: this.HarmonyMethod(nameof(SpriteText_setUpCharacterMap_Postfix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), nameof(SpriteText.drawString)),
                transpiler: this.HarmonyMethod(nameof(SpriteText_drawString_Transpiler)),
                finalizer: this.HarmonyMethod(nameof(SpriteText_drawString_Finalizer)));
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), nameof(SpriteText.getColorFromIndex)),
                postfix: this.HarmonyMethod(nameof(SpriteText_getColorFromIndex_Postfix)));
        }

        private static void SpriteText_drawString_Finalizer(Exception __exception)
        {
            //throw __exception;
        }

        private static void SpriteText_setUpCharacterMap_Postfix()
        {
            if (!CustomSpriteTextInLatinLanguages())
                return;

            if (LocalizedContentManager.CurrentLanguageLatin && SpriteTextFields._characterMap == null)
            {
                SpriteTextFields._characterMap = new Dictionary<char, FontChar>();
                SpriteTextFields.fontPages = new List<Texture2D>();

                SpriteTextFields.FontFile = LoadFontFile(LatinFontFileAssetName());
                SpriteText.fontPixelZoom = 3f;

                foreach (FontChar fontChar in SpriteTextFields.FontFile.Chars)
                {
                    char key = (char)fontChar.ID;
                    SpriteTextFields._characterMap.Add(key, fontChar);
                }
                foreach (FontPage fontPage in SpriteTextFields.FontFile.Pages)
                {
                    SpriteTextFields.fontPages.Add(
                        Game1.content.Load<Texture2D>(LatinFontPageAssetName(fontPage.File)));
                }

                LocalizedContentManager.OnLanguageChange += OnLanguageChange_Latin;
            }
        }

        private static void OnLanguageChange_Latin(LocalizedContentManager.LanguageCode code)
        {
            if (LocalizedContentManager.CurrentLanguageLatin)
            {
                if (SpriteTextFields._characterMap != null)
                    SpriteTextFields._characterMap.Clear();
                else
                    SpriteTextFields._characterMap = new Dictionary<char, FontChar>();
                if (SpriteTextFields.fontPages != null)
                    SpriteTextFields.fontPages.Clear();
                else
                    SpriteTextFields.fontPages = new List<Texture2D>();

                SpriteTextFields.FontFile = LoadFontFile(LatinFontFileAssetName());
                SpriteText.fontPixelZoom = 3f;

                foreach (FontChar fontChar in SpriteTextFields.FontFile.Chars)
                {
                    char key = (char)fontChar.ID;
                    SpriteTextFields._characterMap.Add(key, fontChar);
                }
                foreach (FontPage fontPage in SpriteTextFields.FontFile.Pages)
                {
                    SpriteTextFields.fontPages.Add(
                        Game1.content.Load<Texture2D>(LatinFontPageAssetName(fontPage.File)));
                }
            }
        }

        private static IEnumerable<CodeInstruction> SpriteText_drawString_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {//659
            var oldInstructions = codeInstructions.ToArray();
            for (int i = 0; i < oldInstructions.Length; i++)
            {
                var instruction = oldInstructions[i];

                if (instruction.opcode == OpCodes.Call
                    && instruction.operand is MethodInfo { Name: "get_CurrentLanguageLatin" }
                    && oldInstructions[i + 4].opcode == OpCodes.Callvirt
                    && oldInstructions[i + 4].operand is MethodInfo { Name: "get_Chars" }
                    && oldInstructions[i + 5].opcode == OpCodes.Call
                    && oldInstructions[i + 5].operand is MethodInfo { Name: "IsSpecialCharacter" })
                    yield return new CodeInstruction(instruction)
                    {
                        opcode = OpCodes.Call,
                        operand = CurrentLanguageLatinPatched_Method.Value
                    };

                else
                    yield return instruction;
            }
        }

        private static void SpriteText_getColorFromIndex_Postfix(int index, ref Color __result)
        {
            if (!CustomSpriteTextInLatinLanguages())
                return;

            if (index == -1 && LocalizedContentManager.CurrentLanguageLatin)
                __result = new Color(86, 22, 12);
        }

        private static readonly Lazy<MethodInfo> CurrentLanguageLatinPatched_Method = new(
            () => AccessTools.Method(typeof(SpriteTextLatinPatcher), nameof(CurrentLanguageLatinPatched)));
        private static bool CurrentLanguageLatinPatched()
        {
            if (CustomSpriteTextInLatinLanguages())
                return false;
            else
                return LocalizedContentManager.CurrentLanguageLatin;
        }

        private static string LatinFontFileAssetName()
        {
            return $"Mods/{_manifest.UniqueID}/Fonts/Latin";
        }

        private static string LatinFontPageAssetName(string pageFile)  // pageFile: without file extensions
        {
            return $"Mods/{_manifest.UniqueID}/Fonts/{pageFile}";
        }

        private static bool IsLatinFontPage(IAssetName assetName, out string pageFile)  // pageFile: without file extensions
        {
            string prefix = $"{LatinFontFileAssetName()}_";
            if (assetName.StartsWith(prefix))
            {
                int start = $"Mods/{_manifest.UniqueID}/Fonts/".Length;
                pageFile = assetName.BaseName.Substring(start);
                return true;
            }
            else
            {
                pageFile = null;
                return false;
            }
        }

        private static FontFile LoadFontFile(string assetName)
        {
            return FontLoader.Parse(
                Game1.content.Load<XmlSource>(assetName).Source);
        }

        private static bool CustomSpriteTextInLatinLanguages()
            => _config.EnableLatinDialogueFont;

        private HarmonyMethod HarmonyMethod(string methodName)
            => new HarmonyMethod(this.GetType(), methodName);
    }
}
