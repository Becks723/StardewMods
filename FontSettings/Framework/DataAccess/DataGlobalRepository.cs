using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    internal abstract class DataGlobalRepository<TData>
            where TData : class, new()
    {
        private readonly IDataHelper _helper;

        protected abstract string GlobalKey { get; }

        protected DataGlobalRepository(IDataHelper helper)
        {
            this._helper = helper;
        }

        public TData ReadData()
        {
            TData? data = this._helper.ReadGlobalData<TData>(this.GlobalKey);
            if (data == null)
            {
                data = new TData();
                this.WriteData(data);
            }

            return data;
        }

        public void WriteData(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            this._helper.WriteGlobalData(this.GlobalKey, data);
        }

        public void ClearData()
        {
            this._helper.WriteGlobalData<TData>(this.GlobalKey, null);
        }
    }
}
