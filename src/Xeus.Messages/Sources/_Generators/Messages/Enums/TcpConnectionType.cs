using System;
using System.Runtime.Serialization;

namespace Amoeba.Messages
{
    [Flags]
    public enum TcpConnectionType
    {
        None = 0,
        Ipv4 = 0x01,
        Ipv6 = 0x02,
    }
}
