using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Netcode;

namespace JunimoStudio
{
    public abstract class AbstractNetObjectWrapper<TCore> : INetObject<NetFields>
    {
        public NetFields NetFields { get; } = new NetFields();

        [XmlIgnore]
        public abstract TCore Core { get; }

        protected AbstractNetObjectWrapper()
        {
            this.InitNetFields();
        }

        /// <summary>
        /// 在反序列化时，重新反射泛型<see cref="TCore"/>类型的<see cref="Core"/>，并做一些必要的初始化操作（如，将字段或属性重新赋值给它）。注意，此方法不能有参数。
        /// </summary>
        /// <returns></returns>
        public abstract void RestoreCoreObject();

        protected abstract void InitNetFields();
    }
}
