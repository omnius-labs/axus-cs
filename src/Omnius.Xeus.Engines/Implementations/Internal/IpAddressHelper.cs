using System.Net;
using System.Net.Sockets;
using Omnius.Core;

namespace Omnius.Xeus.Engines.Internal;

internal static class IpAddressHelper
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private static readonly ReadOnlyMemory<byte> _ipAddress_10_0_0_0 = IPAddress.Parse("10.0.0.0").GetAddressBytes();
    private static readonly ReadOnlyMemory<byte> _ipAddress_10_255_255_255 = IPAddress.Parse("10.255.255.255").GetAddressBytes();
    private static readonly ReadOnlyMemory<byte> _ipAddress_172_16_0_0 = IPAddress.Parse("172.16.0.0").GetAddressBytes();
    private static readonly ReadOnlyMemory<byte> _ipAddress_172_31_255_255 = IPAddress.Parse("172.31.255.255").GetAddressBytes();
    private static readonly ReadOnlyMemory<byte> _ipAddress_127_0_0_0 = IPAddress.Parse("127.0.0.0").GetAddressBytes();
    private static readonly ReadOnlyMemory<byte> _ipAddress_127_255_255_255 = IPAddress.Parse("127.255.255.255").GetAddressBytes();
    private static readonly ReadOnlyMemory<byte> _ipAddress_192_168_0_0 = IPAddress.Parse("192.168.0.0").GetAddressBytes();
    private static readonly ReadOnlyMemory<byte> _ipAddress_192_168_255_255 = IPAddress.Parse("192.168.255.255").GetAddressBytes();
    private static readonly ReadOnlyMemory<byte> _ipAddress_169_254_0_0 = IPAddress.Parse("169.254.0.0").GetAddressBytes();
    private static readonly ReadOnlyMemory<byte> _ipAddress_169_254_255_255 = IPAddress.Parse("169.254.255.255").GetAddressBytes();

    public static bool IsGlobalIpAddress(IPAddress ipAddress)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            if (ipAddress == IPAddress.Any || ipAddress == IPAddress.Loopback || ipAddress == IPAddress.Broadcast) return false;

            var bytes = ipAddress.GetAddressBytes();

            // Loopback Address
            if (BytesOperations.Compare(bytes, _ipAddress_127_0_0_0.Span) >= 0
                && BytesOperations.Compare(bytes, _ipAddress_127_255_255_255.Span) <= 0)
            {
                return false;
            }

            // Class A
            if (BytesOperations.Compare(bytes, _ipAddress_10_0_0_0.Span) >= 0
                && BytesOperations.Compare(bytes, _ipAddress_10_255_255_255.Span) <= 0)
            {
                return false;
            }

            // Class B
            if (BytesOperations.Compare(bytes, _ipAddress_172_16_0_0.Span) >= 0
                && BytesOperations.Compare(bytes, _ipAddress_172_31_255_255.Span) <= 0)
            {
                return false;
            }

            // Class C
            if (BytesOperations.Compare(bytes, _ipAddress_192_168_0_0.Span) >= 0
                && BytesOperations.Compare(bytes, _ipAddress_192_168_255_255.Span) <= 0)
            {
                return false;
            }

            // Link Local Address
            if (BytesOperations.Compare(bytes, _ipAddress_169_254_0_0.Span) >= 0
                && BytesOperations.Compare(bytes, _ipAddress_169_254_255_255.Span) <= 0)
            {
                return false;
            }
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (ipAddress == IPAddress.IPv6Any || ipAddress == IPAddress.IPv6Loopback || ipAddress == IPAddress.IPv6None
                || ipAddress.IsIPv4MappedToIPv6 || ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6Multicast || ipAddress.IsIPv6SiteLocal || ipAddress.IsIPv6Teredo)
            {
                return false;
            }
        }

        return true;
    }

    public static IEnumerable<IPAddress> GetMyGlobalIpAddresses()
    {
        var list = new HashSet<IPAddress>();

        try
        {
            list.UnionWith(Dns.GetHostAddresses(Dns.GetHostName()).Where(n => IsGlobalIpAddress(n)));
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }

        return list;
    }
}
