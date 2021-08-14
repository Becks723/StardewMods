using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace JunimoStudio.ModIntegrations
{
    public class GenericIntegHelper<TApi> : BaseIntegHelper
        where TApi : class
    {
        protected readonly TApi _api;

        public GenericIntegHelper(string modName, string modId, IMonitor monitor, IModRegistry modRegistry)
            : base(modName, modId, monitor, modRegistry)
        {
            this.TryGetApi<TApi>(out this._api);
        }

        protected void ErrorIfApiNotLoaded()
        {
            if (!this.ApiLoaded)
                throw new InvalidOperationException(
                    $"类型为{typeof(TApi)}的Api未加载，请检查前置mod是否完整安装。");
        }
    }
}
