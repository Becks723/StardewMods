using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.CommandLine
{
    internal struct FontSize
    {
        public FontSizeType Type { get; }

        public float Value { get; }

        public FontSize(FontSizeType type, float value)
        {
            this.Type = type;
            this.Value = value;
        }

        public FontSize(string s)
            : this(ParseString(s))
        {
        }

        public float InPixels()
            => this.Type switch
            {
                FontSizeType.Pixel => this.Value,
                FontSizeType.Point => this.PtToPx(this.Value),
                _ => throw new NotSupportedException(),
            };

        public float InPoints()
            => this.Type switch
            {
                FontSizeType.Pixel => this.PxToPt(this.Value),
                FontSizeType.Point => this.Value,
                _ => throw new NotSupportedException(),
            };

        private FontSize((FontSizeType type, float value) tuple)
            : this(tuple.type, tuple.value)
        {
        }

        private static (FontSizeType type, float value) ParseString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException($"'{nameof(s)}' cannot be null or whitespace.", nameof(s));

            var types = new Dictionary<string, FontSizeType>
            {
                { "px", FontSizeType.Pixel },
                { "pt", FontSizeType.Point },
            };

            foreach (var type in types)
            {
                if (s.EndsWith(type.Key, StringComparison.OrdinalIgnoreCase))
                {
                    string valStr = s.Substring(0, s.Length - 2);
                    return (type.Value, float.Parse(valStr));
                }
            }

            throw new ArgumentException($"Failed to recognize '{s}' as font size.", nameof(s));
        }

        private float PxToPt(float px) => px * 0.75f;
        private float PtToPx(float pt) => pt / 0.75f;
    }

    internal enum FontSizeType
    {
        Pixel,
        Point
    }
}
