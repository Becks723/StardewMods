using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework.Patchers
{
    internal class TextColorPatcher
    {
        private static Func<Color> _overrideGame1textColor;

        private static ModConfig _config;
        private static ModConfigWatcher _configWatcher;

        public static event EventHandler Game1textColorAssigned;

        public TextColorPatcher(ModConfig config, ModConfigWatcher configWatcher)
        {
            _config = config;
            _configWatcher = configWatcher;
        }

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), "CleanupReturningToTitle"),
                postfix: new HarmonyMethod(typeof(TextColorPatcher), nameof(Game1_CleanupReturningToTitle_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), nameof(SpriteText.getColorFromIndex)),
                postfix: new HarmonyMethod(typeof(TextColorPatcher), nameof(SpriteText_getColorFromIndex_Postfix))
            );
            Game1textColorAssigned += this.OnGame1textColorAssigned;
            _configWatcher.TextColorChanged += this.OnTextColorChanged;
        }

        private static void Game1_CleanupReturningToTitle_Postfix()
        {
            RaiseGame1textColorAssigned(EventArgs.Empty);
        }

        private static void SpriteText_getColorFromIndex_Postfix(int index, ref Color __result)
        {
            if (index == -1 && (_config.EnableLatinDialogueFont || !LocalizedContentManager.CurrentLanguageLatin))
                __result = _config.TextColorDialogue;
        }

        private void OnGame1textColorAssigned(object sender, EventArgs e)
        {
            Game1.textColor = _overrideGame1textColor();
        }

        private void OnTextColorChanged(object sender, EventArgs e)
        {
            this.SetOverrideGame1textColor(_config.TextColor);
        }

        private void SetOverrideGame1textColor(Color color)
        {
            _overrideGame1textColor = () => color;

            RaiseGame1textColorAssigned(EventArgs.Empty);
        }

        private static void RaiseGame1textColorAssigned(EventArgs e)
        {
            Game1textColorAssigned?.Invoke(null, e);
        }
    }
}
