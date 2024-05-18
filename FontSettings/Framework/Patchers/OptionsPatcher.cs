using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontPatching;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace FontSettings.Framework.Patchers
{
    internal class OptionsPatcher
    {
        private static ModConfig _config;
        private static Action<float> _setOverridePixelZoom;
        private static float _pixelZoomCache;

        public OptionsPatcher(ModConfig config, MainFontPatcher fontPatcher, Action<float> setOverridePixelZoom)
        {
            _config = config;
            _setOverridePixelZoom = setOverridePixelZoom;
            fontPatcher.FontPixelZoomOverride += this.OnUserFontPixelZoomOverride;
        }

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Options), nameof(Options.loadChineseFonts)),
                postfix: this.HarmonyMethod(nameof(Options_loadChineseFonts_Postfix))
            );
        }

        private static void Options_loadChineseFonts_Postfix(Options __instance)
        {
            // 启用平滑字体后，缩放恒为1x。
            if (__instance.useChineseSmoothFont)
            {
                _setOverridePixelZoom(1f);
            }

            // 禁用平话字体后，缩放即为之前用户自定义的值。
            else
            {
                _setOverridePixelZoom(_pixelZoomCache);
            }
        }

        private void OnUserFontPixelZoomOverride(object sender, FontPixelZoomOverrideEventArgs e)
        {
            // 每次用户设置完自定义字体后，更新对话字体缩放。
            _pixelZoomCache = e.PixelZoom;
        }

        private HarmonyMethod HarmonyMethod(string methodName)
            => new HarmonyMethod(this.GetType(), methodName);
    }
}
