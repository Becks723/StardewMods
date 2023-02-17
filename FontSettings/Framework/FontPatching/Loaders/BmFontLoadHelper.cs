using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.FontPatching.Replacers;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontPatching.Loaders
{
    internal class BmFontLoadHelper
    {
        public void GetLoaders(BmFontData bmFont,
            out IFontLoader fontFileLoader,
            out IDictionary<string, IFontLoader> pageLoaders)
        {
            FontFile fontFile = bmFont.FontFile;
            Texture2D[] pages = bmFont.Pages;

            // fontFileLoader
            XmlSource xml = FontHelpers.ParseFontFile(fontFile);
            fontFileLoader = new SimpleFontLoader(xml);

            // pageLoaders
            pageLoaders = new Dictionary<string, IFontLoader>();
            for (int i = 0; i < pages.Length; i++)
            {
                string file = fontFile.Pages[i].File;
                string name = $"Fonts/{file}";
                var pageLoader = new SimpleFontLoader(pages[i]);

                pageLoaders.Add(name, pageLoader);
            }
        }

        public void GetLoaders(BmFontData bmFont,
            out IFontReplacer fontFileReplacer,
            out IDictionary<string, IFontLoader> pageLoaders)
        {
            this.GetLoaders(bmFont, out IFontLoader fontFileLoader, out pageLoaders);
            fontFileReplacer = new FontReplacer(fontFileLoader);
        }
    }
}
