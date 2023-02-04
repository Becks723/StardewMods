using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    internal class VanillaFontDataRepository : DataInModDirectoryRepository
    {
        public VanillaFontDataRepository(IModHelper helper, IMonitor monitor) 
            : base(helper, monitor)
        {
        }

        public VanillaFontData ReadVanillaFontData()
        {
            return this.ReadModJsonFile<VanillaFontData>("assets/vanilla-fonts.json");
        }
    }
}
