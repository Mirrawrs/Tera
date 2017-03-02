using Lotus.Serialization.Attributes;

namespace Tera
{
    public class User
    {
        [Serialize]
        public int Unknown1 { get; set; }
        [Serialize]
        public string Name { get; set; }
        [Serialize]
        public ushort Detail1Pointer { get; set; }
        [Serialize]
        public ushort Detail1Length { get; set; }
        [Serialize]
        public ushort Detail2Pointer { get; set; }
        [Serialize]
        public ushort Detail2Length { get; set; }
        [Serialize]
        public ushort LastCharacterBytePointer { get; set; }
        [Serialize]
        public int Id { get; set; }
        [Serialize]
        public Gender Gender { get; set; }
        [Serialize]
        public Race Race { get; set; }
        [Serialize]
        public PlayerClass Class { get; set; }
        [Serialize]
        public int Level { get; set; }
    }
}
