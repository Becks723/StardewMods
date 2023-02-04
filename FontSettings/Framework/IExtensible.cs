using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal interface IExtensible
    {
        bool Supports<T>() where T : class;

        T? GetInstance<T>() where T : class;
    }
}
