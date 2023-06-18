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

    internal static class ExtensibleExtensions
    {
        public static bool TryGetInstance<T>(this IExtensible extensible, out T instance) where T : class
        {
            if (extensible.Supports<T>())
            {
                instance = extensible.GetInstance<T>();
                return true;
            }

            instance = null;
            return false;
        }
    }
}
