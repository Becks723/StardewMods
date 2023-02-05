using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.DataAccess.Models
{
    internal class SampleData
    {
        public IList<Text> Texts { get; set; } = new List<Text>();

        public string? GetTextForCurrentLangauge(bool throwIfKeyNotFound = false)
        {
            return this.GetTextForLangauge(
                FontHelpers.GetCurrentLanguage(), throwIfKeyNotFound);
        }

        public string? GetTextForLangauge(LanguageInfo key, bool throwIfKeyNotFound = false)
        {
            foreach (var entry in this.Texts)
            {
                if (entry.Key == key)
                    return entry.Value;
            }

            if (throwIfKeyNotFound)
                throw new KeyNotFoundException($"key: {key}");
            else
                return null;
        }
    }

    internal class Text
    {
        public LanguageInfo Key { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
