using System;
using System.Collections.Generic;
using System.Text;
using Amoeba.Messages;

namespace Amoeba.Service
{
    sealed partial class NetworkManager
    {
        internal enum ExchangeProtocolVersion : uint
        {
            Version0 = 0,
        }

        internal enum ExchangeMessageId
        {
            LocationsRequest = 0,
            LocationsResult = 1,

            MetadatasLookupRequest = 2,
            MetadatasLookupResult = 3,

            BlocksRequest = 4,
            BlockResult = 5,
        }

        internal sealed class ExchangeSessionInfo
        {
            public string Uri { get; set; }
            public int ThreadId { get; set; }

            public ExchangeProtocolVersion Version { get; set; }
            public byte[] Id { get; set; }
            public Location Location { get; set; }

            public PriorityManager Priority { get; private set; } = new PriorityManager(new TimeSpan(0, 10, 0));

            public SendInfo Send { get; private set; } = new SendInfo();
            public ReceiveInfo Receive { get; private set; } = new ReceiveInfo();

            public DateTime CreationTime { get; private set; } = DateTime.UtcNow;

            public void Update()
            {
                this.Priority.Update();
                this.Send.Update();
                this.Receive.Update();
            }

            public sealed class SendInfo
            {
                public bool IsSentVersion { get; set; }
                public bool IsSentProfile { get; set; }

                public VolatileBloomFilter<Hash> PushedBlockRequestFilter { get; private set; } = new VolatileBloomFilter<Hash>(_maxBlockRequestCount * 2 * 3, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 3, 0), new TimeSpan(0, 30, 0));
                public VolatileBloomFilter<Hash> PushedBlockLinkFilter { get; private set; } = new VolatileBloomFilter<Hash>(_maxBlockLinkCount * 2 * 30, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 30, 0), new TimeSpan(3, 0, 0));

                public Stopwatch LocationResultStopwatch { get; private set; } = Stopwatch.StartNew();
                public Stopwatch BlockResultStopwatch { get; private set; } = Stopwatch.StartNew();
                public Stopwatch BroadcastMetadataResultStopwatch { get; private set; } = Stopwatch.StartNew();
                public Stopwatch UnicastMetadataResultStopwatch { get; private set; } = Stopwatch.StartNew();
                public Stopwatch MulticastMetadataResultStopwatch { get; private set; } = Stopwatch.StartNew();

                public LockedQueue<Hash> PushBlockResultQueue { get; private set; } = new LockedQueue<Hash>();
                public LockedQueue<Hash> PushBlockLinkQueue { get; private set; } = new LockedQueue<Hash>();
                public LockedQueue<Hash> PushBlockRequestQueue { get; private set; } = new LockedQueue<Hash>();
                public LockedQueue<Signature> PushBroadcastMetadataRequestQueue { get; private set; } = new LockedQueue<Signature>();
                public LockedQueue<Signature> PushUnicastMetadataRequestQueue { get; private set; } = new LockedQueue<Signature>();
                public LockedQueue<Tag> PushMulticastMetadataRequestQueue { get; private set; } = new LockedQueue<Tag>();

                public void Update()
                {
                    this.PushedBlockRequestFilter.Update();
                    this.PushedBlockLinkFilter.Update();
                }
            }

            public sealed class ReceiveInfo
            {
                public bool IsReceivedVersion { get; set; }
                public bool IsReceivedProfile { get; set; }

                public Stopwatch Stopwatch { get; private set; } = new Stopwatch();

                public VolatileBloomFilter<Hash> PulledBlockLinkFilter { get; private set; } = new VolatileBloomFilter<Hash>(_maxBlockLinkCount * 2 * 30, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 30, 0), new TimeSpan(3, 0, 0));

                public VolatileHashSet<Location> PulledLocationSet { get; private set; } = new VolatileHashSet<Location>(new TimeSpan(0, 10, 0));
                public VolatileHashSet<Hash> PulledBlockLinkSet { get; private set; } = new VolatileHashSet<Hash>(new TimeSpan(0, 30, 0));
                public VolatileHashSet<Hash> PulledBlockRequestSet { get; private set; } = new VolatileHashSet<Hash>(new TimeSpan(0, 30, 0));
                public VolatileHashSet<Signature> PulledBroadcastMetadataRequestSet { get; private set; } = new VolatileHashSet<Signature>(new TimeSpan(0, 3, 0));
                public VolatileHashSet<Signature> PulledUnicastMetadataRequestSet { get; private set; } = new VolatileHashSet<Signature>(new TimeSpan(0, 3, 0));
                public VolatileHashSet<Tag> PulledMulticastMetadataRequestSet { get; private set; } = new VolatileHashSet<Tag>(new TimeSpan(0, 3, 0));

                public void Update()
                {
                    this.PulledBlockLinkFilter.Update();

                    this.PulledLocationSet.Update();
                    this.PulledBlockLinkSet.Update();
                    this.PulledBlockRequestSet.Update();
                    this.PulledBroadcastMetadataRequestSet.Update();
                    this.PulledUnicastMetadataRequestSet.Update();
                    this.PulledMulticastMetadataRequestSet.Update();
                }
            }
        }
    }
}
