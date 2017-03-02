using System.Collections.Generic;
using Lotus.Serialization.Attributes;

namespace Tera.Packets
{
    public class S_GET_USER_LIST : Packet
    {
        [Serialize]
        public List<User> Users { get; set; }
    }
}