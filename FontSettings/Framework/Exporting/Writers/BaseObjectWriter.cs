using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Exporting.Writers
{
    internal abstract class BaseObjectWriter : IObjectWriter
    {
        public string TypeReaderName => this.GetTypeReaderName();

        public virtual int TypeReaderVersionNumber => 0;

        public abstract Type Type { get; }

        public abstract void Write(XnbWriter writer, object value);

        protected abstract string GetTypeReaderName();
    }

    internal abstract class BaseObjectWriter<TObject> : BaseObjectWriter
    {
        public override sealed Type Type => typeof(TObject);

        public override sealed void Write(XnbWriter writer, object value)
        {
            this.Write(writer, (TObject)value);
        }

        protected override string GetTypeReaderName()
        {
            return $"Microsoft.Xna.Framework.Content.{nameof(TObject)}Reader";
        }

        protected abstract void Write(XnbWriter writer, TObject value);
    }
}
