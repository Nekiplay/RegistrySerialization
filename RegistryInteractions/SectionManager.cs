using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Win32.RegistryInteractions
{
    public class SectionManager
    {
        private protected Type m_EntityType;
        private protected readonly string m_EntityName;

        private protected readonly RegistryKey m_MainRegistrySection;
        private protected readonly RegistryKey m_CurrentSection;

        private protected readonly Lazy<List<SubSectionManager>> m_SubSections;

        public SectionManager(Type type, RegistryKey mainRegistrySection)
        {
            m_EntityType = type;
            m_MainRegistrySection = mainRegistrySection;

            RegistrySectionSerializableAttribute registrySectionAttribute = m_EntityType.GetCustomAttribute<RegistrySectionSerializableAttribute>();

            if (registrySectionAttribute is null)
            {
                throw new InvalidOperationException();
            }

            m_EntityName = string.IsNullOrWhiteSpace(registrySectionAttribute.SectionName) ? m_EntityType.Name : registrySectionAttribute.SectionName;
            m_SubSections = new Lazy<List<SubSectionManager>>();

            m_CurrentSection = m_MainRegistrySection.CreateSubKey(m_EntityName);

            m_EntityType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance).ToList().ForEach(field =>
            {
                RegistrySubSectionSerializableAttribute registrySubSectionAttribute = field.GetCustomAttribute<RegistrySubSectionSerializableAttribute>();

                if (!(registrySubSectionAttribute is null))
                {
                    m_SubSections.Value.Add(new SubSectionManager(field.FieldType, m_CurrentSection));
                }
            });
        }

        public SubSectionManager[] GetSubSections() => m_SubSections.Value.ToArray();

        public virtual object GetSection()
        {
            object newObject = m_EntityType.GetConstructor(Type.EmptyTypes).Invoke(null);

            return GetSection(newObject);
        }

        private protected virtual object GetSection(object newObject)
        {
            foreach (FieldInfo field in newObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance))
            {
                if (!(field.GetCustomAttribute<NonSerializedAttribute>() is null))
                {
                    continue;
                }

                RegistryKeySerializableAttribute registryKeyAttribute = field.GetCustomAttribute<RegistryKeySerializableAttribute>();
                RegistrySubSectionSerializableAttribute registrySubSectionAttribute = field.GetCustomAttribute<RegistrySubSectionSerializableAttribute>();

                if (!(registrySubSectionAttribute is null))
                {
                    object currentFieldValue = field.FieldType.GetConstructor(Type.EmptyTypes).Invoke(null);
                    field.SetValue(newObject, currentFieldValue);

                    GetSubSection(field.FieldType).GetSection(field.GetValue(newObject));
                }

                string fieldName = field.Name;

                if (registryKeyAttribute is null && registrySubSectionAttribute is null)
                {
                    field.SetValue(newObject, m_CurrentSection.GetValue(fieldName));
                }
                else if (!(registryKeyAttribute is null))
                {
                    string keyName = string.IsNullOrWhiteSpace(registryKeyAttribute.KeyName) ? fieldName : registryKeyAttribute.KeyName;

                    field.SetValue(newObject, m_CurrentSection.GetValue(keyName));
                }
            }

            return newObject;
        }

        public virtual SubSectionManager GetSubSection(Type subSectionType) => m_SubSections.Value.FirstOrDefault(subSection => subSection.m_EntityType == subSectionType);

        public virtual SubSectionManager GetSubSection(string subSectionName) => m_SubSections.Value.FirstOrDefault(subSection => subSection.m_EntityName == subSectionName);

        public virtual void CreateSection(object entity)
        {
            foreach (FieldInfo field in m_EntityType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance))
            {
                if (!(field.GetCustomAttribute<NonSerializedAttribute>() is null))
                {
                    continue;
                }

                RegistryKeySerializableAttribute registryKeyAttribute = field.GetCustomAttribute<RegistryKeySerializableAttribute>();
                RegistrySubSectionSerializableAttribute registrySubSectionAttribute = field.GetCustomAttribute<RegistrySubSectionSerializableAttribute>();

                if (!(registrySubSectionAttribute is null))
                {
                    GetSubSection(field.FieldType).CreateSection(field.GetValue(entity));
                }

                string fieldName = field.Name;
                object fieldValue = field.GetValue(entity);

                if (registryKeyAttribute is null && registrySubSectionAttribute is null)
                {
                    m_CurrentSection.SetValue(fieldName, fieldValue);
                }
                else if (!(registryKeyAttribute is null))
                {
                    string keyName = string.IsNullOrWhiteSpace(registryKeyAttribute.KeyName) ? fieldName : registryKeyAttribute.KeyName;

                    m_CurrentSection.SetValue(keyName, fieldValue, registryKeyAttribute.ValueKind);
                }
            }

            m_CurrentSection.Flush();
        }

        public virtual void DeleteSection() => m_CurrentSection.DeleteSubKeyTree(m_EntityName);
    }
}