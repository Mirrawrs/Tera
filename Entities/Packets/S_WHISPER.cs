using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class S_WHISPER : Packet
    {
        [Serialize]
        public string Sender { get; set; }
        [Serialize]
        public string Recipient { get; set; }
        [Serialize]
        public string Text { get; set; }
        [Serialize]
        public ulong SenderId { get; set; }
        [Serialize]
        public bool Outgoing { get; set; }
        [Serialize]
        public bool IsGameMaster { get; set; }
        [Serialize]
        public bool Unknown { get; set; }
    }
}