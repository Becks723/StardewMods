using System;

namespace FluteBlockExtension.Framework.Models
{
    /// <summary>Model for a floor.</summary>
    /// <remarks>Note this's a record.</remarks>
    internal record FloorData
    {
        public static readonly FloorData NonFloor = new FloorData() { WhichFloor = null };

        /// <summary>sheetindex = 328 <see href="https://stardewvalleywiki.com/Wood_Floor"/></summary>
        public static readonly FloorData WoodFloor = new FloorData() { WhichFloor = 0 };

        /// <summary>sheetindex = 329 <see href="https://stardewvalleywiki.com/Stone_Floor"/></summary>
        public static readonly FloorData StoneFloor = new FloorData() { WhichFloor = 1 };

        /// <summary>sheetindex = 331 <see href="https://stardewvalleywiki.com/Weathered_Floor"/></summary>
        public static readonly FloorData WeatheredFloor = new FloorData() { WhichFloor = 2 };

        /// <summary>sheetindex = 333 <see href="https://stardewvalleywiki.com/Crystal_Floor"/></summary>
        public static readonly FloorData CrystalFloor = new FloorData() { WhichFloor = 3 };

        /// <summary>sheetindex = 401 <see href="https://stardewvalleywiki.com/Straw_Floor"/></summary>
        public static readonly FloorData StrawFloor = new FloorData() { WhichFloor = 4 };

        /// <summary>sheetindex = 407 <see href="https://stardewvalleywiki.com/Gravel_Path"/></summary>
        public static readonly FloorData GravelPath = new FloorData() { WhichFloor = 5 };

        /// <summary>sheetindex = 405 <see href="https://stardewvalleywiki.com/Wood_Path"/></summary>
        public static readonly FloorData WoodPath = new FloorData() { WhichFloor = 6 };

        /// <summary>sheetindex = 409 <see href="https://stardewvalleywiki.com/Crystal_Path"/></summary>
        public static readonly FloorData CrystalPath = new FloorData() { WhichFloor = 7 };

        /// <summary>sheetindex = 411 <see href="https://stardewvalleywiki.com/Cobblestone_Path"/></summary>
        public static readonly FloorData CobblestonePath = new FloorData() { WhichFloor = 8 };

        /// <summary>sheetindex = 415 <see href="https://stardewvalleywiki.com/Stepping_Stone_Path"/></summary>
        public static readonly FloorData SteppingStonePath = new FloorData() { WhichFloor = 9 };

        /// <summary>sheetindex = 293 <see href="https://stardewvalleywiki.com/Brick_Floor"/></summary>
        public static readonly FloorData BrickFloor = new FloorData() { WhichFloor = 10 };

        /// <summary>sheetindex = 840 <see href="https://stardewvalleywiki.com/Rustic_Plank_Floor"/></summary>
        public static readonly FloorData RusticPlankFloor = new FloorData() { WhichFloor = 11 };

        /// <summary>sheetindex = 841 <see href="https://stardewvalleywiki.com/Stone_Walkway_Floor"/></summary>
        public static readonly FloorData StoneWalkwayFloor = new FloorData() { WhichFloor = 12 };

        public static FloorData From(int? whichFloor)
        {
            return whichFloor switch
            {
                null => NonFloor,
                0 => WoodFloor,
                1 => StoneFloor,
                2 => WeatheredFloor,
                3 => CrystalFloor,
                4 => StrawFloor,
                5 => GravelPath,
                6 => WoodPath,
                7 => CrystalPath,
                8 => CobblestonePath,
                9 => SteppingStonePath,
                10 => BrickFloor,
                11 => RusticPlankFloor,
                12 => StoneWalkwayFloor,
                _ => throw new NotSupportedException($"Unknown floor number: {whichFloor}.")
            };
        }

        /// <summary>Floor number.</summary>
        /// <remarks>See <see cref="StardewValley.TerrainFeatures.Flooring.whichFloor"/>.</remarks>
        public int? WhichFloor { get; set; }
    }
}
