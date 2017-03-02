using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class S_SYSTEM_MESSAGE : Packet
    {
        [Serialize]
        public string Content { get; set; }
    }
}