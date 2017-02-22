using Lotus.Serialization.Attributes;

namespace Tera.Analytics
{
    /// <summary>
    ///     Represents the header of the Data Center.
    /// </summary>
    public class DataCenterHeader
    {
        [Serialize(0)]
        public int Unknown1 { get; set; }

        [Serialize(1)]
        public int Unknown2 { get; set; }

        [Serialize(2)]
        public int Unknown3 { get; set; }

        [Serialize(3)]
        public int Unknown4 { get; set; }

        [Serialize(4)]
        public int Unknown5 { get; set; }

        [Serialize(5)]
        public int Unknown6 { get; set; }

        [Serialize(6)]
        public int Unknown7 { get; set; }

        [Serialize(7)]
        public int Unknown8 { get; set; }
    }
}