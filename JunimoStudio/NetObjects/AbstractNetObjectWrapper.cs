using System.Xml.Serialization;
using Netcode;

namespace JunimoStudio.NetObjects
{
    /// <summary>封装一个现有类型<see cref="TCore"/>的对象<see cref="Core"/>，使其适配星露谷内对象应满足的要求。</summary>
    /// <remarks>
    /// 星露谷内对象应满足：<br/>
    /// 1. 联机同步（NetField）。<br/>
    /// 2. 可被Xml序列化反序列化。（不是必须满足）<br/>
    /// </remarks>
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
        /// 在反序列化时，重新反射泛型<see cref="TCore"/>类型的<see cref="Core"/>，并做一些必要的初始化操作（如，将字段或属性重新赋值给它）。<br/>
        /// 注意，此方法不能有参数。
        /// </summary>
        /// <returns></returns>
        public abstract void RestoreCoreObject();

        protected abstract void InitNetFields();
    }
}
