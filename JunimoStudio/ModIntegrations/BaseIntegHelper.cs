using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace JunimoStudio.ModIntegrations
{
    public class BaseIntegHelper
    {
        protected readonly string _modName;

        protected readonly string _modId;

        protected readonly IMonitor _monitor;

        protected readonly IModRegistry _modRegistry;

        private bool _apiLoaded;

        public bool ApiLoaded => this._apiLoaded;

        public BaseIntegHelper(string modName, string modId, IMonitor monitor, IModRegistry modRegistry)
        {
            this._modName = modName;
            this._modId = modId;
            this._monitor = monitor;
            this._modRegistry = modRegistry;
        }

        protected bool TryGetApi<TApi>(out TApi api) where TApi : class
        {
            api = this._modRegistry.GetApi<TApi>(this._modId);
            this._apiLoaded = (api != null);
            return this._apiLoaded;
        }
    }
}
