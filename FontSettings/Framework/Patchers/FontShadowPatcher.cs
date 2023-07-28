using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
        private static Func<Color> _textShadowColorOverride;

        private static ModConfig _config;
        private static ModConfigWatcher _configWatcher;

        public static event EventHandler Game1textShadowColorAssigned;

        public FontShadowPatcher(ModConfig config, ModConfigWatcher configWatcher)
        {
            _config = config;
            _configWatcher = configWatcher;
        }

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.drawTextWithShadow), new[] { typeof(SpriteBatch), typeof(string), typeof(SpriteFont), typeof(Vector2), typeof(Color), typeof(float), typeof(float), typeof(int), typeof(int), typeof(float), typeof(int) }),
                prefix: new HarmonyMethod(typeof(FontShadowPatcher), nameof(Utility_drawTextWithShadow_Prefix)),
                transpiler: new HarmonyMethod(typeof(FontShadowPatcher), nameof(Utility_drawTextWithShadow_Transpiler))
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
                original: AccessTools.Method(typeof(Game1), "CleanupReturningToTitle"),
                postfix: new HarmonyMethod(typeof(FontShadowPatcher), nameof(Game1_CleanupReturningToTitle_Postfix))
            );
            Game1textShadowColorAssigned += this.OnGame1textShadowColorAssigned;
            _configWatcher.TextShadowToggled += this.OnTextShadowToggled;
            _configWatcher.ShadowColorGame1Changed += this.OnShadowColorGame1Changed;
        }

        private void OnGame1textShadowColorAssigned(object sender, EventArgs e)
        {
            Game1.textShadowColor = _textShadowColorOverride();
        }

        private void OnTextShadowToggled(object sender, EventArgs e)
        {
            this.SetOverrideTextShadowColor(_config.DisableTextShadow
                ? Color.Transparent
                : _config.ShadowColorGame1);
        }

        private void OnShadowColorGame1Changed(object sender, EventArgs e)
        {
            this.SetOverrideTextShadowColor(_config.DisableTextShadow
                ? Color.Transparent
                : _config.ShadowColorGame1);
        }

        public void SetOverrideTextShadowColor(Color textShadowColor)
        {
            _textShadowColorOverride = () => textShadowColor;

            RaiseGame1textShadowColorAssigned(EventArgs.Empty);
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

        private static IEnumerable<CodeInstruction> Utility_drawTextWithShadow_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var oldInstructions = instructions.ToArray();
            int slide = 0;
            for (int i = 0; i < oldInstructions.Length; i++)
            {
                if (oldInstructions[i + slide].opcode == OpCodes.Ldc_I4
                    && oldInstructions[i + slide].operand is 221
                    && oldInstructions[i + slide + 1].opcode == OpCodes.Ldc_I4
                    && oldInstructions[i + slide + 1].operand is 148
                    && oldInstructions[i + slide + 2].opcode == OpCodes.Ldc_I4_S
                    && oldInstructions[i + slide + 2].operand is (sbyte)84  // 这里不是int，而是sbyte，很奇怪
                    && oldInstructions[i + slide + 3].opcode == OpCodes.Newobj
                    && oldInstructions[i + slide + 3].operand is ConstructorInfo { Name: ".ctor" })
                {
                    if (slide == 0)
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(FontShadowPatcher), nameof(PatchedShadowColor)));

                    --slide;
                    if (slide == -4)
                        slide = 0;
                }

                else
                    yield return oldInstructions[i];
            }
        }

        private static Color PatchedShadowColor()
        {
            return _config.ShadowColorUtility;
        }

        private static void Game1_CleanupReturningToTitle_Postfix()
        {
            RaiseGame1textShadowColorAssigned(EventArgs.Empty);
        }

        private static void RaiseGame1textShadowColorAssigned(EventArgs e)
        {
            Game1textShadowColorAssigned?.Invoke(null, e);
        }
    }
}
