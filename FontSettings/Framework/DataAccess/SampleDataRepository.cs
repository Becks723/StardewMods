using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    internal class SampleDataRepository : DataInModDirectoryRepository
    {
        public SampleDataRepository(IModHelper helper, IMonitor monitor)
            : base(helper, monitor)
        {
        }

        public SampleData ReadSampleData()
        {
            return this.ReadModJsonFile<SampleData>("assets/sample.json");
        }
    }
}
