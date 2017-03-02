using Tera;

namespace SampleBot
{
    //Simple implementation of the ISystemMessageTypeInfo interface.
    internal class SystemMessageTypeInfo : ISystemMessageTypeInfo
    {
        public SystemMessageTypeInfo(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public string Name { get; }
        public string Text { get; }
    }
}