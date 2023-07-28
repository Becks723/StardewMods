using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching.Resolving
{
    internal class PatchModeInfo
    {
        public bool LoadOrReplace { get; }
        public int LoadPriority { get; }
        public int EditPriority { get;  }

        public PatchModeInfo(bool loadOrReplace = true, int loadPriority = int.MaxValue, int editPriority = 0)
        {
            this.LoadOrReplace = loadOrReplace;
            this.LoadPriority = loadPriority;
            this.EditPriority = editPriority;
        }
    }
}
