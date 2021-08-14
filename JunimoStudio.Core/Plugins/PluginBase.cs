using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core.Plugins
{
    public abstract class PluginBase : IPlugin
    {
        protected PluginBase(Guid id, string name, PluginCategory category)
        {
            UniqueId = id;
            Name = name;
            Category = category;
        }

        public string Name { get; }

        public PluginCategory Category { get; }

        public Guid UniqueId { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
