using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Exporting
{
    internal interface IObjectWriter
    {
        Type Type { get; }
        string TypeReaderName { get; }
        int TypeReaderVersionNumber { get; }
        void Write(XnbWriter writer, object value);
    }
}
