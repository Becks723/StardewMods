using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JunimoStudio.Core
{
    public interface IPlugin : IDisposable
    {
        string Name { get; }

        PluginCategory Category { get; }

        Guid UniqueId { get; }
    }
}