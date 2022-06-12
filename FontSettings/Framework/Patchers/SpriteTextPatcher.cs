using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework.Patchers
{
    internal class SpriteTextPatcher
    {
        private static ModConfig _config;
        private static RuntimeFontManager _fontManager;
        private static GameFontChanger _fontChanger;

        public SpriteTextPatcher(ModConfig config, RuntimeFontManager fontManager, GameFontChanger fontChanger)
        {
            _config = config;
            _fontManager = fontManager;
            _fontChanger = fontChanger;
        }

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), "setUpCharacterMap"),
                prefix: new HarmonyMethod(typeof(SpriteTextPatcher), nameof(SpriteText_setUpCharacterMap_Prefix)),
                postfix: new HarmonyMethod(typeof(SpriteTextPatcher), nameof(SpriteText_setUpCharacterMap_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), "OnLanguageChange"),
                postfix: new HarmonyMethod(typeof(SpriteTextPatcher), nameof(SpriteText_OnLanguageChange_Postfix))
            );
        }

        private static void SpriteText_setUpCharacterMap_Prefix(ref bool __state)
        {
            // state表示是否为第一次。
            __state = !LocalizedContentManager.CurrentLanguageLatin && SpriteTextFields._characterMap == null;
        }

        private static async void SpriteText_setUpCharacterMap_Postfix(bool __state)
        {
            if (__state)
                await OnLanguageChangedAsync("SpriteText.setUpCharacterMap");
        }

        private static async void SpriteText_OnLanguageChange_Postfix()
        {
            await OnLanguageChangedAsync("SpriteText.OnLanguageChange");
        }

        private static void OnLanguageChangedSync(string methodName)
        {
            try
            {
                _fontManager.RecordBuiltInBmFont();

                var customFonts = _config.Fonts.Where(f => f.InGameType is GameFontType.SpriteText
                    && f.Lang == (LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode);
                foreach (var font in customFonts)
                {
                    _fontChanger.ReplaceOriginalOrRamain(font);
                }
            }
            catch (Exception ex)
            {
                ILog.Error($"在修改{methodName}时遇到了错误：{ex.Message}\n堆栈信息：\n{ex.StackTrace}");
                return;
            }
        }

        private static async Task OnLanguageChangedAsync(string methodName)
        {
            try
            {
                _fontManager.RecordBuiltInBmFont();

                List<Task> tasks = new();
                var customFonts = _config.Fonts.Where(f => f.InGameType is GameFontType.SpriteText
                    && f.Lang == (LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode);
                foreach (var font in customFonts)
                {
                    Task task = _fontChanger.ReplaceOriginalOrRemainAsync(font);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                ILog.Error($"在修改{methodName}时遇到了错误：{ex.Message}\n堆栈信息：\n{ex.StackTrace}");
                return;
            }
        }
    }
}
