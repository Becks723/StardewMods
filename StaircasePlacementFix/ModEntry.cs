using HarmonyLib;
using StardewModdingAPI;

namespace StaircasePlacementFix
{
    internal class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Harmony harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new MainPatcher()
                    .Patch(harmony, this.Monitor);
            }
        }
    }
}
