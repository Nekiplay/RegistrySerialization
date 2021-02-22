using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Win32.RegistryInteractions.Generic
{
    public class SectionManager<TEntity> : SectionManager where TEntity : new()
    {
        public SectionManager(RegistryKey mainRegistrySection) : base(typeof(TEntity), mainRegistrySection)
        {
        }
    }
}