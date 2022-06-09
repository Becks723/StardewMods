using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework
{
    internal class SpriteTextFields
    {
        private static readonly Lazy<FieldInfo> _characterMapField = new(() => typeof(SpriteText).GetField("_characterMap", BindingFlags.NonPublic | BindingFlags.Static));
        private static readonly Lazy<FieldInfo> _fontPagesField = new(() => typeof(SpriteText).GetField("fontPages", BindingFlags.NonPublic | BindingFlags.Static));
        private static readonly Lazy<FieldInfo> _fontFileField = new(() => typeof(SpriteText).GetField("FontFile", BindingFlags.NonPublic | BindingFlags.Static));

        public static Dictionary<char, FontChar> _characterMap
        {
            get => (Dictionary<char, FontChar>)_characterMapField.Value.GetValue(null);
            set => _characterMapField.Value.SetValue(null, value);
        }

        public static List<Texture2D> fontPages
        {
            get => (List<Texture2D>)_fontPagesField.Value.GetValue(null);
            set => _fontPagesField.Value.SetValue(null, value);
        }

        public static FontFile FontFile
        {
            get => (FontFile)_fontFileField.Value.GetValue(null);
            set => _fontFileField.Value.SetValue(null, value);
        }
    }
}
