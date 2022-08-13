using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using SObject = StardewValley.Object;

namespace FixFloorPlacement
{
    internal class ObjectPatcher
    {
        private static IMonitor _monitor;

        public void Patch(Harmony harmony, IMonitor monitor)
        {
            _monitor = monitor;

            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                prefix: new HarmonyMethod(typeof(ObjectPatcher), nameof(SObject_placementAction_Prefix))
            );
        }

        private static bool SObject_placementAction_Prefix(SObject __instance, ref bool __result, GameLocation location, int x, int y, Farmer who = null)
        {
            Vector2 placementTile = new Vector2(x / 64, y / 64);
            __instance.setHealth(10);
            if (who != null)
                __instance.owner.Value = who.UniqueMultiplayerID;
            else
                __instance.owner.Value = Game1.player.UniqueMultiplayerID;

            if (__instance.ParentSheetIndex is 71)  // 楼梯ID
            {
                if (location is MineShaft mineShaft)
                {
                    if (mineShaft.shouldCreateLadderOnThisLevel() && TryCreateLadderDown(mineShaft, placementTile))
                    {
                        MineShaft.numberOfCraftedStairsUsedThisRun++;
                        __result = true;
                        return false;  // 跳过原函数。
                    }
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                }

                __result = false;
                return false;  // 跳过原函数。
            }

            return true;  // 否则执行原函数。
        }

        /// <summary>无递归。</summary>
        private static bool TryCreateLadderDown(MineShaft mineShaft, Vector2 tilePosition)
        {
            if (!mineShaft.isTileOccupied(tilePosition, "ignoreMe")
                && IsTileOnClearAndSolidGround(mineShaft, tilePosition)
                && mineShaft.isTileOccupiedByFarmer(tilePosition) == null
                /*&& __instance.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back") != null
                && __instance.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back").Equals("Stone")*/
                && (mineShaft.doesTileHaveProperty((int)tilePosition.X, (int)tilePosition.Y, "Type", "Back") is "Stone" or "Dirt" or "Wood" || IsSpecialPlaceableTile(mineShaft.getTileIndexAt((int)tilePosition.X, (int)tilePosition.Y, "Back"))))
            {
                LogPlacement(true, tilePosition, mineShaft.mapPath.Value);
                mineShaft.createLadderAt(tilePosition);
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

            if (map.GetLayer("Buildings").Tiles[(int)v.X, (int)v.Y] != null  // 改成只检测Buildings层，因为Buildings层才是真正的墙壁（指地图边界）
                /*|| map.GetLayer("Front").Tiles[(int)v.X, (int)v.Y] != null*/)           // 而Front层仅作装饰用，无实际用途。如果也检测的话，可能会多算。
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
            return tileIndex is 1 or 2 or 3 or 17 or 18 or 19 or 33 or 34 or 35  // 3x3的卵石地块
                || tileIndex is 149 or 150 or 151 or 152;                        // 4个普通地块和泥土地块的交界处
        }

        private static void LogPlacement(bool success, Vector2 tilePosition, string location)
        {
            string successPrefix = success ? "成功" : "未能";
            _monitor.Log($"{successPrefix}放置楼梯！位置：({tilePosition.X}, {tilePosition.Y})；地点：{location}。");
        }
    }
}
