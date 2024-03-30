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
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace FontSettings.Framework.Patchers
{
    internal class FontShadowPatcher
    {
        private static Func<Color> _textShadowColorOverride;
        private static Func<Color> _textShadowDarkerColorOverride;
        private static Func<float> _shadowAlphaOverride;

        private static ModConfig _config;
        private static ModConfigWatcher _configWatcher;

        public static event EventHandler Game1textShadowColorAssigned;
        public static event EventHandler Game1textShadowDarkerColorAssigned;
        public static event EventHandler SpriteTextshadowAlphaAssigned;

        public FontShadowPatcher(ModConfig config, ModConfigWatcher configWatcher)
        {
            _config = config;
            _configWatcher = configWatcher;
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
                original: AccessTools.Method(typeof(Game1), "CleanupReturningToTitle"),
                postfix: new HarmonyMethod(typeof(FontShadowPatcher), nameof(Game1_CleanupReturningToTitle_Postfix))
            );
            Game1textShadowColorAssigned += this.OnGame1textShadowColorAssigned;
            Game1textShadowDarkerColorAssigned += this.OnGame1textShadowDarkerColorAssigned;
            SpriteTextshadowAlphaAssigned += this.OnSpriteTextshadowAlphaAssigned;
            _configWatcher.TextShadowToggled += this.OnTextShadowToggled;
            _configWatcher.ShadowColorGame1Changed += this.OnShadowColorGame1Changed;
            _configWatcher.ShadowColorDarkerChanged += this.OnShadowColorDarkerChanged;
        }

        private void OnGame1textShadowColorAssigned(object sender, EventArgs e)
        {
            Game1.textShadowColor = _textShadowColorOverride();
        }

        private void OnGame1textShadowDarkerColorAssigned(object sender, EventArgs e)
        {
            Game1.textShadowDarkerColor = _textShadowDarkerColorOverride();
        }

        private void OnSpriteTextshadowAlphaAssigned(object sender, EventArgs e)
        {
            SpriteText.shadowAlpha = _shadowAlphaOverride();
        }

        private void OnTextShadowToggled(object sender, EventArgs e)
        {
            this.SetOverrideTextShadowColor(_config.DisableTextShadow
                ? Color.Transparent
                : _config.ShadowColorGame1);
            this.SetOverrideTextShadowDarkerColor(_config.DisableTextShadow
                ? Color.Transparent
                : _config.ShadowColorUtility);
            this.SetOverrideShadowAlpha(_config.DisableTextShadow
                ? 0f
                : 0.15f);
        }

        private void OnShadowColorGame1Changed(object sender, EventArgs e)
        {
            this.SetOverrideTextShadowColor(_config.DisableTextShadow
                ? Color.Transparent
                : _config.ShadowColorGame1);
        }

        private void OnShadowColorDarkerChanged(object sender, EventArgs e)
        {
            this.SetOverrideTextShadowDarkerColor(_config.DisableTextShadow
                ? Color.Transparent
                : _config.ShadowColorUtility);
        }

        public void SetOverrideTextShadowColor(Color textShadowColor)
        {
            _textShadowColorOverride = () => textShadowColor;

            RaiseGame1textShadowColorAssigned(EventArgs.Empty);
        }

        public void SetOverrideTextShadowDarkerColor(Color textShadowDarkerColor)
        {
            _textShadowDarkerColorOverride = () => textShadowDarkerColor;

            RaiseGame1textShadowDarkerColorAssigned(EventArgs.Empty);
        }

        public void SetOverrideShadowAlpha(float shadowAlpha)
        {
            _shadowAlphaOverride = () => shadowAlpha;

            RaiseSpriteTextshadowAlphaAssigned(EventArgs.Empty);
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

        private static Color PatchedShadowColor()
        {
            return _config.ShadowColorUtility;
        }

        private static void Game1_CleanupReturningToTitle_Postfix()
        {
            RaiseGame1textShadowColorAssigned(EventArgs.Empty);
            RaiseGame1textShadowDarkerColorAssigned(EventArgs.Empty);
        }

        private static void RaiseGame1textShadowColorAssigned(EventArgs e) => Game1textShadowColorAssigned?.Invoke(null, e);
        private static void RaiseGame1textShadowDarkerColorAssigned(EventArgs e) => Game1textShadowDarkerColorAssigned?.Invoke(null, e);
        private static void RaiseSpriteTextshadowAlphaAssigned(EventArgs e) => SpriteTextshadowAlphaAssigned?.Invoke(null, e);
    }
}
