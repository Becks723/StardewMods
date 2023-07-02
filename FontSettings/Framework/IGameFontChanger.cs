using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal interface IGameFontChanger
    {
        IGameFontChangeResult ChangeGameFont(FontConfig font);
    }

    internal interface IAsyncGameFontChanger
    {
        /// <summary>Reload game font using <paramref name="font"/> parameter.</summary>
        Task<IGameFontChangeResult> ChangeGameFontAsync(FontConfig font, FontContext context);
    }
}
