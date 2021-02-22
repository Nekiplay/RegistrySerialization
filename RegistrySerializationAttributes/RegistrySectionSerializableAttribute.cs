namespace System
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegistrySectionSerializableAttribute : Attribute
    {
        public string SectionName { get; set; }
    }
}