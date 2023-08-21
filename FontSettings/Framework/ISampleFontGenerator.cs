using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal interface ISampleFontGenerator
    {
        ISpriteFont Generate(FontConfig config, FontContext context);
        Task<ISpriteFont> GenerateAsync(FontConfig config, FontContext context, CancellationToken cancellationToken);
    }
}
