using System;

namespace Microsoft.Win32.RegistryInteractions
{
    public sealed class SubSectionManager : SectionManager
    {
        public SubSectionManager(Type type, RegistryKey mainRegistrySection) : base(type, mainRegistrySection)
        {
        }
    }
}