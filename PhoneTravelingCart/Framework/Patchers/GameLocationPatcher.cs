using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace PhoneTravelingCart.Framework.Patchers
{
    internal class GameLocationPatcher
    {
        private static ModConfig _config;

        public GameLocationPatcher(ModConfig config)
        {
            _config = config;
        }

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue), new[] { typeof(string), typeof(Response[]), typeof(string) }),
                prefix: new HarmonyMethod(typeof(GameLocationPatcher), nameof(GameLocation_createQuestionDialogue_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(GameLocationPatcher), nameof(GameLocation_answerDialogueAction_Prefix))
            );
        }

        private static void GameLocation_createQuestionDialogue_Prefix(string dialogKey, ref Response[] answerChoices)
        {
            if (dialogKey is "telephone")
            {
                if (Game1.dayOfMonth % 7 % 5 == 0  // 礼拜五或礼拜天
                    && Game1.timeOfDay < 2000)     // 晚上8点之前
                {
                    var responses = answerChoices.ToList();
                    responses.Add(new Response("Traveler", I18n.TravelingMerchantName()));
                    answerChoices = responses.ToArray();
                }
            }
        }

        private static bool GameLocation_answerDialogueAction_Prefix(string questionAndAnswer, string[] questionParams)
        {
            if (questionAndAnswer == "telephone_Traveler")
            {
                playShopPhoneNumberSounds(questionAndAnswer);
                Game1.player.freezePause = 4950;
                DelayedAction.functionAfterDelay(delegate
                {
                    Game1.activeClickableMenu = new ShopMenu(Utility.getTravelingMerchantStock((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed)), 0, "Traveler", Utility.onTravelingMerchantShopPurchase)
                    {
                        readOnly = !_config.RemotePurchase
                    };
                }, 4950);
                return false;
            }

            return true;
        }

        private static void playShopPhoneNumberSounds(string whichShop)
        {
            Random r = new Random(whichShop.GetHashCode());
            DelayedAction.playSoundAfterDelay("telephone_dialtone", 495, null, 1200);
            DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1200, null, 1200 + r.Next(-4, 5) * 100);
            DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1370, null, 1200 + r.Next(-4, 5) * 100);
            DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1600, null, 1200 + r.Next(-4, 5) * 100);
            DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1850, null, 1200 + r.Next(-4, 5) * 100);
            DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2030, null, 1200 + r.Next(-4, 5) * 100);
            DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2250, null, 1200 + r.Next(-4, 5) * 100);
            DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2410, null, 1200 + r.Next(-4, 5) * 100);
            DelayedAction.playSoundAfterDelay("telephone_ringingInEar", 3150);
        }
    }
}
