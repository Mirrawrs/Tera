namespace Tera.Packets
{
    public class UnknownPacket : Packet
    {
        public string PacketName { get; }
        public byte[] RawPacketData { get; }

        public UnknownPacket(string packetName, byte[] rawPacketData)
        {
            PacketName = packetName;
            RawPacketData = rawPacketData;
        }
    }
}