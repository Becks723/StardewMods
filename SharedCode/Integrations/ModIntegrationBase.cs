using System;
using StardewModdingAPI;

namespace CodeShared.Integrations
{
    internal abstract class ModIntegrationBase<TInterface>
        where TInterface : class
    {
        private readonly TInterface? _api;

        private readonly string _modID;
        private readonly IMonitor _monitor;

        protected ModIntegrationBase(string modID, IModRegistry modRegistry, IMonitor monitor, string failMessage = null)
        {
            this._modID = modID;
            this._monitor = monitor;

            this._api = modRegistry.GetApi<TInterface>(modID);
            if (this._api == null)
            {
                failMessage ??= $"The mod \"{modID}\" is not loaded, thus its API wouldn't be used!";
                this._monitor.Log(failMessage);
            }
        }

        public void Integrate()
        {
            var api = this._api;
            if (api == null)
                return;
            try
            {
                this.IntegrateOverride(api);
            }
            catch (Exception ex)
            {
                this._monitor.Log($"Failed to integrate with \"{this._modID}\" API: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }
        }

        protected abstract void IntegrateOverride(TInterface api);
    }
}
