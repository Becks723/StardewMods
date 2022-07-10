using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontInfomation
{
    internal class TableRecord
    {
        public string Tag { get; }

        public uint CheckSum { get; }

        public uint Offset { get; }

        public uint Length { get; }

        public TableRecord(string tag, uint checkSum, uint offset, uint length)
        {
            this.Tag = tag;
            this.CheckSum = checkSum;
            this.Offset = offset;
            this.Length = length;
        }
    }
}
