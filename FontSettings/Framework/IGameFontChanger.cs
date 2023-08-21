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
        IResultWithoutData<string> ChangeGameFont(FontConfig font);
    }

    internal interface IAsyncGameFontChanger
    {
        /// <summary>Reload game font using <paramref name="font"/> parameter.</summary>
        Task<IResultWithoutData<string>> ChangeGameFontAsync(FontConfig font, FontContext context);
    }
}
