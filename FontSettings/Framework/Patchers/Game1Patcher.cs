using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace FontSettings.Framework.Patchers
{
    internal class Game1Patcher
    {
        private static ModConfig _config;

        private static RuntimeFontManager _fontManager;

        private static GameFontChanger _fontChanger;

        public Game1Patcher(ModConfig config, RuntimeFontManager fontManager, GameFontChanger fontChanger)
        {
            _config = config;
            _fontManager = fontManager;
            _fontChanger = fontChanger;
        }

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.TranslateFields)),
                postfix: new HarmonyMethod(typeof(Game1Patcher), nameof(Game1_TranslateFields_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), "set_activeClickableMenu"),
                prefix: new HarmonyMethod(typeof(Game1Patcher), nameof(Game1_set_activeClickableMenu_Prefix))
            );
        }

        private static async void Game1_TranslateFields_Postfix()
        {
            try
            {
                _fontManager.RecordBuiltInSpriteFont();
                CharRangeSource.RecordBuiltInCharRange();

                List<Task> tasks = new();
                var customFonts = _config.Fonts.Where(f => f.InGameType is GameFontType.SmallFont or GameFontType.DialogueFont
                    && f.Lang == (LanguageCode)(int)LocalizedContentManager.CurrentLanguageCode);
                foreach (var font in customFonts)
                {
                    Task task = _fontChanger.ReplaceOriginalOrRemainAsync(font);
                    tasks.Add(task);
                }

                //Texture2D tex = Game1.smallFont.Texture;
                //string fileName = $"spriteFont-{Path.GetFileNameWithoutExtension(font.FontFilePath)}-{font.FontSize}px-{DateTime.Now:HH-mm-ss}";
                //using Stream stream = File.OpenWrite(@$"D:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\CustomFonts\Temp\{fileName}.png");
                //tex.SaveAsPng(stream, tex.Width, tex.Height);

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                ILog.Error($"在修改Game1.TranslateFields时遇到了错误：{ex.Message}\n堆栈信息：\n{ex.StackTrace}");
                return;
            }
            finally
            {
                GC.Collect();
            }
        }

        private static void Game1_set_activeClickableMenu_Prefix(IClickableMenu ____activeClickableMenu)
        {
            if (____activeClickableMenu is GameMenu gameMenu && !gameMenu.HasDependencies())
            {
                foreach (IClickableMenu subMenu in gameMenu.pages)
                {
                    if (subMenu is IDisposable dis && !subMenu.HasDependencies())
                        dis.Dispose();
                }
            }
        }
    }
}
