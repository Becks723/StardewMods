using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace FontSettings.CommandLine.ContentPipeline
{
    [ContentTypeWriter]
    public class XmlSourceWriter : ContentTypeWriter<XmlSource>
    {
        protected override void Write(ContentWriter output, XmlSource value)
        {
            output.Write(value.Source);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(XmlSource).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(XmlSourceReader).AssemblyQualifiedName;
        }
    }
}
