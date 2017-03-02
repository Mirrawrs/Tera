using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class C_ASK_INTERACTIVE : Packet
    {
        [Serialize]
        public string Username { get; set; }
        [Serialize]
        public int Unknown1 { get; set; } = 1;
        [Serialize]
        public int Unknown2 { get; set; } = 4012;
    }
}