using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class S_LOGIN_ARBITER : Packet
    {
        [Serialize]
        public bool Success { get; set; }
        [Serialize]
        private byte Unknown1 { get; set; }
        [Serialize]
        private byte Unknown2 { get; set; }
        [Serialize]
        private byte Unknown3 { get; set; }
        [Serialize]
        private int ResultCode { get; set; }
    }
}