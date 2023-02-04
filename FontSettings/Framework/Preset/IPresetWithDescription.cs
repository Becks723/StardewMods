using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Preset
{
    internal interface IPresetWithDescription : IPresetWithName
    {
        string Description { get; }
    }
}
