using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    internal interface IAudioItem
    {
        IAudioPack Pack { get; internal set; }

        string Name { get; }


    }
}
