using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class S_ANSWER_INTERACTIVE : Packet
    {
        [Serialize]
        public string Username { get; set; }
        [Serialize]
        public int Unknown1 { get; set; }
        [Serialize]
        public ModelInfo ModelInfo { get; set; }
        [Serialize]
        public int Level { get; set; }
        [Serialize]
        public bool HasGuild { get; set; }
        [Serialize]
        public byte Unknown4 { get; set; }
        [Serialize]
        public int Unknown5 { get; set; }
    }
}