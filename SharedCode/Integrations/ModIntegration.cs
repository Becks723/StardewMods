using StardewModdingAPI;

namespace CodeShared.Integrations
{
    internal abstract class ModIntegration<TInterface>
        where TInterface : class
    {
        private readonly TInterface _api;

        private readonly bool _loaded;

        protected ModIntegration(string modID, IModRegistry modRegistry, IMonitor monitor, string failMessage = null)
        {
            this._api = modRegistry.GetApi<TInterface>(modID);
            this._loaded = true;
            if (this._api is null)
            {
                this._loaded = false;
                failMessage ??= $"The mod \"{modID}\" is not loaded, thus its API wouldn't be used!";
                monitor.Log(failMessage, LogLevel.Warn);
            }
        }

        public void Integrate()
        {
            if (!this._loaded)
                return;

            this.IntegrateOverride(this._api);
        }

        protected abstract void IntegrateOverride(TInterface api);
    }
}
