using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Exporting
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ObjectWriterAttribute : Attribute
    {
        public Type ObjectType { get; }

        public ObjectWriterAttribute(Type objectType)
        {
            this.ObjectType = objectType;
        }
    }
}
