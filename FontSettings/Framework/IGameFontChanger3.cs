using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal interface IGameFontChanger3
    {
        IGameFontChangeResult ChangeGameFont(FontConfig_ font);
    }

    internal interface IAsyncGameFontChanger3
    {
        Task<IGameFontChangeResult> ChangeGameFontAsync(FontConfig_ font);
    }
}
