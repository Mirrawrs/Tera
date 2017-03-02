using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Lotus.Serialization;
using Lotus.Serialization.Attributes;
using Tera.DirectiveSelectors;
using Tera.Packets;

namespace Tera.Net
{
    /// <summary>
    ///     Provides methods to serialize and deserialize TERA packets. Used in conjunction with
    ///     <see cref="SerializerMediator" />.
    /// </summary>
    internal class TeraSerializer
    {
        private readonly SerializerMediator mediator;
        private readonly IReadOnlyDictionary<Type, ushort> opcodesByPacketType;
        private readonly IReadOnlyDictionary<ushort, string> packetNamesByOpcode;
        private readonly IReadOnlyDictionary<ushort, Type> packetTypesByOpcode;
        private readonly BinaryReader reader;
        private readonly MemoryStream stream;

        private readonly Queue<(long pointerAddress, string value, Encoding encoding)> strings =
            new Queue<(long, string, Encoding)>();

        private readonly BinaryWriter writer;

        public TeraSerializer(MemoryStream stream, TeraClientConfiguration configuration)
        {
            this.stream = stream;
            reader = new BinaryReader(this.stream, Encoding.Unicode);
            writer = new BinaryWriter(this.stream, Encoding.Unicode);
            mediator = new SerializerMediator(this);
            packetNamesByOpcode = configuration.PacketNamesByOpcode;
            var opcodesByPacketNames = packetNamesByOpcode.ToDictionary(pair => pair.Value, pair => pair.Key);
            packetTypesByOpcode = typeof(Packet).GetTypeInfo()
                .Assembly.GetTypes()
                .Where(type => type.GetTypeInfo().IsSubclassOf(typeof(Packet)) && type != typeof(UnknownPacket))
                .ToDictionary(type => opcodesByPacketNames[type.Name]);
            opcodesByPacketType = packetTypesByOpcode.ToDictionary(pair => pair.Value, pair => pair.Key);
        }

        public Packet ReadPacket()
        {
            stream.Seek(0, SeekOrigin.Begin);
            var packetSize = mediator.Read<ushort>();
            var opcode = mediator.Read<ushort>();
            if (packetTypesByOpcode.TryGetValue(opcode, out var packetType))
                return (Packet) mediator.Read(packetType);
            var rawPacketData = new byte[packetSize];
            stream.Read(rawPacketData, 0, rawPacketData.Length);
            return new UnknownPacket(packetNamesByOpcode[opcode], rawPacketData);
        }

        public int WritePacket(Packet packet)
        {
            const uint headerLength = 4;
            stream.Seek(headerLength, SeekOrigin.Begin);
            mediator.Write(packet.GetType(), packet);
            var packetSize = (ushort) stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
            mediator.Write(packetSize);
            if (!opcodesByPacketType.TryGetValue(packet.GetType(), out var opcode))
                throw new Exception($"No opcode found for type {packet.GetType().Name}.");
            mediator.Write(opcode);
            return packetSize;
        }

        [Reader]
        private byte ReadByte() => reader.ReadByte();

        [Writer]
        private void WriteByte(byte value) => writer.Write(value);

        [Reader]
        private bool ReadBoolean() => reader.ReadBoolean();

        [Writer]
        private void WriteBoolean(bool value) => writer.Write(value);

        [Reader]
        private ushort ReadUInt16() => reader.ReadUInt16();

        [Writer]
        private void WriteUInt16(ushort value) => writer.Write(value);

        [Reader]
        private int ReadInt32() => reader.ReadInt32();

        [Writer]
        private void WriteInt32(int value) => writer.Write(value);

        [Reader]
        private ulong ReadUInt64() => reader.ReadUInt64();

        [Writer]
        private void WriteUInt64(ulong value) => writer.Write(value);

        [Reader]
        private string ReadUnicodeString()
        {
            var stringAddress = mediator.Read<ushort>();
            var oldPosition = stream.Position;
            stream.Seek(stringAddress, SeekOrigin.Begin);
            var builder = new StringBuilder();
            while (reader.ReadChar() is var c && c > 0) builder.Append(c);
            var value = builder.ToString();
            stream.Seek(oldPosition, SeekOrigin.Begin);
            return value;
        }

        [Writer]
        private void WriteUnicodeString(string value)
        {
            strings.Enqueue((stream.Position, value + '\x0', Encoding.Unicode));
            stream.Seek(2, SeekOrigin.Current);
        }

        [ASCII]
        [Writer]
        private void WriteASCIIString(string value)
        {
            strings.Enqueue((stream.Position, value, Encoding.ASCII));
            stream.Seek(2, SeekOrigin.Current);
        }

        [Reader]
        private List<TItem> ReadList<TItem>()
        {
            var list = new List<TItem>();
            var count = mediator.Read<ushort>();
            var oldPosition = stream.Position;
            var nextItemAddress = mediator.Read<ushort>();
            for (var i = 0; i < count; i++)
            {
                stream.Seek(nextItemAddress, SeekOrigin.Begin);
                var addressToSelf = mediator.Read<ushort>();
                nextItemAddress = mediator.Read<ushort>();
                var item = mediator.Read<TItem>();
                list.Add(item);
            }
            stream.Seek(oldPosition, SeekOrigin.Begin);
            return list;
        }

        [AfterWriting]
        private void WriteStringsAndPointers(Packet packet)
        {
            while (strings.Count > 0)
            {
                var (pointerAddress, value, encoding) = strings.Dequeue();
                var address = (ushort) stream.Position;
                stream.Position = pointerAddress;
                mediator.Write(address);
                stream.Position = address;
                writer.Write(encoding.GetBytes(value));
            }
        }

        [Reader]
        private ModelInfo ReadModelInfo()
        {
            var value = mediator.Read<int>();
            value -= 10101;
            var race = value / 200;
            value -= 200 * race;
            var gender = value / 100;
            value -= 100 * gender;
            var playerClass = (PlayerClass) value;
            return new ModelInfo
            {
                Race = (Race) race,
                Gender = (Gender) gender,
                Class = playerClass
            };
        }
    }
}