using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    internal interface IAudioLoader
    {
        IAudioPack LoadPack(string key);

        IAudioItem Load(string key);
    }
}
