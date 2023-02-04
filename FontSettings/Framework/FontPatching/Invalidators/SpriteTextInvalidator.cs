using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace FontSettings.Framework.FontPatching.Invalidators
{
    internal class SpriteTextInvalidator : BaseFontPatchInvalidator, ISpriteTextPatchInvalidator
    {
        private readonly IGameContentHelper _contentHelper;

        public SpriteTextInvalidator(IModHelper modHelper)
        {
            this._contentHelper = modHelper.GameContent;
        }

        protected override void InvalidateCore()
        {
            // invalidate.
            {
                // font file
                string fontFileName = FontHelpers.GetFontFileAssetName();
                this._contentHelper.InvalidateCache(fontFileName);
                this._contentHelper.InvalidateCache(this.LocalizeBaseAssetName(fontFileName));

                // pages
                var fontFile = SpriteTextFields.FontFile;
                foreach (FontPage page in fontFile?.Pages ?? Enumerable.Empty<FontPage>())
                {
                    string pageName = $"Fonts/{page.File}";

                    this._contentHelper.InvalidateCache(pageName);
                    this._contentHelper.InvalidateCache(this.LocalizeBaseAssetName(pageName));
                }
            }

            // propagate
            {
                // fontFile
                FontFile fontFile = this.LoadFontFile(FontHelpers.GetFontFileAssetName());

                // characterMap
                var characterMap = new Dictionary<char, FontChar>();
                foreach (FontChar current in fontFile.Chars)
                {
                    char key = (char)current.ID;
                    characterMap.Add(key, current);
                }

                // fontPages
                var fontPages = new List<Texture2D>();
                foreach (FontPage current2 in fontFile.Pages)
                {
                    fontPages.Add(Game1.content.Load<Texture2D>("Fonts\\" + current2.File));
                }

                SpriteTextFields.FontFile = fontFile;
                SpriteTextFields._characterMap = characterMap;
                SpriteTextFields.fontPages = fontPages;
            }
        }

        private FontFile LoadFontFile(string assetName)
        {
            return FontLoader.Parse(Game1.content.Load<XmlSource>(assetName).Source);
        }

        void ISpriteTextPatchInvalidator.UpdateFontFile(FontFile fontFile)
        {

        }
    }
}
