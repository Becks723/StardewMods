using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FontSettings.Framework.Exporting.Writers
{
    [ObjectWriter(typeof(Rectangle))]
    internal class RectangleWriter : BaseObjectWriter<Rectangle>
    {
        protected override void Write(XnbWriter writer, Rectangle value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Width);
            writer.Write(value.Height);
        }

        protected override string GetTypeReaderName()
        {
            return "Microsoft.Xna.Framework.Content.RectangleReader";
        }
    }
}
