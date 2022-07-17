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
    internal class GameMenuAdder
    {
        private const string _fontTabName = "font";
        private static Lazy<Texture2D> _fontTab;

        private static ModConfig _config;
        private static RuntimeFontManager _fontManager;
        private static GameFontChanger _fontChanger;
        private static Action<ModConfig> _saveConfig;

        private readonly IModHelper _helper;
        private readonly Harmony _harmony;

        public GameMenuAdder(IModHelper helper, Harmony harmony)
        {
            this._helper = helper;
            this._harmony = harmony;
            _fontTab = new(() => this.LoadFontTab(helper));
        }

        public void AddFontSettingsPage(ModConfig config, RuntimeFontManager fontManager, GameFontChanger fontChanger, Action<ModConfig> saveConfig)
        {
            _config = config;
            _fontManager = fontManager;
            _fontChanger = fontChanger;
            _saveConfig = saveConfig;

            var harmony = this._harmony;
            harmony.Patch(
                original: AccessTools.Constructor(typeof(GameMenu), new Type[] { typeof(bool) }),
                postfix: new HarmonyMethod(typeof(GameMenuAdder), nameof(GameMenuAdder.GameMenu_ctor_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameMenu), nameof(GameMenu.draw), new Type[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(GameMenuAdder), nameof(GameMenuAdder.GameMenu_draw_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameMenu), nameof(GameMenu.getTabNumberFromName)),
                postfix: new HarmonyMethod(typeof(GameMenuAdder), nameof(GameMenuAdder.GameMenu_getTabNumberFromName_Postfix))
            );
        }

        private static void GameMenu_ctor_Postfix(GameMenu __instance)
        {
            __instance.tabs.Add(new ClickableComponent(new Rectangle(__instance.xPositionOnScreen + 576, __instance.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), _fontTabName, I18n.OptionsPage_FontHeader())
            {
                myID = 12348,
                downNeighborID = 8,
                leftNeighborID = 12347,
                tryDefaultIfNoDownNeighborExists = true,
                fullyImmutable = true
            });
            __instance.pages.Add(new FontSettingsPage(_config, _fontManager, _fontChanger, _saveConfig,
                __instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.width, __instance.height));
        }

        private static void GameMenu_getTabNumberFromName_Postfix(string name, ref int __result)
        {
            if (name == _fontTabName)
                __result = 8;
        }

        private static bool GameMenu_draw_Prefix(GameMenu __instance, SpriteBatch b)
        {
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
                        const int index = 2;
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

        private Texture2D LoadFontTab(IModHelper helper)
        {
            return helper.ModContent.Load<Texture2D>(@"assets/font-tab.png");
        }
    }
}
