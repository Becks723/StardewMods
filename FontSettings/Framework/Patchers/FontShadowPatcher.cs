using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

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
    }
}
