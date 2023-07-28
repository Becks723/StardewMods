using System;
using System.Collections.Generic;
using System.Text;
using StardewModdingAPI;

namespace CodeShared.Integrations.GMCMOptions
{
    internal abstract class GMCMOptionsIntegrationBase : ModIntegrationBase<IGMCMOptionsAPI>
    {
        public GMCMOptionsIntegrationBase(IModRegistry modRegistry, IMonitor monitor)
            : base("jltaylor-us.GMCMOptions", modRegistry, monitor)
        {
        }
    }
}
