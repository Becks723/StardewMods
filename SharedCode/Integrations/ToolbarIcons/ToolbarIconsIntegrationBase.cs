using System;
using System.Collections.Generic;
using System.Text;
using StardewModdingAPI;

namespace CodeShared.Integrations.ToolbarIcons
{
    internal abstract class ToolbarIconsIntegrationBase : ModIntegrationBase<IToolbarIconsApi>
    {
        protected ToolbarIconsIntegrationBase(IModRegistry modRegistry, IMonitor monitor)
            : base(modID: "furyx639.ToolbarIcons", modRegistry, monitor)
        {
        }
    }
}
