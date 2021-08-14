using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace JunimoStudio
{
    internal static class Textures
    {
        public static Texture2D ExpandedCursors { get; private set; }

        public static Texture2D NoteBlock { get; private set; }

        public static Texture2D TuningStick { get; private set; }

        public static void LoadAll(IContentHelper content)
        {
            ExpandedCursors = content.Load<Texture2D>("assets/cursors.png");
            NoteBlock = content.Load<Texture2D>("assets/note block.png");
            TuningStick = content.Load<Texture2D>("assets/tuning stick.png");
        }
    }
}
