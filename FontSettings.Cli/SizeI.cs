using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.CommandLine
{
    internal struct SizeI
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public SizeI(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public SizeI(string s)
            : this(ParseString(s))
        {
        }

        private SizeI((int width, int height) tuple)
            : this(tuple.width, tuple.height)
        {
        }

        private static (int width, int height) ParseString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException($"'{nameof(s)}' cannot be null or whitespace.", nameof(s));

            try
            {
                string[] seg = s.Split('x');

                int width = int.Parse(seg[0]);
                int height = int.Parse(seg[1]);

                return (width, height);
            }
            catch (Exception ex)
            {
                throw new FormatException($"'{s}' cannot be identified as a size.", ex);
            }
        }
    }
}
