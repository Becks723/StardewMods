using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal interface IFontConfigRepository
    {
        FontConfigModel? ReadConfig(FontConfigKey key);

        void WriteConfig(FontConfigKey key, FontConfigModel? config);
    }
}
