using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class C_WHISPER : Packet
    {
        [Serialize]
        public string RecipientName { get; set; }
        [Serialize]
        public string Text { get; set; }
    }
}