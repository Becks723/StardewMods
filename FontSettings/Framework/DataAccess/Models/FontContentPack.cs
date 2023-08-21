using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess.Models
{
    internal class FontContentPack
    {
        public ISemanticVersion Format { get; set; }
        public FontContentPackItem[] Fonts { get; set; }
    }
}
