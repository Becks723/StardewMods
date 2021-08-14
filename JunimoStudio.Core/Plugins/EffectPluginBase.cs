using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core.Plugins
{
    public class EffectPluginBase : PluginBase, IEffectPlugin
    {
        public EffectPluginBase(Guid id, string name)
            : base(id, name, PluginCategory.Effect)
        {
        }
    }
}
