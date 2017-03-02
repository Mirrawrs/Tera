using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class C_BLOCK_USER : Packet
    {
        [Serialize]
        public string UserName { get; set; }
    }
}