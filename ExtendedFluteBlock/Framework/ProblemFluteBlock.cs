using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace FluteBlockExtension.Framework
{
    /// <summary>
    /// Problem is caused by mismatch between game pitch (preservedParentSheetIndex) and mod extended pitch (extraPitch).
    /// </summary>
    /// <remarks>
    /// e.g.1 preservedParentSheetIndex: 100; extraPitch: -100. It's pitch is ambiguous between -100 and 100.<br/>
    /// e.g.2 preservedParentSheetIndex: 100; extraPitch: 1000. It's pitch is ambiguous between 3300 and 100.<br/>
    /// Two fix schemes:<br/>
    /// 1. apply game pitch.    Set extraPitch to 0.                        Then now pitch is preservedParentSheetIndex.<br/>
    /// 2. apply extra pitch.   Set preservedParentSheetIndex to 0 or 2300. Then now pitch is preservedParentSheetIndex + extraPitch.<br/>
    /// </remarks>
    /// <param name="Core">The core flute block.</param>
    /// <param name="TilePosition">Flute block's tile pos.</param>
    /// <param name="Location">Flute block's location.</param>
    internal record ProblemFluteBlock(SObject Core, Vector2 TilePosition, GameLocation Location);
}
