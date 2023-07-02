using StardewModdingAPI;

namespace CodeShared.Integrations.SaveAnywhere
{
    internal abstract class SaveAnywhereIntegrationBase : ModIntegrationBase<ISaveAnywhereAPI>
    {
        protected SaveAnywhereIntegrationBase(IModRegistry modRegistry, IMonitor monitor)
            : base("Omegasis.SaveAnywhere", modRegistry, monitor)
        {
        }
    }
}