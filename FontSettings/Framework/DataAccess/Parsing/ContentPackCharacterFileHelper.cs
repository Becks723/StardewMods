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
        public bool TryParseJson(string file, IContentPack sContentPack, out IEnumerable<CharacterRange> result, out Exception exception)
        {
            try
            {
                result = sContentPack.ReadJsonFile<IEnumerable<CharacterRange>>(file);
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                result = null;
                exception = ex;
                return false;
            }
        }

        public bool TryParseUnicode(string file, IContentPack sContentPack, out IEnumerable<CharacterRange> result, out Exception exception)
        {
            result = null;

            string fullPath = Path.Combine(sContentPack.DirectoryPath, file);

            if (!File.Exists(fullPath))
            {
                exception = new FileNotFoundException(null, file);
                return false;
            }

            string text;
            try
            {
                text = File.ReadAllText(file);
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }

            result = FontHelpers.GetCharacterRanges(text);
            exception = null;
            return true;
        }
    }
}
