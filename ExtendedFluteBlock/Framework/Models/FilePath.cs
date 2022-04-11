namespace FluteBlockExtension.Framework.Models
{
    internal class FilePath
    {
        public static FilePath With(string path, bool relative = true)
        {
            return new FilePath { Path = path, Relative = relative };
        }

        /// <summary>The path string value.</summary>
        public string Path { get; set; }

        /// <summary>Whether the path is relative or absolute.</summary>
        public bool Relative { get; set; }

        /// <summary>Helper method. Gets the absolute path indicated by this instace.</summary>
        /// <param name="relativeTo">The base path if <see cref="Relative"/> is true.</param>
        public string GetFullPath(string relativeTo)
        {
            return this.Relative
                ? System.IO.Path.Combine(relativeTo, this.Path)
                : this.Path;
        }
    }
}