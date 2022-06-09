using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal struct CharacterRange
    {
        public char Start;
        public char End;

        public CharacterRange(int start, int end)
            : this((char)start, (char)end)
        {
        }

        public CharacterRange(char start, char end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
