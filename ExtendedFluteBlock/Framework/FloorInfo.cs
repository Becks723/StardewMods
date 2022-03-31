using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluteBlockExtension.Framework.Models;
using StardewValley;
using static FluteBlockExtension.Framework.Models.FloorData;

namespace FluteBlockExtension.Framework
{
    /// <summary>Queries common information for a <see cref="FloorData"/>.</summary>
    internal class FloorInfo
    {
        public string DisplayName { get; set; }

        public string Description { get; set; }


        private static readonly Dictionary<FloorData, FloorInfo> _cachedFloors = new();

        static FloorInfo()
        {
            LocalizedContentManager.OnLanguageChange += _ => OnLocaleChanged();

            OnLocaleChanged();
        }

        public static string GetDisplayName(FloorData floor!!)
        {
            // common.
            if (_cachedFloors.TryGetValue(floor, out FloorInfo info))
            {
                return info.DisplayName;
            }

            // empty.
            if (floor.IsEmptyFloor())
            {
                return I18n.Floor_Empty();
            }

            // unknown.
            return I18n.Floor_UnknownFloor_Name(floor.WhichFloor);
        }

        private static int MapSheetIndex(int whichFloor)
        {
            return whichFloor switch
            {
                0 => 328,
                1 => 329,
                2 => 331,
                3 => 333,
                4 => 401,
                5 => 407,
                6 => 405,
                7 => 409,
                8 => 411,
                9 => 415,
                10 => 293,
                11 => 840,
                12 => 841,
                _ => throw new NotSupportedException($"Unknown floor number: {whichFloor}.")
            };
        }

        private static void OnLocaleChanged()
        {
            static string GameFloorName(FloorData gameFloor)
            {
                int whichFloor = gameFloor.WhichFloor.Value;
                int sheetIndex = MapSheetIndex(whichFloor);
                return Game1.objectInformation[sheetIndex].Split('/')[4];
            }

#pragma warning disable format
            _cachedFloors[WoodFloor]            = new() { DisplayName = GameFloorName(WoodFloor) };
            _cachedFloors[StoneFloor]           = new() { DisplayName = GameFloorName(StoneFloor) };
            _cachedFloors[WeatheredFloor]       = new() { DisplayName = GameFloorName(WeatheredFloor) };
            _cachedFloors[CrystalFloor]         = new() { DisplayName = GameFloorName(CrystalFloor) };
            _cachedFloors[StrawFloor]           = new() { DisplayName = GameFloorName(StrawFloor) };
            _cachedFloors[GravelPath]           = new() { DisplayName = GameFloorName(GravelPath) };
            _cachedFloors[WoodPath]             = new() { DisplayName = GameFloorName(WoodPath) };
            _cachedFloors[CrystalPath]          = new() { DisplayName = GameFloorName(CrystalPath) };
            _cachedFloors[CobblestonePath]      = new() { DisplayName = GameFloorName(CobblestonePath) };
            _cachedFloors[SteppingStonePath]    = new() { DisplayName = GameFloorName(SteppingStonePath) };
            _cachedFloors[BrickFloor]           = new() { DisplayName = GameFloorName(BrickFloor) };
            _cachedFloors[RusticPlankFloor]     = new() { DisplayName = GameFloorName(RusticPlankFloor) };
            _cachedFloors[StoneWalkwayFloor]    = new() { DisplayName = GameFloorName(StoneWalkwayFloor) };
#pragma warning restore format

            _cachedFloors[NonFloor] = new() { DisplayName = I18n.Floor_NonFloor_Name() };
        }
    }
}