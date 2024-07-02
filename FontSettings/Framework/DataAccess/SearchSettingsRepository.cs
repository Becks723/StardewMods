using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    internal class SearchSettingsRepository : DataGlobalRepository<SearchSettings>
    {
        protected override string GlobalKey { get; } = "search";

        public SearchSettingsRepository(IModHelper helper)
            : base(helper.Data)
        {
        }
    }
}
