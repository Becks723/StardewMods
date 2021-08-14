using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Menus
{
    /// <summary>A set of cursor icons containing both original ones and expanded.</summary>
    internal enum Cursors
    {
        /// <summary>A stardard cursor.</summary>
        Arrow = 0,

        /// <summary>A sandglass like cursor. Never appear in original game.</summary>
        Busy = 1,

        /// <summary>
        /// A cursor showing up as a hand (white glove).
        /// Used when an action is available at hovering point.
        /// </summary>
        Hand = 2,

        /// <summary>
        /// A normal cursor with a giftbox at right-bottom corner.
        /// Used when hovering on NPC to send him/her a gift.
        /// </summary>
        Gift = 3,

        /// <summary>
        /// A normal cursor with a text bubble at right-bottom corner.
        /// Used when hovering on NPC for a dialogue.
        /// </summary>
        Dialogue = 4,

        /// <summary>
        /// A cursor showing up as a magnifying glass.
        /// Used when hovering on stuffs for detailed information in NPC's room.
        /// </summary>
        Zoom = 5,

        /// <summary>
        /// A normal cursor with a green plus at right-bottom corner.
        /// Used when hovering on something on the ground (e.g., crop) can be picked up by player.
        /// </summary>
        Grab = 6,

        /// <summary>A normal cursor with a heart at right-bottom corner. Never appear in original game.</summary>
        Heart = 7,

        /// <summary>A stardard cursor which replaces the <see cref="Arrow"/> one in gamepad mode.</summary>
        Gamepad_Arrow = 44,

        /// <summary>A button in gamepad mode. Never appear in original game.</summary>
        Gamepad_A = 45,

        /// <summary>X button in gamepad mode. Never appear in original game.</summary>
        Gamepad_X = 46,

        /// <summary>B button in gamepad mode. Never appear in original game.</summary>
        Gamepad_B = 47,

        /// <summary>Y button in gamepad mode. Never appear in original game.</summary>
        Gamepad_Y = 48,

        Move,
        Size1,
        Size2,
        Size3,
        Size4,
    }
}
