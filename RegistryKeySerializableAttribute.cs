using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

namespace System
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RegistryKeySerializableAttribute : Attribute
    {
        public string KeyName { get; set; }

        public RegistryValueKind ValueKind { get; set; }
    }
}