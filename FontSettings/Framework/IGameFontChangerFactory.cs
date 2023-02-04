using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal interface IGameFontChangerFactory
    {
        IGameFontChanger3 CreateChanger(GameFontType fontType);

        IAsyncGameFontChanger3 CreateAsyncChanger(GameFontType fontType);
    }
}
