using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace PhoneTravelingCart.Framework.Patchers
{
    internal class Game1Patcher
    {
        public void Patch(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.ShowTelephoneMenu)),
                prefix: new HarmonyMethod(typeof(Game1Patcher), nameof(Game1_ShowTelephoneMenu_Prefix))
            );
        }

        private static bool Game1_ShowTelephoneMenu_Prefix()
        {
            Game1.playSound("openBox");
            var responses = new List<Response>();
            responses.Add(new Response("Carpenter", Game1.getCharacterFromName("Robin").displayName));
            responses.Add(new Response("Blacksmith", Game1.getCharacterFromName("Clint").displayName));
            responses.Add(new Response("SeedShop", Game1.getCharacterFromName("Pierre").displayName));
            responses.Add(new Response("AnimalShop", Game1.getCharacterFromName("Marnie").displayName));
            responses.Add(new Response("Saloon", Game1.getCharacterFromName("Gus").displayName));
            if (Game1.player.mailReceived.Contains("Gil_Telephone"))
                responses.Add(new Response("AdventureGuild", Game1.getCharacterFromName("Marlon").displayName));

            // 猪车
            if (Game1.dayOfMonth % 7 % 5 == 0  // 礼拜五或礼拜天
                && Game1.timeOfDay < 2000)     // 晚上8点之前
                responses.Add(new Response("Traveler", I18n.TravelingMerchantName()));

            responses.Add(new Response("HangUp", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
            Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectNumber"), responses.ToArray(), "telephone");

            return false;
        }
    }
}
