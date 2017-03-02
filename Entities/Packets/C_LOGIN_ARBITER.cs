using Lotus.Serialization.Attributes;
using Tera.DirectiveSelectors;

namespace Tera.Packets
{
    public class C_LOGIN_ARBITER : Packet
    {
        [Serialize]
        private string AccountName { get; set; } = " ";

        [ASCII]
        [Serialize]
        public string Ticket { get; set; }

        [Serialize]
        private ushort TicketLength => (ushort) Ticket.Length;

        [Serialize]
        private int Unknown1 { get; set; }

        [Serialize]
        private byte Unknown2 { get; set; }

        [Serialize]
        private int Unknown3 { get; set; }

        [Serialize]
        public int BuildNumber { get; set; }
    }
}