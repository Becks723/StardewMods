using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess.Parsing
{
    internal class ContentPackCharacterFileHelper
    {
        public bool TryParseJson(string file, IContentPack sContentPack, out IEnumerable<CharacterRange> result)
        {
            try
            {
                result = sContentPack.ReadJsonFile<IEnumerable<CharacterRange>>(file);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public bool TryParsePlainText(string file, out IEnumerable<CharacterRange> result)
        {
            string text = File.ReadAllText(file);
            result = FontHelpers.GetCharacterRanges(text);
            return true;
        }
    }
}
