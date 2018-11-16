using System.Runtime.Serialization;

namespace Amoeba.Messages
{
    public enum ConnectionType
    {
        None = 0,
        Tcp = 1,
        Socks5Proxy = 2,
        HttpProxy = 3,
    }
}
