using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Lotus.Serialization.Attributes;
using Tera.Analytics.DirectiveSelectors;
using Tera.Analytics.Unpacking;

namespace Tera.Analytics
{
    /// <summary>
    ///     Represents the unpacked Data Center.
    /// </summary>
    /// <remarks>
    ///     The Data Center is a polytree in which the vertices are represented by instances of the
    ///     <see cref="DataCenterElement" /> class. Both the children of a vertex and its attributes are contiguously mapped
    ///     in, respectively, <see cref="Elements" /> and <see cref="Attributes" />.
    /// </remarks>
    public class DataCenter
    {
        /// <summary>
        ///     Gets the <see cref="DataCenter" />'s header.
        /// </summary>
        [Serialize(0)]
        public DataCenterHeader Header { get; private set; }


        [Serialize(1)]
        public List<Unknown> Unknown { get; private set; }

        /// <summary>
        ///     Gets the region containing the elements' attributes.
        /// </summary>
        [Serialize(2)]
        public Region<DataCenterAttribute> Attributes { get; private set; }

        /// <summary>
        ///     Gets the region containing the elements. The first entry is always the root of the Data Center.
        /// </summary>
        [Serialize(3)]
        public Region<DataCenterElement> Elements { get; private set; }

        /// <summary>
        ///     Gets the region containing zero-terminated strings used as attributes' values.
        /// </summary>
        [Serialize(4)]
        public StringsRegion ValuesRegion { get; private set; }

        /// <summary>
        ///     Gets a jagged list of <see cref="StringMetadata" /> related to <see cref="ValuesRegion" />. There are always 1024
        ///     nested lists.
        /// </summary>
        [Serialize(5)]
        [ValuesMetadata]
        public List<List<StringMetadata>> ValuesMetadata { get; private set; }

        /// <summary>
        ///     Gets a list of indices that point to the first character of a string contained in <see cref="ValuesRegion" />.
        /// </summary>
        [Serialize(6)]
        [OffByOne]
        public List<DataCenterIndex> ValueIndices { get; private set; }

        /// <summary>
        ///     Gets the region containing zero-terminated strings used as names for attributes and elements.
        /// </summary>
        [Serialize(7)]
        public StringsRegion NamesRegion { get; private set; }

        /// <summary>
        ///     Gets a jagged list of <see cref="StringMetadata" /> related to <see cref="NamesRegion" />. There are always 512
        ///     nested lists.
        /// </summary>
        [Serialize(8)]
        [NamesMetadata]
        public List<List<StringMetadata>> NamesMetadata { get; private set; }

        /// <summary>
        ///     Gets a list of indices that point to the first character of a string contained in <see cref="NameIndices" />.
        /// </summary>
        [Serialize(9)]
        [OffByOne]
        public List<DataCenterIndex> NameIndices { get; private set; }

        /// <summary>
        ///     Gets a list of strings used as names for attributes and elements.
        /// </summary>
        /// <remarks>
        ///     Unlike for string values, name lookups are performed by index on this list.
        /// </remarks>
        public List<string> Names { get; set; }

        /// <summary>
        ///     Gets the root of the Data Center.
        /// </summary>
        public DataCenterElement Root => Elements[default(DataCenterIndex)];

        /// <summary>
        ///     Extracts the key and initialization vector to decrypt the Data Center from the game client, then unpacks the Data
        ///     Center.
        /// </summary>
        /// <param name="path">The path to the Data Center file.</param>
        /// <param name="gamePath">The path to the game client.</param>
        /// <param name="cancellationToken">
        ///     The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
        /// </param>
        /// <returns>
        ///     A task that represents the Data Center's loading. The task's <see cref="Task{T}.Result" /> contains the
        ///     unpacked Data Center.
        /// </returns>
        public static async Task<DataCenter> Load(
            string path,
            string gamePath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var analyzer = new GameClientAnalyzer();
            var info = await analyzer.Analyze(gamePath, cancellationToken);
            return await Load(path, info.DataCenterKey, info.DataCenterIv, cancellationToken);
        }

        /// <summary>
        ///     Unpacks the Data Center using the given key and initialization vector.
        /// </summary>
        /// <param name="path">The path to the Data Center file.</param>
        /// <param name="key">The key that will be used to decrypt the Data Center.</param>
        /// <param name="iv">The initialization vector that will be used to decrypt the Data Center.</param>
        /// <param name="cancellationToken">
        ///     The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
        /// </param>
        /// <returns>
        ///     A task that represents the Data Center's loading. The task's <see cref="Task{T}.Result" /> contains the
        ///     unpacked Data Center.
        /// </returns>
        public static async Task<DataCenter> Load(
            string path,
            byte[] key,
            byte[] iv,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            async Task<Stream> Unpack()
            {
                var unpacker = new DataCenterUnpacker(key, iv);
                using (var fileStream = File.OpenRead(path))
                using (var deflateStream = unpacker.Unpack(fileStream))
                {
                    var memoryStream = new MemoryStream();
                    await deflateStream.CopyToAsync(memoryStream, 0x14000, cancellationToken);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream;
                }
            }

            var dataCenterStream = await Unpack();
            var reader = new DataCenterReader(dataCenterStream);
            return reader.ReadDataCenter();
        }
    }
}