using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FontSettings.Framework.Exporting.Writers
{
    [ObjectWriter(typeof(Vector3))]
    internal class Vector3Writer : BaseObjectWriter<Vector3>
    {
        protected override void Write(XnbWriter writer, Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        protected override string GetTypeReaderName()
        {
            return "Microsoft.Xna.Framework.Content.Vector3Reader";
        }
    }
}
