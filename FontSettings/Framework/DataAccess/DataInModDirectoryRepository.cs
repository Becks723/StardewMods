using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    internal abstract class DataInModDirectoryRepository
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        public DataInModDirectoryRepository(IModHelper helper, IMonitor monitor)
        {
            this._helper = helper;
            this._monitor = monitor;
        }

        protected TModel ReadModJsonFile<TModel>(string path, bool logIfNotFound = true)
            where TModel : class, new()
        {
            var model = this._helper.Data.ReadJsonFile<TModel>(path);
            if (model == null)
            {
                if (logIfNotFound)
                    this._monitor.Log(I18n.Misc_ModFileNotFound(path), LogLevel.Error);
                model = new();
            }
            return model;
        }

    }
}
