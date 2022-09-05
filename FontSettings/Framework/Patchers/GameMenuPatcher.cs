using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Menus;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace FontSettings.Framework.Patchers
{
    internal class GameMenuPatcher
    {
        private const string _fontTabName = "font";
        private static readonly Lazy<Texture2D> _fontTab = new(() => Textures.FontTab);

        private static IModHelper _helper;
        private static ModConfig _config;
        private static FontManager _fontManager;
        private static GameFontChanger _fontChanger;
        private static FontPresetManager _presetManager;
        private static Action<ModConfig> _saveConfig;

        public void AddFontSettingsPage(IModHelper helper, Harmony harmony, ModConfig config, FontManager fontManager, GameFontChanger fontChanger, FontPresetManager presetManager, Action<ModConfig> saveConfig)
        {
            _helper = helper;
            _config = config;
            _fontManager = fontManager;
            _fontChanger = fontChanger;
            _presetManager = presetManager;
            _saveConfig = saveConfig;

            harmony.Patch(
                original: AccessTools.Constructor(typeof(GameMenu), new Type[] { typeof(bool) }),
                postfix: new HarmonyMethod(typeof(GameMenuPatcher), nameof(GameMenuPatcher.GameMenu_ctor_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameMenu), nameof(GameMenu.draw), new Type[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(GameMenuPatcher), nameof(GameMenuPatcher.GameMenu_draw_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameMenu), nameof(GameMenu.getTabNumberFromName)),
                postfix: new HarmonyMethod(typeof(GameMenuPatcher), nameof(GameMenuPatcher.GameMenu_getTabNumberFromName_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), "set_activeClickableMenu"),
                prefix: new HarmonyMethod(typeof(GameMenuPatcher), nameof(GameMenuPatcher.Game1_set_activeClickableMenu_Prefix))
            );
        }

        private static void GameMenu_ctor_Postfix(GameMenu __instance)
        {
            // 如果禁用了游戏菜单内设置字体，直接返回。
            if (!_config.FontSettingsInGameMenu)
                return;

            __instance.tabs.Add(new ClickableComponent(new Rectangle(__instance.xPositionOnScreen + 576, __instance.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), _fontTabName, I18n.OptionsPage_FontHeader())
            {
                myID = 12348,
                downNeighborID = 8,
                leftNeighborID = 12347,
                tryDefaultIfNoDownNeighborExists = true,
                fullyImmutable = true
            });
            __instance.pages.Add(new FontSettingsPage(_config, _fontManager, _fontChanger, _presetManager, _saveConfig,
                __instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.width, __instance.height + 64)
                .FixConflictWithStarrySkyInterface(_helper.ModRegistry)
            );
        }

        private static void GameMenu_getTabNumberFromName_Postfix(string name, ref int __result)
        {
            if (name == _fontTabName)
                __result = 8;
        }

        private static bool GameMenu_draw_Prefix(GameMenu __instance, SpriteBatch b)
        {            
            // 如果禁用了游戏菜单内设置字体，直接返回，执行原函数。
            if (!_config.FontSettingsInGameMenu)
                return true;

            if (!__instance.invisible)
            {
                if (!Game1.options.showMenuBackground)
                    b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);

                Game1.drawDialogueBox(__instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.pages[__instance.currentTab].width, __instance.pages[__instance.currentTab].height, speaker: false, drawOnlyBox: true);
                b.End();
                b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                foreach (ClickableComponent tab in __instance.tabs)
                {
                    int? sheetIndex = null;
                    bool isFontTab = false;
                    switch (tab.name)
                    {
                        case "inventory":
                            sheetIndex = 0;
                            break;
                        case "skills":
                            sheetIndex = 1;
                            break;
                        case "social":
                            sheetIndex = 2;
                            break;
                        case "map":
                            sheetIndex = 3;
                            break;
                        case "crafting":
                            sheetIndex = 4;
                            break;
                        case "catalogue":
                            sheetIndex = 7;
                            break;
                        case "collections":
                            sheetIndex = 5;
                            break;
                        case "options":
                            sheetIndex = 6;
                            break;
                        case "exit":
                            sheetIndex = 7;
                            break;
                        case "coop":
                            sheetIndex = 1;
                            break;
                        case _fontTabName:
                            isFontTab = true;
                            break;
                    }

                    if (sheetIndex != null)
                    {
                        b.Draw(Game1.mouseCursors, new Vector2(tab.bounds.X, tab.bounds.Y + (__instance.currentTab == __instance.getTabNumberFromName(tab.name) ? 8 : 0)), new Rectangle(sheetIndex.Value * 16, 368, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
                        if (tab.name.Equals("skills"))
                            Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2(tab.bounds.X + 8, tab.bounds.Y + 12 + (__instance.currentTab == __instance.getTabNumberFromName(tab.name) ? 8 : 0)), 0.00011f, 3f, 2, Game1.player);
                    }

                    if (isFontTab)
                    {
                        // 框
                        b.Draw(
                            Game1.mouseCursors,
                            new Vector2(tab.bounds.X, tab.bounds.Y + (__instance.currentTab == __instance.getTabNumberFromName(tab.name) ? 8 : 0)),
                            new Rectangle(16, 368, 16, 16),
                            Color.White,
                            0f,
                            Vector2.Zero,
                            4f,
                            SpriteEffects.None,
                            0.0001f);

                        // 内容
                        const int index = 3;
                        b.Draw(
                            _fontTab.Value,
                            new Vector2(tab.bounds.X, tab.bounds.Y + (__instance.currentTab == __instance.getTabNumberFromName(tab.name) ? 8 : 0)),
                            new Rectangle(index * 16, 0, 16, 16),
                            Color.White,
                            0f,
                            Vector2.Zero,
                            4f,
                            SpriteEffects.None,
                            0.0001f);
                    }
                }

                b.End();
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                __instance.pages[__instance.currentTab].draw(b);
                if (!__instance.hoverText.Equals(""))
                    IClickableMenu.drawHoverText(b, __instance.hoverText, Game1.smallFont);
            }
            else
                __instance.pages[__instance.currentTab].draw(b);

            if (!GameMenu.forcePreventClose && __instance.pages[__instance.currentTab].shouldDrawCloseButton())
                __instance.upperRightCloseButton?.draw(b);

            if ((!Game1.options.SnappyMenus || __instance.pages[__instance.currentTab] is not CollectionsPage || (__instance.pages[__instance.currentTab] as CollectionsPage).letterviewerSubMenu == null) && !Game1.options.hardwareCursor)
                __instance.drawMouse(b, ignore_transparency: true);

            return false;
        }

        private static void Game1_set_activeClickableMenu_Prefix()
        {
            if (Game1.activeClickableMenu is GameMenu gameMenu)
            {
                foreach (IClickableMenu page in gameMenu.pages)
                    if (page is IDisposable disposable)
                        disposable.Dispose();
            }
        }
    }
}
