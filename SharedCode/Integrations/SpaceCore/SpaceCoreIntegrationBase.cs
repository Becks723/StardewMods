using StardewModdingAPI;

namespace CodeShared.Integrations.SpaceCore
{
    internal abstract class SpaceCoreIntegrationBase : ModIntegrationBase<ISpaceCoreApi>
    {
        protected SpaceCoreIntegrationBase(IModRegistry modRegistry, IMonitor monitor)
            : base(modID: "spacechase0.SpaceCore", modRegistry: modRegistry, monitor: monitor)
        {
        }
    }
}