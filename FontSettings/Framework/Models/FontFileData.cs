using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontInfomation;

namespace FontSettings.Framework.Models
{
    internal class FontFileData  //TODO: make immutable
    {
        public string FullPath { get; }

        public string FamilyName { get; }

        public string Name { get; }

        public string SubfamilyName { get; }

        public int FontIndex { get; }
    }
}
