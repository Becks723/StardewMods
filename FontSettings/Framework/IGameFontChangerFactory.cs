using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal interface IGameFontChangerFactory
    {
        IGameFontChanger CreateChanger(GameFontType fontType);

        IAsyncGameFontChanger CreateAsyncChanger(GameFontType fontType);
    }
}
