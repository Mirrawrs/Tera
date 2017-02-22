using System;
using System.Collections.Generic;

namespace Tera.Analytics
{
    [Serializable]
    public class AnalysisResult
    {
        public IReadOnlyDictionary<ushort, string> PacketNamesByOpcode { get; set; }
        public IList<string> SystemMessageReadableIds { get; set; }
        public byte[] DataCenterKey { get; set; }
        public byte[] DataCenterIv { get; set; }
    }
}