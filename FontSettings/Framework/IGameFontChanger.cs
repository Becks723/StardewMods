using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal interface IGameFontChanger
    {
        IGameFontChangeResult ChangeGameFont(FontConfig font);
    }

    internal interface IAsyncGameFontChanger
    {
        Task<IGameFontChangeResult> ChangeGameFontAsync(FontConfig font);
    }
}
