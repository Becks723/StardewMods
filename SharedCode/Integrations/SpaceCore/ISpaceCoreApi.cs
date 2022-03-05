using System;

namespace CodeShared.Integrations.SpaceCore
{
    public interface ISpaceCoreApi
    {
        /// <summary></summary>
        /// <remarks> Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")</remarks>
        void RegisterSerializerType(Type type);
    }
}
