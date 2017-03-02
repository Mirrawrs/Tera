using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class S_LOGIN_ACCOUNT_INFO : Packet
    {
        [Serialize]
        public string WorldName { get; set; }
        [Serialize]
        public int Unknown1 { get; set; }
        [Serialize]
        public int Unknown2 { get; set; }
    }
}