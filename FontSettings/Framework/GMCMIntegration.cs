using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeShared.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace FontSettings.Framework
{
    internal class GMCMIntegration : GenericModConfigMenuIntegrationBase
    {
        public GMCMIntegration(Action reset, Action save, IModRegistry modRegistry, IMonitor monitor, IManifest manifest)
            : base(reset, save, modRegistry, monitor, manifest)
        {
        }

        protected override void IntegrateOverride(GenericModConfigMenuFluentHelper helper)
        {
        }
    }
}
