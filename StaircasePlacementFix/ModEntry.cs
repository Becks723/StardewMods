#if NET452
using Harmony;
#elif NET5_0_OR_GREATER
using HarmonyLib;
#endif
using StardewModdingAPI;

namespace StaircasePlacementFix
{
    internal class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            var harmony =
#if NET452
            HarmonyInstance.Create
#elif NET5_0_OR_GREATER
            new Harmony
#endif
            (this.ModManifest.UniqueID);
            {
                new MainPatcher()
                    .Patch(harmony, this.Monitor);
            }
        }
    }
}
