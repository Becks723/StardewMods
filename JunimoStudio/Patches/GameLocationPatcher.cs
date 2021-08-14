using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;
using System;
using System.Linq;
using HarmonyLib;

namespace JunimoStudio.Patches
{
    internal static class GameLocationPatcher
    {
        private static IMonitor _monitor;

        private static Func<bool> _enableTracks;

        public static void Patch(Harmony harmony, IMonitor monitor, Func<bool> enableTracks)
        {
            _monitor = monitor;
            _enableTracks = enableTracks;

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation)),
                postfix: new HarmonyMethod(typeof(GameLocationPatcher), nameof(Update_Postfix)));
        }

        private static void Update_Postfix(GameLocation __instance)
        {
            // 使得轨道上的音符块能同步播放。
            try
            {
                foreach (Farmer farmer in __instance.farmers)
                {
                    Vector2 playerPos = farmer.getTileLocation();

                    bool horizontal = true;
                    bool canPatchHere = 
                        _enableTracks() 
                        && TrackManager.IsTrackHere(__instance, playerPos, out horizontal);

                    if (canPatchHere)
                    {
                        var targetObjects =
                            from SObject obj in __instance.objects.Values
                            where
                            (horizontal
                            ? obj.TileLocation.X == playerPos.X
                            : obj.TileLocation.Y == playerPos.Y) // 条件1
                            && obj.name != "Obelisk" // 条件2
                            && Character.AdjacentTilesOffsets.All(off => playerPos + off != obj.TileLocation) // 条件3
                            select obj;

                        foreach (SObject obj in targetObjects)
                            obj.farmerAdjacentAction(__instance);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"An Error occurs in the {nameof(Update_Postfix)}:\n"
                            + $"{ex}", LogLevel.Error);
            }
        }
    }
}
