using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess
{
    partial class FontConfigRepository
    {
        private readonly string _globalFontDataKey = "font-data";
        private readonly IModHelper _helper;

        public FontConfigRepository(IModHelper helper)
        {
            this._helper = helper;
        }

        public FontConfigs ReadAllConfigs()
        {
            FontConfigs fonts = this._helper.Data.ReadGlobalData<FontConfigs>(this._globalFontDataKey);
            if (fonts == null)
            {
                fonts = new FontConfigs();
                this.WriteAllConfigs(fonts);
            }

            return fonts.Distinct();
        }

        public void WriteAllConfigs(FontConfigs configs)
        {
            this._helper.Data.WriteGlobalData(this._globalFontDataKey, configs.Distinct());
        }

        public void ClearAllConfigs()
        {
            this.WriteAllConfigs(new FontConfigs());
        }
    }
}
