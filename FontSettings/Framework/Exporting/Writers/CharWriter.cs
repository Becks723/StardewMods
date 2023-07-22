using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FontSettings.Framework.Exporting.Writers
{
    [ObjectWriter(typeof(char))]
    internal class CharWriter : BaseObjectWriter<char>
    {
        protected override void Write(XnbWriter writer, char value)
        {
            writer.Write(value);
        }

        protected override string GetTypeReaderName()
        {
            return "Microsoft.Xna.Framework.Content.CharReader";
        }
    }
}
