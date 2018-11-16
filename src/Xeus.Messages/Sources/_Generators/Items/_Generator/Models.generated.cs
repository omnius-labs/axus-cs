using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Omnius.Base;
using Omnius.Net;
using Omnius.Net.Secure;
using Omnius.Security;
using Omnius.Serialization;
using Omnius.Utils;

namespace Amoeba.Messages
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class MessageCondition : ItemBase<MessageCondition>
    {
        [JsonConstructor]
        public MessageCondition(Signature authorSignature, DateTime creationTime)
        {
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
        }
        [JsonProperty]
        public Signature AuthorSignature { get; }
        private DateTime _creationTime;
        [JsonProperty]
        public DateTime CreationTime
        {
            get => _creationTime;
            private set => _creationTime = value.Normalize();
        }
        public override bool Equals(MessageCondition target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.AuthorSignature != default(Signature)) h ^= this.AuthorSignature.GetHashCode();
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class ConnectionFilter : ItemBase<ConnectionFilter>
    {
        [JsonConstructor]
        public ConnectionFilter(string scheme, ConnectionType type, string proxyUri)
        {
            this.Scheme = scheme;
            this.Type = type;
            this.ProxyUri = proxyUri;
        }
        [JsonProperty]
        public string Scheme { get; }
        [JsonProperty]
        public ConnectionType Type { get; }
        [JsonProperty]
        public string ProxyUri { get; }
        public override bool Equals(ConnectionFilter target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Scheme != target.Scheme) return false;
            if (this.Type != target.Type) return false;
            if (this.ProxyUri != target.ProxyUri) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Scheme != default(string)) h ^= this.Scheme.GetHashCode();
                if (this.Type != default(ConnectionType)) h ^= this.Type.GetHashCode();
                if (this.ProxyUri != default(string)) h ^= this.ProxyUri.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CheckBlocksProgressReport : ItemBase<CheckBlocksProgressReport>
    {
        [JsonConstructor]
        public CheckBlocksProgressReport(long badCount, long checkedCount, long blockCount)
        {
            this.BadCount = badCount;
            this.CheckedCount = checkedCount;
            this.BlockCount = blockCount;
        }
        [JsonProperty]
        public long BadCount { get; }
        [JsonProperty]
        public long CheckedCount { get; }
        [JsonProperty]
        public long BlockCount { get; }
        public override bool Equals(CheckBlocksProgressReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.BadCount != target.BadCount) return false;
            if (this.CheckedCount != target.CheckedCount) return false;
            if (this.BlockCount != target.BlockCount) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.BadCount != default(long)) h ^= this.BadCount.GetHashCode();
                if (this.CheckedCount != default(long)) h ^= this.CheckedCount.GetHashCode();
                if (this.BlockCount != default(long)) h ^= this.BlockCount.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class ServiceReport : ItemBase<ServiceReport>
    {
        [JsonConstructor]
        public ServiceReport(CoreReport core, ConnectionReport connection)
        {
            this.Core = core;
            this.Connection = connection;
        }
        [JsonProperty]
        public CoreReport Core { get; }
        [JsonProperty]
        public ConnectionReport Connection { get; }
        public override bool Equals(ServiceReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Core != target.Core) return false;
            if (this.Connection != target.Connection) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Core != default(CoreReport)) h ^= this.Core.GetHashCode();
                if (this.Connection != default(ConnectionReport)) h ^= this.Connection.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CoreReport : ItemBase<CoreReport>
    {
        [JsonConstructor]
        public CoreReport(CacheReport cache, NetworkReport network)
        {
            this.Cache = cache;
            this.Network = network;
        }
        [JsonProperty]
        public CacheReport Cache { get; }
        [JsonProperty]
        public NetworkReport Network { get; }
        public override bool Equals(CoreReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Cache != target.Cache) return false;
            if (this.Network != target.Network) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Cache != default(CacheReport)) h ^= this.Cache.GetHashCode();
                if (this.Network != default(NetworkReport)) h ^= this.Network.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CacheReport : ItemBase<CacheReport>
    {
        [JsonConstructor]
        public CacheReport(long blockCount, long usingSpace, long lockSpace, long freeSpace)
        {
            this.BlockCount = blockCount;
            this.UsingSpace = usingSpace;
            this.LockSpace = lockSpace;
            this.FreeSpace = freeSpace;
        }
        [JsonProperty]
        public long BlockCount { get; }
        [JsonProperty]
        public long UsingSpace { get; }
        [JsonProperty]
        public long LockSpace { get; }
        [JsonProperty]
        public long FreeSpace { get; }
        public override bool Equals(CacheReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.BlockCount != target.BlockCount) return false;
            if (this.UsingSpace != target.UsingSpace) return false;
            if (this.LockSpace != target.LockSpace) return false;
            if (this.FreeSpace != target.FreeSpace) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.BlockCount != default(long)) h ^= this.BlockCount.GetHashCode();
                if (this.UsingSpace != default(long)) h ^= this.UsingSpace.GetHashCode();
                if (this.LockSpace != default(long)) h ^= this.LockSpace.GetHashCode();
                if (this.FreeSpace != default(long)) h ^= this.FreeSpace.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class NetworkReport : ItemBase<NetworkReport>
    {
        [JsonConstructor]
        public NetworkReport(DirectExchangeReport directExchange, RelayExchangeReport indirectExchange)
        {
            this.DirectExchange = directExchange;
            this.RelayExchange = indirectExchange;
        }
        [JsonProperty]
        public DirectExchangeReport DirectExchange { get; }
        [JsonProperty]
        public RelayExchangeReport RelayExchange { get; }
        public override bool Equals(NetworkReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.DirectExchange != target.DirectExchange) return false;
            if (this.RelayExchange != target.RelayExchange) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.DirectExchange != default(DirectExchangeReport)) h ^= this.DirectExchange.GetHashCode();
                if (this.RelayExchange != default(RelayExchangeReport)) h ^= this.RelayExchange.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class DirectExchangeReport : ItemBase<DirectExchangeReport>
    {
        [JsonConstructor]
        public DirectExchangeReport(Location myLocation, long connectCount, long acceptCount, int cloudNodeCount, int messageCount, int uploadBlockCount, int diffusionBlockCount, long totalReceivedByteCount, long totalSentByteCount, long pushLocationCount, long pushBlockLinkCount, long pushBlockRequestCount, long pushBlockResultCount, long pushMessageRequestCount, long pushMessageResultCount, long pullLocationCount, long pullBlockLinkCount, long pullBlockRequestCount, long pullBlockResultCount, long pullMessageRequestCount, long pullMessageResultCount)
        {
            this.MyLocation = myLocation;
            this.ConnectCount = connectCount;
            this.AcceptCount = acceptCount;
            this.CloudNodeCount = cloudNodeCount;
            this.MessageCount = messageCount;
            this.UploadBlockCount = uploadBlockCount;
            this.DiffusionBlockCount = diffusionBlockCount;
            this.TotalReceivedByteCount = totalReceivedByteCount;
            this.TotalSentByteCount = totalSentByteCount;
            this.PushLocationCount = pushLocationCount;
            this.PushBlockLinkCount = pushBlockLinkCount;
            this.PushBlockRequestCount = pushBlockRequestCount;
            this.PushBlockResultCount = pushBlockResultCount;
            this.PushMessageRequestCount = pushMessageRequestCount;
            this.PushMessageResultCount = pushMessageResultCount;
            this.PullLocationCount = pullLocationCount;
            this.PullBlockLinkCount = pullBlockLinkCount;
            this.PullBlockRequestCount = pullBlockRequestCount;
            this.PullBlockResultCount = pullBlockResultCount;
            this.PullMessageRequestCount = pullMessageRequestCount;
            this.PullMessageResultCount = pullMessageResultCount;
        }
        [JsonProperty]
        public Location MyLocation { get; }
        [JsonProperty]
        public long ConnectCount { get; }
        [JsonProperty]
        public long AcceptCount { get; }
        [JsonProperty]
        public int CloudNodeCount { get; }
        [JsonProperty]
        public int MessageCount { get; }
        [JsonProperty]
        public int UploadBlockCount { get; }
        [JsonProperty]
        public int DiffusionBlockCount { get; }
        [JsonProperty]
        public long TotalReceivedByteCount { get; }
        [JsonProperty]
        public long TotalSentByteCount { get; }
        [JsonProperty]
        public long PushLocationCount { get; }
        [JsonProperty]
        public long PushBlockLinkCount { get; }
        [JsonProperty]
        public long PushBlockRequestCount { get; }
        [JsonProperty]
        public long PushBlockResultCount { get; }
        [JsonProperty]
        public long PushMessageRequestCount { get; }
        [JsonProperty]
        public long PushMessageResultCount { get; }
        [JsonProperty]
        public long PullLocationCount { get; }
        [JsonProperty]
        public long PullBlockLinkCount { get; }
        [JsonProperty]
        public long PullBlockRequestCount { get; }
        [JsonProperty]
        public long PullBlockResultCount { get; }
        [JsonProperty]
        public long PullMessageRequestCount { get; }
        [JsonProperty]
        public long PullMessageResultCount { get; }
        public override bool Equals(DirectExchangeReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.MyLocation != target.MyLocation) return false;
            if (this.ConnectCount != target.ConnectCount) return false;
            if (this.AcceptCount != target.AcceptCount) return false;
            if (this.CloudNodeCount != target.CloudNodeCount) return false;
            if (this.MessageCount != target.MessageCount) return false;
            if (this.UploadBlockCount != target.UploadBlockCount) return false;
            if (this.DiffusionBlockCount != target.DiffusionBlockCount) return false;
            if (this.TotalReceivedByteCount != target.TotalReceivedByteCount) return false;
            if (this.TotalSentByteCount != target.TotalSentByteCount) return false;
            if (this.PushLocationCount != target.PushLocationCount) return false;
            if (this.PushBlockLinkCount != target.PushBlockLinkCount) return false;
            if (this.PushBlockRequestCount != target.PushBlockRequestCount) return false;
            if (this.PushBlockResultCount != target.PushBlockResultCount) return false;
            if (this.PushMessageRequestCount != target.PushMessageRequestCount) return false;
            if (this.PushMessageResultCount != target.PushMessageResultCount) return false;
            if (this.PullLocationCount != target.PullLocationCount) return false;
            if (this.PullBlockLinkCount != target.PullBlockLinkCount) return false;
            if (this.PullBlockRequestCount != target.PullBlockRequestCount) return false;
            if (this.PullBlockResultCount != target.PullBlockResultCount) return false;
            if (this.PullMessageRequestCount != target.PullMessageRequestCount) return false;
            if (this.PullMessageResultCount != target.PullMessageResultCount) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.MyLocation != default(Location)) h ^= this.MyLocation.GetHashCode();
                if (this.ConnectCount != default(long)) h ^= this.ConnectCount.GetHashCode();
                if (this.AcceptCount != default(long)) h ^= this.AcceptCount.GetHashCode();
                if (this.CloudNodeCount != default(int)) h ^= this.CloudNodeCount.GetHashCode();
                if (this.MessageCount != default(int)) h ^= this.MessageCount.GetHashCode();
                if (this.UploadBlockCount != default(int)) h ^= this.UploadBlockCount.GetHashCode();
                if (this.DiffusionBlockCount != default(int)) h ^= this.DiffusionBlockCount.GetHashCode();
                if (this.TotalReceivedByteCount != default(long)) h ^= this.TotalReceivedByteCount.GetHashCode();
                if (this.TotalSentByteCount != default(long)) h ^= this.TotalSentByteCount.GetHashCode();
                if (this.PushLocationCount != default(long)) h ^= this.PushLocationCount.GetHashCode();
                if (this.PushBlockLinkCount != default(long)) h ^= this.PushBlockLinkCount.GetHashCode();
                if (this.PushBlockRequestCount != default(long)) h ^= this.PushBlockRequestCount.GetHashCode();
                if (this.PushBlockResultCount != default(long)) h ^= this.PushBlockResultCount.GetHashCode();
                if (this.PushMessageRequestCount != default(long)) h ^= this.PushMessageRequestCount.GetHashCode();
                if (this.PushMessageResultCount != default(long)) h ^= this.PushMessageResultCount.GetHashCode();
                if (this.PullLocationCount != default(long)) h ^= this.PullLocationCount.GetHashCode();
                if (this.PullBlockLinkCount != default(long)) h ^= this.PullBlockLinkCount.GetHashCode();
                if (this.PullBlockRequestCount != default(long)) h ^= this.PullBlockRequestCount.GetHashCode();
                if (this.PullBlockResultCount != default(long)) h ^= this.PullBlockResultCount.GetHashCode();
                if (this.PullMessageRequestCount != default(long)) h ^= this.PullMessageRequestCount.GetHashCode();
                if (this.PullMessageResultCount != default(long)) h ^= this.PullMessageResultCount.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class RelayExchangeReport : ItemBase<RelayExchangeReport>
    {
        [JsonConstructor]
        public RelayExchangeReport(Location myLocation, long connectCount, long acceptCount, int cloudNodeCount, int messageCount, int uploadBlockCount, int diffusionBlockCount, long totalReceivedByteCount, long totalSentByteCount, long pushLocationCount, long pushBlockLinkCount, long pushBlockRequestCount, long pushBlockResultCount, long pushMessageRequestCount, long pushMessageResultCount, long pullLocationCount, long pullBlockLinkCount, long pullBlockRequestCount, long pullBlockResultCount, long pullMessageRequestCount, long pullMessageResultCount)
        {
            this.MyLocation = myLocation;
            this.ConnectCount = connectCount;
            this.AcceptCount = acceptCount;
            this.CloudNodeCount = cloudNodeCount;
            this.MessageCount = messageCount;
            this.UploadBlockCount = uploadBlockCount;
            this.DiffusionBlockCount = diffusionBlockCount;
            this.TotalReceivedByteCount = totalReceivedByteCount;
            this.TotalSentByteCount = totalSentByteCount;
            this.PushLocationCount = pushLocationCount;
            this.PushBlockLinkCount = pushBlockLinkCount;
            this.PushBlockRequestCount = pushBlockRequestCount;
            this.PushBlockResultCount = pushBlockResultCount;
            this.PushMessageRequestCount = pushMessageRequestCount;
            this.PushMessageResultCount = pushMessageResultCount;
            this.PullLocationCount = pullLocationCount;
            this.PullBlockLinkCount = pullBlockLinkCount;
            this.PullBlockRequestCount = pullBlockRequestCount;
            this.PullBlockResultCount = pullBlockResultCount;
            this.PullMessageRequestCount = pullMessageRequestCount;
            this.PullMessageResultCount = pullMessageResultCount;
        }
        [JsonProperty]
        public Location MyLocation { get; }
        [JsonProperty]
        public long ConnectCount { get; }
        [JsonProperty]
        public long AcceptCount { get; }
        [JsonProperty]
        public int CloudNodeCount { get; }
        [JsonProperty]
        public int MessageCount { get; }
        [JsonProperty]
        public int UploadBlockCount { get; }
        [JsonProperty]
        public int DiffusionBlockCount { get; }
        [JsonProperty]
        public long TotalReceivedByteCount { get; }
        [JsonProperty]
        public long TotalSentByteCount { get; }
        [JsonProperty]
        public long PushLocationCount { get; }
        [JsonProperty]
        public long PushBlockLinkCount { get; }
        [JsonProperty]
        public long PushBlockRequestCount { get; }
        [JsonProperty]
        public long PushBlockResultCount { get; }
        [JsonProperty]
        public long PushMessageRequestCount { get; }
        [JsonProperty]
        public long PushMessageResultCount { get; }
        [JsonProperty]
        public long PullLocationCount { get; }
        [JsonProperty]
        public long PullBlockLinkCount { get; }
        [JsonProperty]
        public long PullBlockRequestCount { get; }
        [JsonProperty]
        public long PullBlockResultCount { get; }
        [JsonProperty]
        public long PullMessageRequestCount { get; }
        [JsonProperty]
        public long PullMessageResultCount { get; }
        public override bool Equals(RelayExchangeReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.MyLocation != target.MyLocation) return false;
            if (this.ConnectCount != target.ConnectCount) return false;
            if (this.AcceptCount != target.AcceptCount) return false;
            if (this.CloudNodeCount != target.CloudNodeCount) return false;
            if (this.MessageCount != target.MessageCount) return false;
            if (this.UploadBlockCount != target.UploadBlockCount) return false;
            if (this.DiffusionBlockCount != target.DiffusionBlockCount) return false;
            if (this.TotalReceivedByteCount != target.TotalReceivedByteCount) return false;
            if (this.TotalSentByteCount != target.TotalSentByteCount) return false;
            if (this.PushLocationCount != target.PushLocationCount) return false;
            if (this.PushBlockLinkCount != target.PushBlockLinkCount) return false;
            if (this.PushBlockRequestCount != target.PushBlockRequestCount) return false;
            if (this.PushBlockResultCount != target.PushBlockResultCount) return false;
            if (this.PushMessageRequestCount != target.PushMessageRequestCount) return false;
            if (this.PushMessageResultCount != target.PushMessageResultCount) return false;
            if (this.PullLocationCount != target.PullLocationCount) return false;
            if (this.PullBlockLinkCount != target.PullBlockLinkCount) return false;
            if (this.PullBlockRequestCount != target.PullBlockRequestCount) return false;
            if (this.PullBlockResultCount != target.PullBlockResultCount) return false;
            if (this.PullMessageRequestCount != target.PullMessageRequestCount) return false;
            if (this.PullMessageResultCount != target.PullMessageResultCount) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.MyLocation != default(Location)) h ^= this.MyLocation.GetHashCode();
                if (this.ConnectCount != default(long)) h ^= this.ConnectCount.GetHashCode();
                if (this.AcceptCount != default(long)) h ^= this.AcceptCount.GetHashCode();
                if (this.CloudNodeCount != default(int)) h ^= this.CloudNodeCount.GetHashCode();
                if (this.MessageCount != default(int)) h ^= this.MessageCount.GetHashCode();
                if (this.UploadBlockCount != default(int)) h ^= this.UploadBlockCount.GetHashCode();
                if (this.DiffusionBlockCount != default(int)) h ^= this.DiffusionBlockCount.GetHashCode();
                if (this.TotalReceivedByteCount != default(long)) h ^= this.TotalReceivedByteCount.GetHashCode();
                if (this.TotalSentByteCount != default(long)) h ^= this.TotalSentByteCount.GetHashCode();
                if (this.PushLocationCount != default(long)) h ^= this.PushLocationCount.GetHashCode();
                if (this.PushBlockLinkCount != default(long)) h ^= this.PushBlockLinkCount.GetHashCode();
                if (this.PushBlockRequestCount != default(long)) h ^= this.PushBlockRequestCount.GetHashCode();
                if (this.PushBlockResultCount != default(long)) h ^= this.PushBlockResultCount.GetHashCode();
                if (this.PushMessageRequestCount != default(long)) h ^= this.PushMessageRequestCount.GetHashCode();
                if (this.PushMessageResultCount != default(long)) h ^= this.PushMessageResultCount.GetHashCode();
                if (this.PullLocationCount != default(long)) h ^= this.PullLocationCount.GetHashCode();
                if (this.PullBlockLinkCount != default(long)) h ^= this.PullBlockLinkCount.GetHashCode();
                if (this.PullBlockRequestCount != default(long)) h ^= this.PullBlockRequestCount.GetHashCode();
                if (this.PullBlockResultCount != default(long)) h ^= this.PullBlockResultCount.GetHashCode();
                if (this.PullMessageRequestCount != default(long)) h ^= this.PullMessageRequestCount.GetHashCode();
                if (this.PullMessageResultCount != default(long)) h ^= this.PullMessageResultCount.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class ConnectionReport : ItemBase<ConnectionReport>
    {
        [JsonConstructor]
        public ConnectionReport(TcpConnectionReport tcp, CustomConnectionReport custom)
        {
            this.Tcp = tcp;
            this.Custom = custom;
        }
        [JsonProperty]
        public TcpConnectionReport Tcp { get; }
        [JsonProperty]
        public CustomConnectionReport Custom { get; }
        public override bool Equals(ConnectionReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Tcp != target.Tcp) return false;
            if (this.Custom != target.Custom) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Tcp != default(TcpConnectionReport)) h ^= this.Tcp.GetHashCode();
                if (this.Custom != default(CustomConnectionReport)) h ^= this.Custom.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class TcpConnectionReport : ItemBase<TcpConnectionReport>
    {
        [JsonConstructor]
        public TcpConnectionReport(long catharsisBlockCount)
        {
            this.CatharsisBlockCount = catharsisBlockCount;
        }
        [JsonProperty]
        public long CatharsisBlockCount { get; }
        public override bool Equals(TcpConnectionReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.CatharsisBlockCount != target.CatharsisBlockCount) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.CatharsisBlockCount != default(long)) h ^= this.CatharsisBlockCount.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CustomConnectionReport : ItemBase<CustomConnectionReport>
    {
        [JsonConstructor]
        public CustomConnectionReport(long catharsisBlockCount)
        {
            this.CatharsisBlockCount = catharsisBlockCount;
        }
        [JsonProperty]
        public long CatharsisBlockCount { get; }
        public override bool Equals(CustomConnectionReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.CatharsisBlockCount != target.CatharsisBlockCount) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.CatharsisBlockCount != default(long)) h ^= this.CatharsisBlockCount.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class DirectExchangeConnectionReport : ItemBase<DirectExchangeConnectionReport>
    {
        [JsonConstructor]
        public DirectExchangeConnectionReport(byte[] id, SecureConnectionType type, string uri, Location location, long priority, long receivedByteCount, long sentByteCount)
        {
            this.Id = id;
            this.Type = type;
            this.Uri = uri;
            this.Location = location;
            this.Priority = priority;
            this.ReceivedByteCount = receivedByteCount;
            this.SentByteCount = sentByteCount;
        }
        [JsonProperty]
        public byte[] Id { get; }
        [JsonProperty]
        public SecureConnectionType Type { get; }
        [JsonProperty]
        public string Uri { get; }
        [JsonProperty]
        public Location Location { get; }
        [JsonProperty]
        public long Priority { get; }
        [JsonProperty]
        public long ReceivedByteCount { get; }
        [JsonProperty]
        public long SentByteCount { get; }
        public override bool Equals(DirectExchangeConnectionReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Id != target.Id) return false;
            if (this.Type != target.Type) return false;
            if (this.Uri != target.Uri) return false;
            if (this.Location != target.Location) return false;
            if (this.Priority != target.Priority) return false;
            if (this.ReceivedByteCount != target.ReceivedByteCount) return false;
            if (this.SentByteCount != target.SentByteCount) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Id != default(byte[])) h ^= this.Id.GetHashCode();
                if (this.Type != default(SecureConnectionType)) h ^= this.Type.GetHashCode();
                if (this.Uri != default(string)) h ^= this.Uri.GetHashCode();
                if (this.Location != default(Location)) h ^= this.Location.GetHashCode();
                if (this.Priority != default(long)) h ^= this.Priority.GetHashCode();
                if (this.ReceivedByteCount != default(long)) h ^= this.ReceivedByteCount.GetHashCode();
                if (this.SentByteCount != default(long)) h ^= this.SentByteCount.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class RelayExchangeConnectionReport : ItemBase<RelayExchangeConnectionReport>
    {
        [JsonConstructor]
        public RelayExchangeConnectionReport(byte[] id, SecureConnectionType type, string uri, Location location, long priority, long receivedByteCount, long sentByteCount)
        {
            this.Id = id;
            this.Type = type;
            this.Uri = uri;
            this.Location = location;
            this.Priority = priority;
            this.ReceivedByteCount = receivedByteCount;
            this.SentByteCount = sentByteCount;
        }
        [JsonProperty]
        public byte[] Id { get; }
        [JsonProperty]
        public SecureConnectionType Type { get; }
        [JsonProperty]
        public string Uri { get; }
        [JsonProperty]
        public Location Location { get; }
        [JsonProperty]
        public long Priority { get; }
        [JsonProperty]
        public long ReceivedByteCount { get; }
        [JsonProperty]
        public long SentByteCount { get; }
        public override bool Equals(RelayExchangeConnectionReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Id != target.Id) return false;
            if (this.Type != target.Type) return false;
            if (this.Uri != target.Uri) return false;
            if (this.Location != target.Location) return false;
            if (this.Priority != target.Priority) return false;
            if (this.ReceivedByteCount != target.ReceivedByteCount) return false;
            if (this.SentByteCount != target.SentByteCount) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Id != default(byte[])) h ^= this.Id.GetHashCode();
                if (this.Type != default(SecureConnectionType)) h ^= this.Type.GetHashCode();
                if (this.Uri != default(string)) h ^= this.Uri.GetHashCode();
                if (this.Location != default(Location)) h ^= this.Location.GetHashCode();
                if (this.Priority != default(long)) h ^= this.Priority.GetHashCode();
                if (this.ReceivedByteCount != default(long)) h ^= this.ReceivedByteCount.GetHashCode();
                if (this.SentByteCount != default(long)) h ^= this.SentByteCount.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CacheContentReport : ItemBase<CacheContentReport>
    {
        [JsonConstructor]
        public CacheContentReport(DateTime creationTime, long length, Metadata metadata, string path)
        {
            this.CreationTime = creationTime;
            this.Length = length;
            this.Metadata = metadata;
            this.Path = path;
        }
        private DateTime _creationTime;
        [JsonProperty]
        public DateTime CreationTime
        {
            get => _creationTime;
            private set => _creationTime = value.Normalize();
        }
        [JsonProperty]
        public long Length { get; }
        [JsonProperty]
        public Metadata Metadata { get; }
        [JsonProperty]
        public string Path { get; }
        public override bool Equals(CacheContentReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Length != target.Length) return false;
            if (this.Metadata != target.Metadata) return false;
            if (this.Path != target.Path) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                if (this.Length != default(long)) h ^= this.Length.GetHashCode();
                if (this.Metadata != default(Metadata)) h ^= this.Metadata.GetHashCode();
                if (this.Path != default(string)) h ^= this.Path.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class DownloadContentReport : ItemBase<DownloadContentReport>
    {
        [JsonConstructor]
        public DownloadContentReport(Metadata metadata, string path, DownloadState state, int depth, int blockCount, int downloadBlockCount, int parityBlockCount)
        {
            this.Metadata = metadata;
            this.Path = path;
            this.State = state;
            this.Depth = depth;
            this.BlockCount = blockCount;
            this.DownloadBlockCount = downloadBlockCount;
            this.ParityBlockCount = parityBlockCount;
        }
        [JsonProperty]
        public Metadata Metadata { get; }
        [JsonProperty]
        public string Path { get; }
        [JsonProperty]
        public DownloadState State { get; }
        [JsonProperty]
        public int Depth { get; }
        [JsonProperty]
        public int BlockCount { get; }
        [JsonProperty]
        public int DownloadBlockCount { get; }
        [JsonProperty]
        public int ParityBlockCount { get; }
        public override bool Equals(DownloadContentReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Metadata != target.Metadata) return false;
            if (this.Path != target.Path) return false;
            if (this.State != target.State) return false;
            if (this.Depth != target.Depth) return false;
            if (this.BlockCount != target.BlockCount) return false;
            if (this.DownloadBlockCount != target.DownloadBlockCount) return false;
            if (this.ParityBlockCount != target.ParityBlockCount) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Metadata != default(Metadata)) h ^= this.Metadata.GetHashCode();
                if (this.Path != default(string)) h ^= this.Path.GetHashCode();
                if (this.State != default(DownloadState)) h ^= this.State.GetHashCode();
                if (this.Depth != default(int)) h ^= this.Depth.GetHashCode();
                if (this.BlockCount != default(int)) h ^= this.BlockCount.GetHashCode();
                if (this.DownloadBlockCount != default(int)) h ^= this.DownloadBlockCount.GetHashCode();
                if (this.ParityBlockCount != default(int)) h ^= this.ParityBlockCount.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class ServiceConfig : ItemBase<ServiceConfig>
    {
        [JsonConstructor]
        public ServiceConfig(CoreConfig core, ConnectionConfig connection, MessageConfig message)
        {
            this.Core = core;
            this.Connection = connection;
            this.Message = message;
        }
        [JsonProperty]
        public CoreConfig Core { get; }
        [JsonProperty]
        public ConnectionConfig Connection { get; }
        [JsonProperty]
        public MessageConfig Message { get; }
        public override bool Equals(ServiceConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Core != target.Core) return false;
            if (this.Connection != target.Connection) return false;
            if (this.Message != target.Message) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Core != default(CoreConfig)) h ^= this.Core.GetHashCode();
                if (this.Connection != default(ConnectionConfig)) h ^= this.Connection.GetHashCode();
                if (this.Message != default(MessageConfig)) h ^= this.Message.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CoreConfig : ItemBase<CoreConfig>
    {
        [JsonConstructor]
        public CoreConfig(NetworkConfig network, DownloadConfig download)
        {
            this.Network = network;
            this.Download = download;
        }
        [JsonProperty]
        public NetworkConfig Network { get; }
        [JsonProperty]
        public DownloadConfig Download { get; }
        public override bool Equals(CoreConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Network != target.Network) return false;
            if (this.Download != target.Download) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Network != default(NetworkConfig)) h ^= this.Network.GetHashCode();
                if (this.Download != default(DownloadConfig)) h ^= this.Download.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class NetworkConfig : ItemBase<NetworkConfig>
    {
        [JsonConstructor]
        public NetworkConfig(DirectExchangeConfig directExchange, RelayExchangeConfig indirectExchange)
        {
            this.DirectExchange = directExchange;
            this.RelayExchange = indirectExchange;
        }
        [JsonProperty]
        public DirectExchangeConfig DirectExchange { get; }
        [JsonProperty]
        public RelayExchangeConfig RelayExchange { get; }
        public override bool Equals(NetworkConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.DirectExchange != target.DirectExchange) return false;
            if (this.RelayExchange != target.RelayExchange) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.DirectExchange != default(DirectExchangeConfig)) h ^= this.DirectExchange.GetHashCode();
                if (this.RelayExchange != default(RelayExchangeConfig)) h ^= this.RelayExchange.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class DirectExchangeConfig : ItemBase<DirectExchangeConfig>
    {
        [JsonConstructor]
        public DirectExchangeConfig(bool isEnabled, Location myLocation, int connectionCountUpperLimit, int bandwidthUpperLimit)
        {
            this.IsEnabled = isEnabled;
            this.MyLocation = myLocation;
            this.ConnectionCountUpperLimit = connectionCountUpperLimit;
            this.BandwidthUpperLimit = bandwidthUpperLimit;
        }
        [JsonProperty]
        public bool IsEnabled { get; }
        [JsonProperty]
        public Location MyLocation { get; }
        [JsonProperty]
        public int ConnectionCountUpperLimit { get; }
        [JsonProperty]
        public int BandwidthUpperLimit { get; }
        public override bool Equals(DirectExchangeConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.IsEnabled != target.IsEnabled) return false;
            if (this.MyLocation != target.MyLocation) return false;
            if (this.ConnectionCountUpperLimit != target.ConnectionCountUpperLimit) return false;
            if (this.BandwidthUpperLimit != target.BandwidthUpperLimit) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.IsEnabled != default(bool)) h ^= this.IsEnabled.GetHashCode();
                if (this.MyLocation != default(Location)) h ^= this.MyLocation.GetHashCode();
                if (this.ConnectionCountUpperLimit != default(int)) h ^= this.ConnectionCountUpperLimit.GetHashCode();
                if (this.BandwidthUpperLimit != default(int)) h ^= this.BandwidthUpperLimit.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class RelayExchangeConfig : ItemBase<RelayExchangeConfig>
    {
        [JsonConstructor]
        public RelayExchangeConfig(Location myLocation, int connectionCountUpperLimit, int bandwidthUpperLimit)
        {
            this.MyLocation = myLocation;
            this.ConnectionCountUpperLimit = connectionCountUpperLimit;
            this.BandwidthUpperLimit = bandwidthUpperLimit;
        }
        [JsonProperty]
        public Location MyLocation { get; }
        [JsonProperty]
        public int ConnectionCountUpperLimit { get; }
        [JsonProperty]
        public int BandwidthUpperLimit { get; }
        public override bool Equals(RelayExchangeConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.MyLocation != target.MyLocation) return false;
            if (this.ConnectionCountUpperLimit != target.ConnectionCountUpperLimit) return false;
            if (this.BandwidthUpperLimit != target.BandwidthUpperLimit) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.MyLocation != default(Location)) h ^= this.MyLocation.GetHashCode();
                if (this.ConnectionCountUpperLimit != default(int)) h ^= this.ConnectionCountUpperLimit.GetHashCode();
                if (this.BandwidthUpperLimit != default(int)) h ^= this.BandwidthUpperLimit.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CacheConfig : ItemBase<CacheConfig>
    {
        [JsonConstructor]
        public CacheConfig(int uploadedMessageProtectionAreaUpperLimitPercentage)
        {
            this.UploadedMessageProtectionAreaUpperLimitPercentage = uploadedMessageProtectionAreaUpperLimitPercentage;
        }
        [JsonProperty]
        public int UploadedMessageProtectionAreaUpperLimitPercentage { get; }
        public override bool Equals(CacheConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.UploadedMessageProtectionAreaUpperLimitPercentage != target.UploadedMessageProtectionAreaUpperLimitPercentage) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.UploadedMessageProtectionAreaUpperLimitPercentage != default(int)) h ^= this.UploadedMessageProtectionAreaUpperLimitPercentage.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class DownloadConfig : ItemBase<DownloadConfig>
    {
        [JsonConstructor]
        public DownloadConfig(string basePath, int downloadedContentProtectionAreaUpperlimitPercentage)
        {
            this.BasePath = basePath;
            this.DownloadedContentProtectionAreaUpperlimitPercentage = downloadedContentProtectionAreaUpperlimitPercentage;
        }
        [JsonProperty]
        public string BasePath { get; }
        [JsonProperty]
        public int DownloadedContentProtectionAreaUpperlimitPercentage { get; }
        public override bool Equals(DownloadConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.BasePath != target.BasePath) return false;
            if (this.DownloadedContentProtectionAreaUpperlimitPercentage != target.DownloadedContentProtectionAreaUpperlimitPercentage) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.BasePath != default(string)) h ^= this.BasePath.GetHashCode();
                if (this.DownloadedContentProtectionAreaUpperlimitPercentage != default(int)) h ^= this.DownloadedContentProtectionAreaUpperlimitPercentage.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class ConnectionConfig : ItemBase<ConnectionConfig>
    {
        [JsonConstructor]
        public ConnectionConfig(TcpConnectionConfig tcp, I2pConnectionConfig i2p, CustomConnectionConfig custom, CatharsisConfig catharsis)
        {
            this.Tcp = tcp;
            this.I2p = i2p;
            this.Custom = custom;
            this.Catharsis = catharsis;
        }
        [JsonProperty]
        public TcpConnectionConfig Tcp { get; }
        [JsonProperty]
        public I2pConnectionConfig I2p { get; }
        [JsonProperty]
        public CustomConnectionConfig Custom { get; }
        [JsonProperty]
        public CatharsisConfig Catharsis { get; }
        public override bool Equals(ConnectionConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Tcp != target.Tcp) return false;
            if (this.I2p != target.I2p) return false;
            if (this.Custom != target.Custom) return false;
            if (this.Catharsis != target.Catharsis) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Tcp != default(TcpConnectionConfig)) h ^= this.Tcp.GetHashCode();
                if (this.I2p != default(I2pConnectionConfig)) h ^= this.I2p.GetHashCode();
                if (this.Custom != default(CustomConnectionConfig)) h ^= this.Custom.GetHashCode();
                if (this.Catharsis != default(CatharsisConfig)) h ^= this.Catharsis.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CatharsisConfig : ItemBase<CatharsisConfig>
    {
        [JsonConstructor]
        public CatharsisConfig(CatharsisIpv4Config ipv4Config)
        {
            this.Ipv4Config = ipv4Config;
        }
        [JsonProperty]
        public CatharsisIpv4Config Ipv4Config { get; }
        public override bool Equals(CatharsisConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Ipv4Config != target.Ipv4Config) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Ipv4Config != default(CatharsisIpv4Config)) h ^= this.Ipv4Config.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CatharsisIpv4Config : ItemBase<CatharsisIpv4Config>
    {
        [JsonConstructor]
        public CatharsisIpv4Config(IEnumerable<string> urls, IEnumerable<string> paths)
        {
            this.Urls = new ReadOnlyCollection<string>(urls.ToArray());
            this.Paths = new ReadOnlyCollection<string>(paths.ToArray());
        }
        [JsonProperty]
        public IReadOnlyList<string> Urls { get; }
        [JsonProperty]
        public IReadOnlyList<string> Paths { get; }
        public override bool Equals(CatharsisIpv4Config target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (!CollectionUtils.Equals(this.Urls, target.Urls)) return false;
            if (!CollectionUtils.Equals(this.Paths, target.Paths)) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                for (int i = 0; i < Urls.Count; i++)
                {
                    h ^= this.Urls[i].GetHashCode();
                }
                for (int i = 0; i < Paths.Count; i++)
                {
                    h ^= this.Paths[i].GetHashCode();
                }
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class TcpConnectionConfig : ItemBase<TcpConnectionConfig>
    {
        [JsonConstructor]
        public TcpConnectionConfig(TcpConnectionType type, ushort ipv4Port, ushort ipv6Port, string proxyUri)
        {
            this.Type = type;
            this.Ipv4Port = ipv4Port;
            this.Ipv6Port = ipv6Port;
            this.ProxyUri = proxyUri;
        }
        [JsonProperty]
        public TcpConnectionType Type { get; }
        [JsonProperty]
        public ushort Ipv4Port { get; }
        [JsonProperty]
        public ushort Ipv6Port { get; }
        [JsonProperty]
        public string ProxyUri { get; }
        public override bool Equals(TcpConnectionConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Ipv4Port != target.Ipv4Port) return false;
            if (this.Ipv6Port != target.Ipv6Port) return false;
            if (this.ProxyUri != target.ProxyUri) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Type != default(TcpConnectionType)) h ^= this.Type.GetHashCode();
                if (this.Ipv4Port != default(ushort)) h ^= this.Ipv4Port.GetHashCode();
                if (this.Ipv6Port != default(ushort)) h ^= this.Ipv6Port.GetHashCode();
                if (this.ProxyUri != default(string)) h ^= this.ProxyUri.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class I2pConnectionConfig : ItemBase<I2pConnectionConfig>
    {
        [JsonConstructor]
        public I2pConnectionConfig(bool isEnabled, string samBridgeUri)
        {
            this.IsEnabled = isEnabled;
            this.SamBridgeUri = samBridgeUri;
        }
        [JsonProperty]
        public bool IsEnabled { get; }
        [JsonProperty]
        public string SamBridgeUri { get; }
        public override bool Equals(I2pConnectionConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.IsEnabled != target.IsEnabled) return false;
            if (this.SamBridgeUri != target.SamBridgeUri) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.IsEnabled != default(bool)) h ^= this.IsEnabled.GetHashCode();
                if (this.SamBridgeUri != default(string)) h ^= this.SamBridgeUri.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CustomConnectionConfig : ItemBase<CustomConnectionConfig>
    {
        [JsonConstructor]
        public CustomConnectionConfig(IEnumerable<string> locationUris, IEnumerable<ConnectionFilter> connectionFilters, IEnumerable<string> listenUris)
        {
            this.LocationUris = new ReadOnlyCollection<string>(locationUris.ToArray());
            this.ConnectionFilters = new ReadOnlyCollection<ConnectionFilter>(connectionFilters.ToArray());
            this.ListenUris = new ReadOnlyCollection<string>(listenUris.ToArray());
        }
        [JsonProperty]
        public IReadOnlyList<string> LocationUris { get; }
        [JsonProperty]
        public IReadOnlyList<ConnectionFilter> ConnectionFilters { get; }
        [JsonProperty]
        public IReadOnlyList<string> ListenUris { get; }
        public override bool Equals(CustomConnectionConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (!CollectionUtils.Equals(this.LocationUris, target.LocationUris)) return false;
            if (!CollectionUtils.Equals(this.ConnectionFilters, target.ConnectionFilters)) return false;
            if (!CollectionUtils.Equals(this.ListenUris, target.ListenUris)) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                for (int i = 0; i < LocationUris.Count; i++)
                {
                    h ^= this.LocationUris[i].GetHashCode();
                }
                for (int i = 0; i < ConnectionFilters.Count; i++)
                {
                    h ^= this.ConnectionFilters[i].GetHashCode();
                }
                for (int i = 0; i < ListenUris.Count; i++)
                {
                    h ^= this.ListenUris[i].GetHashCode();
                }
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class MessageConfig : ItemBase<MessageConfig>
    {
        [JsonConstructor]
        public MessageConfig(IEnumerable<Signature> searchSignatures)
        {
            this.SearchSignatures = new ReadOnlyCollection<Signature>(searchSignatures.ToArray());
        }
        [JsonProperty]
        public IReadOnlyList<Signature> SearchSignatures { get; }
        public override bool Equals(MessageConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (!CollectionUtils.Equals(this.SearchSignatures, target.SearchSignatures)) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                for (int i = 0; i < SearchSignatures.Count; i++)
                {
                    h ^= this.SearchSignatures[i].GetHashCode();
                }
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }

}

