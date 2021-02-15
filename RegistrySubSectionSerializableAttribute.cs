using System;

namespace Microsoft.Win32
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RegistrySubSectionSerializableAttribute : RegistrySectionSerializableAttribute { }
}