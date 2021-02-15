using System;
using System.Reflection;

namespace Microsoft.Win32
{
    public class RegistrySerializer
    {
        private readonly RegistryKey m_MainRegistrySection;

        public RegistrySerializer(RegistryKey registryKey)
        {
            m_MainRegistrySection = registryKey;
        }

        public void Serialize(object serializableObject)
        {
            Type objectType = serializableObject.GetType();

            RegistrySectionSerializableAttribute registrySectionAttribute = objectType.GetCustomAttribute<RegistrySectionSerializableAttribute>();

            if (registrySectionAttribute is null)
            {
                throw new InvalidOperationException();
            }

            RegistryKey currentSection = m_MainRegistrySection.CreateSubKey(string.IsNullOrWhiteSpace(registrySectionAttribute.SectionName) ? objectType.Name : registrySectionAttribute.SectionName);

            CreateSection(serializableObject, objectType, currentSection);

            currentSection.Flush();
        }

        public void Serialize<T>(T serializableObject) => Serialize((object)serializableObject);

        public T Deserialize<T>() where T : new()
        {
            T newObject = new T();
            Type objectType = newObject.GetType();

            RegistrySectionSerializableAttribute registrySectionAttribute = objectType.GetCustomAttribute<RegistrySectionSerializableAttribute>();

            if (registrySectionAttribute is null)
            {
                throw new InvalidOperationException();
            }

            RegistryKey currentSection = m_MainRegistrySection.OpenSubKey(string.IsNullOrWhiteSpace(registrySectionAttribute.SectionName) ? objectType.Name : registrySectionAttribute.SectionName);

            if (currentSection is null)
            {
                throw new NullReferenceException();
            }

            GetSection(newObject, objectType, currentSection);

            return newObject;
        }

        private static void GetSection<T>(T newObject, Type objectType, RegistryKey currentSection) where T : new()
        {
            foreach (FieldInfo currentField in objectType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.NonPublic))
            {
                if (!(currentField.GetCustomAttribute<NonSerializedAttribute>() is null))
                {
                    continue;
                }

                RegistryKeySerializableAttribute registryKeyAttribute = currentField.GetCustomAttribute<RegistryKeySerializableAttribute>();

                RegistrySubSectionSerializableAttribute registrySubSectionAttribute = currentField.GetCustomAttribute<RegistrySubSectionSerializableAttribute>();

                GetSubSection(newObject, currentSection, currentField, registrySubSectionAttribute);

                string propertyName = currentField.Name;

                if (registryKeyAttribute is null && registrySubSectionAttribute is null)
                {
                    currentField.SetValue(newObject, currentSection.GetValue(propertyName));
                }
                else if (!(registryKeyAttribute is null))
                {
                    string keyName = string.IsNullOrWhiteSpace(registryKeyAttribute.KeyName) ? propertyName : registryKeyAttribute.KeyName;

                    currentField.SetValue(newObject, currentSection.GetValue(keyName));
                }
            }
        }

        private static void GetSubSection(object deserializableObject, RegistryKey currentSection, FieldInfo currentField, RegistrySubSectionSerializableAttribute registrySubSectionAttribute)
        {
            if (registrySubSectionAttribute is null)
            {
                return;
            }

            RegistryKey currentSubSection = currentSection.OpenSubKey(string.IsNullOrWhiteSpace(registrySubSectionAttribute.SectionName) ? currentField.Name : registrySubSectionAttribute.SectionName);

            object currentFieldValue = currentField.FieldType.GetConstructor(new Type[] { }).Invoke(new object[] { });
            currentField.SetValue(deserializableObject, currentFieldValue);

            GetSection(currentField.GetValue(deserializableObject), currentField.FieldType, currentSubSection);
        }

        private static void CreateSection(object serializableObject, Type objectType, RegistryKey currentSection)
        {
            foreach (FieldInfo currentField in objectType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.NonPublic))
            {
                if (!(currentField.GetCustomAttribute<NonSerializedAttribute>() is null))
                {
                    continue;
                }

                RegistryKeySerializableAttribute registryKeyAttribute = currentField.GetCustomAttribute<RegistryKeySerializableAttribute>();

                RegistrySubSectionSerializableAttribute registrySubSectionAttribute = currentField.GetCustomAttribute<RegistrySubSectionSerializableAttribute>();

                CreateSubSection(serializableObject, currentSection, currentField, registrySubSectionAttribute);

                string propertyName = currentField.Name;
                object propertyValue = currentField.GetValue(serializableObject);

                if (registryKeyAttribute is null && registrySubSectionAttribute is null)
                {
                    currentSection.SetValue(propertyName, propertyValue);
                }
                else if (!(registryKeyAttribute is null))
                {
                    string keyName = string.IsNullOrWhiteSpace(registryKeyAttribute.KeyName) ? propertyName : registryKeyAttribute.KeyName;

                    currentSection.SetValue(keyName, propertyValue, registryKeyAttribute.ValueKind);
                }
            }
        }

        private static void CreateSubSection(object serializableObject, RegistryKey currentSection, FieldInfo currentField, RegistrySubSectionSerializableAttribute registrySubSectionAttribute)
        {
            if (registrySubSectionAttribute is null)
            {
                return;
            }

            RegistryKey currentSubSection = currentSection.CreateSubKey(string.IsNullOrWhiteSpace(registrySubSectionAttribute.SectionName) ? currentField.Name : registrySubSectionAttribute.SectionName);

            object currentFieldValue = currentField.GetValue(serializableObject);
            Type currentFieldType = currentFieldValue.GetType();

            CreateSection(currentFieldValue, currentFieldType, currentSubSection);
        }
    }
}