using StardewModdingAPI;

namespace CodeShared.Integrations.SaveAnywhere
{
    internal abstract class SaveAnywhereIntegrationBase : ModIntegration<ISaveAnywhereAPI>
    {
        protected SaveAnywhereIntegrationBase(IModRegistry modRegistry, IMonitor monitor)
            : base("Omegasis.SaveAnywhere", modRegistry, monitor)
        {
        }
    }
}