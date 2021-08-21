using System;
using System.Xml.Serialization;
using JunimoStudio.Core;
using Netcode;

namespace JunimoStudio.NetObjects
{
    public class NetPlugin : AbstractNetObjectWrapper<IPlugin>
    {
        private IPlugin _plugin;

        public override IPlugin Core => this._plugin;

        [XmlElement("uniqueId")]
        public readonly NetGuid uniqueId = new NetGuid(Guid.Empty);

        public NetPlugin()
            : base()
        {
        }

        public NetPlugin(IPlugin plugin)
        {
            this.InitNetFields();

            this._plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            this.uniqueId.Value = plugin.UniqueId;
        }

        public override void RestoreCoreObject()
        {
            string id = this.uniqueId.Value.ToString();
            this._plugin = Factory.Plugin<IPlugin>(id);
        }

        protected override void InitNetFields()
        {
            this.NetFields.AddFields(this.uniqueId);
        }
    }
}
