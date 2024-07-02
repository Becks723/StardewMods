using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models
{
    internal class SearchSettings
    {
        public IList<SearchFolderSettings> Folders { get; set; } = new List<SearchFolderSettings>();

        public ISet<string> IgnoredFiles { get; set; } = new HashSet<string>();

        public SearchSettings() { }

        public SearchSettings(SearchSettings copy)
        {
            if (copy == null)
                throw new ArgumentNullException(nameof(copy));

            if (copy.Folders == null)
                this.Folders = null;
            else
            {
                foreach (var folder in copy.Folders)
                    this.Folders.Add(new SearchFolderSettings(folder));
            }

            if (copy.IgnoredFiles == null)
                this.IgnoredFiles = null;
            else
            {
                foreach (var file in this.IgnoredFiles)
                    this.IgnoredFiles.Add(file);
            }
        }
    }

    internal class SearchFolderSettings
    {
        public bool Enabled { get; set; } = true;

        public string Path { get; set; }

        public bool RecursiveScan { get; set; } = true;

        public ISet<string> Extensions { get; set; } = new HashSet<string>(new[] { ".ttf", ".ttc", ".otf" });

        public bool LogDetails { get; set; } = false;

        public SearchFolderSettings() { }

        public SearchFolderSettings(SearchFolderSettings copy)
        {
            if (copy == null)
                throw new ArgumentNullException(nameof(copy));

            this.Enabled = copy.Enabled;
            this.Path = copy.Path;
            this.RecursiveScan = copy.RecursiveScan;
            this.LogDetails = copy.LogDetails;
            this.Extensions = copy.Extensions != null ? new HashSet<string>(copy.Extensions) : null;
        }
    }
}
