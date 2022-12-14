using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal interface IGameFontChanger
    {
        bool ChangeGameFont(FontConfig font);
    }

    internal interface IAsyncGameFontChanger
    {
        Task<bool> ChangeGameFontAsync(FontConfig font);
    }
}
