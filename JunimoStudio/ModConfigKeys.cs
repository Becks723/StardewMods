using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace JunimoStudio
{
    internal class ModConfigKeys
    {
        public KeybindList Undo { get; set; } = KeybindList.Parse($"{SButton.LeftControl} + {SButton.Z}");

        public KeybindList Redo { get; set; } = KeybindList.Parse($"{SButton.LeftControl} + {SButton.Y}");

        public KeybindList Cut { get; set; } = KeybindList.Parse($"{SButton.LeftControl} + {SButton.X}");

        public KeybindList Copy { get; set; } = KeybindList.Parse($"{SButton.LeftControl} + {SButton.C}");

        public KeybindList Paste { get; set; } = KeybindList.Parse($"{SButton.LeftControl} + {SButton.V}");

        public KeybindList Delete { get; set; } = KeybindList.Parse($"{SButton.Delete}");

        public KeybindList SelectAll { get; set; } = KeybindList.Parse($"{SButton.LeftControl} + {SButton.A}");

    }
}
