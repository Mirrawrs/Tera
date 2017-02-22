using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lotus.Serialization;
using Lotus.Serialization.Attributes;
using Tera.Analytics.DirectiveSelectors;

namespace Tera.Analytics
{
    /// <summary>
    ///     Reads primitive data types from the Data Center's stream.
    /// </summary>
    public class DataCenterReader
    {
        private readonly StringBuilder builder = new StringBuilder();
        private readonly SerializerMediator mediator;
        private readonly BinaryReader reader;
        private readonly Stream stream;
        private DataCenter dataCenter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataCenterReader" /> class targeting the specified Data Center's
        ///     stream.
        /// </summary>
        /// <param name="stream">The Data Center's stream.</param>
        public DataCenterReader(Stream stream)
        {
            this.stream = stream;
            reader = new BinaryReader(stream, Encoding.Unicode);
            mediator = new SerializerMediator(this);
        }

        private long CurrentAddress
        {
            [Reader] [CurrentAddress] get => stream.Position;
            [Writer] [CurrentAddress] set => stream.Position = value;
        }

        //Used by lazy components to provide a delegate to read a given type at the specified address.
        [Reader]
        private ReadAtAddressDelegate<TOutput> GetReadAt<TOutput>()
        {
            var read = mediator.GetReadFor<TOutput>();
            return address =>
            {
                CurrentAddress = address;
                return read();
            };
        }

        /// <summary>
        ///     Reads the Data Center structure from the stream.
        /// </summary>
        /// <returns>The deserialized Data Center.</returns>
        public DataCenter ReadDataCenter() => mediator.Read<DataCenter>();

        //Used to retrieve a reference to the names an values region when reading attributes and elements.
        [BeforeReading]
        private void GetReferenceToDataCenter(DataCenter dc) => dataCenter = dc;

        [Reader]
        private char ReadChar() => reader.ReadChar();

        [Reader]
        private float ReadSingle() => reader.ReadSingle();

        [Reader]
        private ushort ReadUInt16() => reader.ReadUInt16();

        [Reader]
        private int ReadInt32() => reader.ReadInt32();

        [Reader]
        private bool ReadBoolean() => ReadInt32() == 1;

        [Reader]
        private List<TOutput> ReadList<TOutput>() => ReadList<TOutput>(ReadInt32());

        //Used to read the list of indices after strings' metadata which is prefixed by an index that is off by one.
        [OffByOne]
        [Reader]
        private List<DataCenterIndex> ReadIndices() => ReadList<DataCenterIndex>(ReadInt32() - 1);

        private List<TOutput> ReadList<TOutput>(int count)
        {
            return Enumerable.Repeat(mediator.GetReadFor<TOutput>(), count)
                .Select(readItem => readItem())
                .ToList();
        }

        //To allow lazy loading of elements and attributes, they are initialized as enumerables here.
        [AfterReading]
        private void InitializeElementEnumerables(DataCenterElement element)
        {
            IEnumerable<TOutput> ReadLazy<TOutput>(
                Region<TOutput> region,
                DataCenterIndex regionIndex,
                int count)
            {
                for (var i = 0; i < count; i++)
                {
                    yield return region[regionIndex];
                    if (++regionIndex.ItemIndex < region.Buckets[regionIndex.BucketIndex].Count) continue;
                    //If the end of a bucket is reached, advance to the next one.
                    regionIndex.BucketIndex++;
                    regionIndex.ItemIndex = 0;
                }
            }

            var attributesCount = ReadUInt16();
            var childrenCount = ReadUInt16();
            var firstAttributeIndex = mediator.Read<DataCenterIndex>();
            var firstChildIndex = mediator.Read<DataCenterIndex>();
            element.Attributes = ReadLazy(dataCenter.Attributes, firstAttributeIndex, attributesCount);
            element.Children = ReadLazy(dataCenter.Elements, firstChildIndex, childrenCount);
        }

        [Reader]
        private object ReadAttributeValue()
        {
            switch (mediator.Read<TypeCode>())
            {
                case TypeCode.Int:
                    return ReadInt32();
                case TypeCode.Float:
                    return ReadSingle();
                case TypeCode.Bool:
                    return ReadBoolean();
                default:
                    var index = mediator.Read<DataCenterIndex>();
                    //Null propagation operator because the first time a value is read is before the region is mapped (see SetBucketSizeAndSkip).
                    return dataCenter.ValuesRegion?[index];
            }
        }

        //The index isn't pointing to a region but to a flattened list of names.
        [Reader]
        [NameString]
        private string ReadIndexPrefixedNameString()
        {
            var nameIndex = ReadUInt16();
            //Null propagation operator because the first time a name is read is before the region is mapped (see SetBucketSizeAndSkip).
            return dataCenter.Names?[nameIndex];
        }

        [Reader]
        private string ReadNullTerminatedString()
        {
            builder.Clear();
            while (reader.ReadChar() is var c && c > 0) builder.Append(c);
            return builder.ToString();
        }

        [Reader]
        [NamesMetadata]
        private List<List<StringMetadata>> ReadNamesMetadata() => ReadMetadata(512, true);

        [Reader]
        [ValuesMetadata]
        private List<List<StringMetadata>> ReadValuesMetadata() => ReadMetadata(1024, true);

        private List<List<StringMetadata>> ReadMetadata(int listsCount, bool skip)
        {
            //Since the strings can be efficiently read without their metadata, allow to skip its deserialization.
            void Skip()
            {
                const int metadataSize = 16;
                for (var i = 0; i < listsCount; i++)
                {
                    var count = ReadInt32();
                    CurrentAddress += count * metadataSize;
                }
            }

            var lists = new List<List<StringMetadata>>(listsCount);
            if (skip) Skip();
            else for (var i = 0; i < listsCount; i++) lists.Add(ReadList<StringMetadata>());
            return lists;
        }

        //While values are accessed by their region index, element and attribute names are loaded in memory in a flat list, a placeholder string is prepended, and they're accessed by index.
        [AfterReading]
        public void FlattenNamesRegion(DataCenter dc)
            => dataCenter.Names = new[] {"__placeholder__"}
                .Concat(dc.NameIndices.Select(index => dc.NamesRegion[index]))
                .ToList();

        //The item's size is needed to determine the bucket's length in bytes, so a sample item is read and the stream's position is advanced to the end of the bucket.
        [AfterReading]
        public void SetBucketSizeAndSkip<T>(Bucket<T> bucket)
        {
            var previousAddress = CurrentAddress;
            var sampleValue = mediator.Read<T>();
            bucket.ValueSize = (int) (CurrentAddress - previousAddress);
            CurrentAddress = bucket.FirstValueAddress + bucket.Capacity * bucket.ValueSize;
        }

        internal delegate TOutput ReadAtAddressDelegate<out TOutput>(int address);
    }
}