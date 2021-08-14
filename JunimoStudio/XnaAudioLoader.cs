using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    internal class XnaAudioLoader : IAudioLoader
    {

        public IAudioPack LoadPack(string key)
        {
            var dir = ParseKey(key);
            throw new NotImplementedException();
        }

        public IAudioItem Load(string key)
        {
            throw new NotImplementedException();
        }

        private DirectoryInfo ParseKey(string key)
        {
            DirectoryInfo dir = new(key);

            if (!dir.Exists)
                throw new DirectoryNotFoundException(key);

            return dir;
        }
    }
}
