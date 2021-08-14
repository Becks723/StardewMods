using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.ModIntegrations.SpaceCore
{
    public interface ISpaceCoreApi
    {
        /// <summary></summary>
        /// <remarks> Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")</remarks>
        void RegisterSerializerType(Type type);
    }
}
