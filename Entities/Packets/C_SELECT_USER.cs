using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class C_SELECT_USER : Packet
    {
        [Serialize]
        public int UserId { get; set; }
        [Serialize]
        public bool Unknown1 { get; set; }
    }
}