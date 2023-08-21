using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;

namespace FontSettings.Framework.Exporting.Writers
{
    [ObjectWriter(typeof(XmlSource))]
    internal class XmlSourceWriter : BaseObjectWriter<XmlSource>
    {
        protected override void Write(XnbWriter writer, XmlSource value)
        {
            writer.Write(value.Source);
        }

        protected override string GetTypeReaderName()
        {
            return "BmFont.XmlSourceReader, BmFont, Version=2012.1.7.0, Culture=neutral, PublicKeyToken=null";
        }
    }
}
