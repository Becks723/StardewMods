using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    partial class VanillaFontDataRepository : DataInModDirectoryRepository
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
