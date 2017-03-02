using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class S_CHAT : Packet
    {
        [Serialize]
        public string Sender { get; set; }
        [Serialize]
        public string Message { get; set; }
        [Serialize]
        public ChatChannel Channel { get; set; }
        [Serialize]
        public ulong SenderId { get; set; }
        [Serialize]
        public bool Unknown1 { get; set; }
        [Serialize]
        public bool IsGameMaster { get; set; }
        [Serialize]
        public bool Unknown3 { get; set; }
    }
}