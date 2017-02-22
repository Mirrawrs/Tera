using Lotus.Serialization.Attributes;

namespace Tera.Analytics
{
    public class Unknown
    {
        [Serialize(0)]
        public int Unknown1 { get; set; }

        [Serialize(1)]
        public int Unknown2 { get; set; }
    }
}