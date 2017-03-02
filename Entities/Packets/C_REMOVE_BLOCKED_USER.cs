using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class C_REMOVE_BLOCKED_USER : Packet
    {
        [Serialize]
        public string UserName { get; set; }
    }
}