using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontPatching.Editors
{
    internal class BmFontFileEditor : BaseFontEditor<XmlSource>
    {
        public BmFontFileEditor(FontConfig_ config)
            : base(config)
        {
        }

        protected override void Edit(XmlSource xmlSource, FontConfig_ config)
        {
            FontFile fontFile = FontLoader.Parse(xmlSource.Source);
            this.Edit(fontFile, config);
            this.ResetXmlSource(xmlSource, FontHelpers.ParseFontFile(fontFile).Source);
        }

        private void Edit(FontFile fontFile, FontConfig_ config)
        {
            BmFontGenerator.EditExisting(
                existingFont: fontFile,
                overrideSpacing: config.Spacing,
                overrideLineSpacing: config.LineSpacing,
                extraCharOffsetX: config.CharOffsetX,
                extraCharOffsetY: config.CharOffsetY);
        }

        /// <summary>Set the <see cref="XmlSource.Source"/> of <paramref name="xmlSource"/> to <paramref name="xml"/>.</summary>
        private void ResetXmlSource(XmlSource xmlSource, string xml)
        {
            typeof(XmlSource)
                .GetField("source", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(xmlSource, xml);
        }
    }
}
