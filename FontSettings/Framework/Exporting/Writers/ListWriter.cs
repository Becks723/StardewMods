using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Exporting.Writers
{
    [ObjectWriter(typeof(List<>))]
    internal class ListWriter<T> : BaseObjectWriter<List<T>>
    {
        /*private readonly string _genericTypeName;

        public ListWriter(string genericTypeName)
        {
            this._genericTypeName = genericTypeName;
        }*/

        protected override void Write(XnbWriter writer, List<T> list)
        {
            writer.Write(list.Count);
            foreach (T item in list)
            {
                writer.WriteObject<T>(item);
            }
        }

        protected override string GetTypeReaderName()
        {
            return $"Microsoft.Xna.Framework.Content.ListReader`1[[{typeof(T).AssemblyQualifiedName}]]";
        }
    }
}
