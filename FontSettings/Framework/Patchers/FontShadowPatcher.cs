using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace FontSettings.Framework.Patchers
{
    internal class FontShadowPatcher
    {
        private static ModConfig _config;

        public FontShadowPatcher(ModConfig config)
        {
            _config = config;
        }

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.drawTextWithShadow), new[] { typeof(SpriteBatch), typeof(string), typeof(SpriteFont), typeof(Vector2), typeof(Color), typeof(float), typeof(float), typeof(int), typeof(int), typeof(float), typeof(int) }),
                prefix: new HarmonyMethod(typeof(FontShadowPatcher), nameof(Utility_drawTextWithShadow_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.drawTextWithShadow), new[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(Vector2), typeof(Color), typeof(float), typeof(float), typeof(int), typeof(int), typeof(float), typeof(int) }),
                prefix: new HarmonyMethod(typeof(FontShadowPatcher), nameof(Utility_drawTextWithShadow_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.drawTextWithColoredShadow)),
                prefix: new HarmonyMethod(typeof(FontShadowPatcher), nameof(Utility_drawTextWithColoredShadow_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(IList<Item>) }),
                prefix: new HarmonyMethod(typeof(FontShadowPatcher), nameof(IClickableMenu_drawHoverText_Prefix)),
                postfix: new HarmonyMethod(typeof(FontShadowPatcher), nameof(IClickableMenu_drawHoverText_Postfix))
            );
        }

        private static void Utility_drawTextWithShadow_Prefix(ref float shadowIntensity)
        {
            if (_config.DisableTextShadow)
                shadowIntensity = 0f;
        }

        private static void Utility_drawTextWithColoredShadow_Prefix(ref Color shadowColor)
        {
            if (_config.DisableTextShadow)
                shadowColor = Color.Transparent;
        }

        private static void IClickableMenu_drawHoverText_Prefix(out Color __state)  // __state里记录了原来的阴影颜色
        {
            __state = Game1.textShadowColor;

            // 在方法开始前，将阴影颜色设置为透明。
            if (_config.DisableTextShadow)
                Game1.textShadowColor = Color.Transparent;
        }

        private static void IClickableMenu_drawHoverText_Postfix(Color __state)
        {
            // 在方法结束后，恢复原来的阴影颜色。
            Game1.textShadowColor = __state;
        }
    }
}
