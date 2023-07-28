using System;
using System.Collections.Generic;
using System.Text;
using StardewModdingAPI;

namespace CodeShared.Integrations.GMCMOptions
{
    internal class GMCMOptionsIntegrationObject : GMCMOptionsIntegrationBase
    {
        public IGMCMOptionsAPI? Api { get; private set; }

        public GMCMOptionsIntegrationObject(IModRegistry modRegistry, IMonitor monitor)
            : base(modRegistry, monitor)
        {
        }

        protected override void IntegrateOverride(IGMCMOptionsAPI api)
        {
            this.Api = api;
        }
    }
}
