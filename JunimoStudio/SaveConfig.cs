using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    /// <summary>每个存档都有一个这样的配置文件。</summary>
    internal class SaveConfig
    {
        public TimeSettings Time { get; } = new();

        public TracksConfig Tracks { get; } = new();
    }
}
