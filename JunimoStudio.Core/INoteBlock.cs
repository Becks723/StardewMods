using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core;

namespace JunimoStudio.Core
{
    public interface INoteBlock
    {
        IChannelManager ChannelManager { get; }
    }
}
