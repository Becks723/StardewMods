using HarmonyLib;
using StardewModdingAPI;

namespace FixFloorPlacement
{
    internal class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Harmony harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new ObjectPatcher()
                    .Patch(harmony, this.Monitor);
            }
        }
    }
}
