using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace FontSettings.Framework.Patchers
{
    internal class GameMenuResizeHandler
    {
        private static IMonitor _monitor;

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            _monitor = monitor;

            harmony.Patch(
                original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.gameWindowSizeChanged)),
                prefix: new HarmonyMethod(typeof(GameMenuResizeHandler), nameof(IClickableMenu_gameWindowSizeChanged_Prefix)),
                postfix: new HarmonyMethod(typeof(GameMenuResizeHandler), nameof(IClickableMenu_gameWindowSizeChanged_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.SetWindowSize)),
                transpiler: new HarmonyMethod(typeof(GameMenuResizeHandler), nameof(Game1_SetWindowSize_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameMenu), nameof(GameMenu.changeTab)),
                prefix: new HarmonyMethod(typeof(GameMenuResizeHandler), nameof(GameMenuResizeHandler.GameMenu_changeTab_Prefix))
            );
        }

        private static void GameMenu_changeTab_Prefix()
        {
        }

        private static void IClickableMenu_gameWindowSizeChanged_Prefix(IClickableMenu __instance, Rectangle oldBounds, Rectangle newBounds)
        {
            if (__instance is OptionsPage optionsPage)
                optionsPage.preWindowSizeChange();
        }

        private static void IClickableMenu_gameWindowSizeChanged_Postfix(IClickableMenu __instance, Rectangle oldBounds, Rectangle newBounds)
        {
            if (__instance is GameMenu gameMenu)
                gameMenu.pages[gameMenu.currentTab].gameWindowSizeChanged(oldBounds, newBounds);

            else if (__instance is OptionsPage optionsPage)
                optionsPage.postWindowSizeChange();
        }

        private static IEnumerable<CodeInstruction> Game1_SetWindowSize_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            /* 删去了最后一个if段。
             * 删：
             * ...
             * if (activeClickableMenu is GameMenu && !overrideGameMenuReset)
             * {
             *     ...
             * }
             * ...
             */

            var oldInstructions = instructions.ToArray();
            var result = new List<CodeInstruction>();
            bool isSkipping = false;
            CodeInstruction labelFrom = null;
            for (int i = 0; i < oldInstructions.Length; i++)
            {
                if (i >= 1
                    && oldInstructions[i - 1].opcode == OpCodes.Callvirt
                    && oldInstructions[i - 1].operand is MethodInfo { Name: nameof(IClickableMenu.gameWindowSizeChanged) }
                    && oldInstructions[i].opcode == OpCodes.Call
                    && oldInstructions[i].operand is MethodInfo { Name: "get_activeClickableMenu" }
                    && oldInstructions[i + 1].opcode == OpCodes.Isinst
                    && oldInstructions[i + 1].operand is Type { Name: nameof(GameMenu) }
                    && oldInstructions[i + 3].opcode == OpCodes.Ldsfld
                    && oldInstructions[i + 3].operand is FieldInfo { Name: nameof(Game1.overrideGameMenuReset) })
                {
                    labelFrom = oldInstructions[i];
                    isSkipping = true;
                }

                if (oldInstructions[i].opcode == OpCodes.Call
                    && oldInstructions[i].operand is MethodInfo { Name: nameof(Game1.PopUIMode) })
                {
                    oldInstructions[i].MoveLabelsFrom(labelFrom);
                    isSkipping = false;
                }

                if (isSkipping)
                    continue;
                else
                    result.Add(oldInstructions[i]);
            }

            return result;
        }

        private static void PatchFailed()
        {
            _monitor.Log($"修改代码失败，请联系作者！");
        }
    }
}
