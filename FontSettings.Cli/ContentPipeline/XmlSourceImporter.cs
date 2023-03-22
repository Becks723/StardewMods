using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace FontSettings.CommandLine.ContentPipeline
{
    [ContentImporter(".xml", DisplayName = "Xml Source Importer", DefaultProcessor = "PassThroughProcessor")]
    public class XmlSourceImporter : ContentImporter<XmlSource>
    {
        public override XmlSource Import(string filename, ContentImporterContext context)
        {
            return new XmlSource(File.ReadAllText(filename));
        }
    }
}
