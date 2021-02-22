using System;

using Microsoft.Win32.RegistryInteractions;

namespace Microsoft.Win32
{
    public class RegistrySerializer
    {
        private readonly SectionManager m_RegistrySectionManager;

        public RegistrySerializer(Type type, RegistryKey registrySection) => m_RegistrySectionManager = new SectionManager(type, registrySection);

        public void Serialize(object o)
        {
            if(o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            m_RegistrySectionManager.CreateSection(o);
        }

        public object Deserialize() => m_RegistrySectionManager.GetSection();
    }
}