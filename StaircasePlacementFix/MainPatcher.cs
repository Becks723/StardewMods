#if NET452
using System.Linq;
using Harmony;
#elif NET5_0_OR_GREATER
using HarmonyLib;
#endif
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StaircasePlacementFix
{
    internal class MainPatcher
    {
        private static IMonitor _monitor;

        public void Patch(
#if NET452
            HarmonyInstance
#elif NET5_0_OR_GREATER
            Harmony
#endif
            harmony, IMonitor monitor)
        {
            _monitor = monitor;

            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.recursiveTryToCreateLadderDown)),
                prefix: new HarmonyMethod(typeof(MainPatcher), nameof(MineShaft_recursiveTryToCreateLadderDown_Rewrite))
            );
        }

        private static bool MineShaft_recursiveTryToCreateLadderDown_Rewrite(MineShaft __instance, ref bool __result, Vector2 centerTile, string sound = "hoeHit", int maxIterations = 16)
        {
            __result = TryCreateLadderDown(__instance, centerTile, sound);
            return false;  // 跳过原函数。
        }

        /// <summary>无递归。</summary>
        private static bool TryCreateLadderDown(MineShaft mineShaft, Vector2 tilePosition, string sound = "hoeHit")
        {
            if (!mineShaft.isTileOccupied(tilePosition, "ignoreMe")
                && IsTileOnClearAndSolidGround(mineShaft, tilePosition)
                && mineShaft.isTileOccupiedByFarmer(tilePosition) == null
                /*&& __instance.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back") != null
                && __instance.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back").Equals("Stone")*/
                && (
#if NET452
                    Matches(
                        mineShaft.doesTileHaveProperty((int)tilePosition.X, (int)tilePosition.Y, "Type", "Back"),
                        "Stone", "Dirt", "Wood")
#elif NET5_0_OR_GREATER
                    mineShaft.doesTileHaveProperty((int)tilePosition.X, (int)tilePosition.Y, "Type", "Back") is "Stone" or "Dirt" or "Wood"
#endif
                    || IsSpecialPlaceableTile(mineShaft.getTileIndexAt((int)tilePosition.X, (int)tilePosition.Y, "Back"))))
            {
                LogPlacement(true, tilePosition, mineShaft.mapPath.Value);
                mineShaft.createLadderAt(tilePosition, sound);
                return true;
            }

            LogPlacement(false, tilePosition, mineShaft.mapPath.Value);
            return false;
        }

        private static bool IsTileOnClearAndSolidGround(MineShaft instance, Vector2 v)
        {
            xTile.Map map = instance.map;

            if (map.GetLayer("Back").Tiles[(int)v.X, (int)v.Y] == null)
            {
                return false;
            }

            if (map.GetLayer("Buildings").Tiles[(int)v.X, (int)v.Y] != null     // 改成只检测Buildings层，因为Buildings层才是真正的墙壁（指地图边界）
               /*|| map.GetLayer("Front").Tiles[(int)v.X, (int)v.Y] != null*/)  // 而Front层仅作装饰用，无实际用途。如果也检测的话，可能会多算。
            {
                return false;
            }

            if (instance.getTileIndexAt((int)v.X, (int)v.Y, "Back") == 77)
            {
                return false;
            }

            return true;
        }

        /// <summary>见Maps/Mines/mine.png，用Tiled地图编辑器打开，显示在Tilesets页，打开Properties菜单，点击每个图块就可以查看<paramref name="tileIndex"/>值。</summary>
        private static bool IsSpecialPlaceableTile(int tileIndex)
        {
            return
#if NET452
                   Matches(
                tileIndex,
                1, 2, 3, 17, 18, 19, 33, 34, 35,
                149, 150, 151, 152);
#elif NET5_0_OR_GREATER
                   tileIndex is
                1 or 2 or 3 or 17 or 18 or 19 or 33 or 34 or 35  // 3x3的卵石地块
                or 149 or 150 or 151 or 152;                     // 4个普通地块和泥土地块的交界处
#endif
        }

        private static void LogPlacement(bool success, Vector2 tilePosition, string location)
        {
            string successPrefix = success ? "成功" : "未能";
            _monitor.Log($"{successPrefix}放置楼梯！位置：({tilePosition.X}, {tilePosition.Y})；地点：{location}。");
        }

#if NET452
        private static bool Matches<T>(T value, params T[] predicates)
        {
            return predicates.Contains(value);
        }
#endif
    }
}
