using System;
using System.Diagnostics;
using Omnix.Base;
using Omnix.Collections;
using Omnix.Cryptography;
using Omnix.Network;
using Xeus.Messages;
using Xeus.Messages.Reports;

namespace Xeus.Core.Exchange.Internal
{
    internal sealed class SessionInfo
    {
        public SessionInfo()
        {
            var random = RandomProvider.GetThreadRandom();

            this.DecrementAtMaximum = (random.Next(0, int.MaxValue) % 2 == 0);
            this.DecrementAtMinimum = (random.Next(0, int.MaxValue) % 2 == 0);
        }

        public ProtocolVersion Version { get; set; }
        public SessionType Type { get; set; }
        public int ThreadId { get; set; }

        public byte[] Id { get; set; }
        public OmniAddress Address { get; set; }

        public PriorityManager Priority { get; } = new PriorityManager(new TimeSpan(0, 10, 0));

        public SendInfo Send { get; } = new SendInfo();
        public ReceiveInfo Receive { get; } = new ReceiveInfo();

        public bool DecrementAtMaximum { get; }
        public bool DecrementAtMinimum { get; }

        public DateTime CreationTime { get; } = DateTime.UtcNow;

        public const int MaxLocationCount = 256;
        public const int MaxMetadataRequestCount = 256;
        public const int MaxMetadataResultCount = 256;
        public const int MaxBlockLinkCount = 256;
        public const int MaxBlockRequestCount = 256;

        public void Update()
        {
            this.Priority.Update();
            this.Send.Update();
            this.Receive.Update();
        }

        public sealed class SendInfo
        {
            internal SendInfo() { }

            public bool IsInitialized { get; set; }

            public StopwatchInfo Stopwatch { get; } = new StopwatchInfo();
            public AnchorInfo Anchor { get; } = new AnchorInfo();
            public QueueInfo Queue { get; } = new QueueInfo();

            internal void Update()
            {
                this.Anchor.Update();
            }

            public sealed class StopwatchInfo
            {
                internal StopwatchInfo()
                {
                    this.AddressesResultStopwatch.Start();
                    this.BlockResultStopwatch.Start();
                    this.BroadcastMetadataResultStopwatch.Start();
                    this.UnicastMetadataResultStopwatch.Start();
                    this.MulticastMetadataResultStopwatch.Start();
                }

                public Stopwatch AddressesResultStopwatch { get; } = new Stopwatch();
                public Stopwatch BlockResultStopwatch { get; } = new Stopwatch();
                public Stopwatch BroadcastMetadataResultStopwatch { get; } = new Stopwatch();
                public Stopwatch UnicastMetadataResultStopwatch { get; } = new Stopwatch();
                public Stopwatch MulticastMetadataResultStopwatch { get; } = new Stopwatch();
            }

            public sealed class AnchorInfo
            {
                public VolatileBloomFilter<OmniHash> BlockRequestFilter { get; } = new VolatileBloomFilter<OmniHash>(MaxBlockRequestCount * 2 * 3, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 3, 0), new TimeSpan(0, 30, 0));
                public VolatileBloomFilter<OmniHash> BlockLinkFilter { get; } = new VolatileBloomFilter<OmniHash>(MaxBlockLinkCount * 2 * 30, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 10, 0), new TimeSpan(1, 0, 0));

                internal void Update()
                {
                    this.BlockRequestFilter.Update();
                    this.BlockLinkFilter.Update();
                }
            }

            public sealed class QueueInfo
            {
                internal QueueInfo() { }

                public LockedQueue<BroadcastMetadataRequestPacket> BroadcastMetadataRequestQueue { get; } = new LockedQueue<BroadcastMetadataRequestPacket>();
                public LockedQueue<UnicastMetadataRequestPacket> UnicastMetadataRequestQueue { get; } = new LockedQueue<UnicastMetadataRequestPacket>();
                public LockedQueue<MulticastMetadataRequestPacket> MulticastMetadataRequestQueue { get; } = new LockedQueue<MulticastMetadataRequestPacket>();

                public LockedQueue<BlockLinkPacket> BlockLinkQueue { get; } = new LockedQueue<BlockLinkPacket>();
                public LockedQueue<BlockRequestPacket> BlockRequestQueue { get; } = new LockedQueue<BlockRequestPacket>();
                public LockedQueue<BlockResultPacket> BlockResultQueue { get; } = new LockedQueue<BlockResultPacket>();
            }
        }

        public sealed class ReceiveInfo
        {
            internal ReceiveInfo() { }

            public bool IsInitialized { get; set; }

            public StopwatchInfo Stopwatch { get; } = new StopwatchInfo();
            public MapInfo Queue { get; } = new MapInfo();

            internal void Update()
            {
                this.Queue.Update();
            }

            public sealed class StopwatchInfo
            {
                internal StopwatchInfo()
                {
                    this.BlockResultStopwatch.Start();
                }

                public Stopwatch BlockResultStopwatch { get; } = new Stopwatch();
            }
            public sealed class AnchorInfo
            {
                public VolatileBloomFilter<OmniHash> BlockRequestFilter { get; } = new VolatileBloomFilter<OmniHash>(MaxBlockRequestCount * 2 * 3, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 3, 0), new TimeSpan(0, 30, 0));
                public VolatileBloomFilter<OmniHash> BlockLinkFilter { get; } = new VolatileBloomFilter<OmniHash>(MaxBlockLinkCount * 2 * 30, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 30, 0), new TimeSpan(3, 0, 0));

                internal void Update()
                {
                    this.BlockRequestFilter.Update();
                    this.BlockLinkFilter.Update();
                }
            }

            public sealed class MapInfo
            {
                internal MapInfo() { }

                public VolatileHashDictionary<BroadcastMetadata, BroadcastMetadataRequestPacket> BroadcastMetadataRequestSet { get; } = new VolatileHashDictionary<BroadcastMetadata, BroadcastMetadataRequestPacket>(new TimeSpan(0, 3, 0));
                public VolatileHashDictionary<UnicastMetadata, UnicastMetadataRequestPacket> UnicastMetadataRequestSet { get; } = new VolatileHashDictionary<UnicastMetadata, UnicastMetadataRequestPacket>(new TimeSpan(0, 3, 0));
                public VolatileHashDictionary<MulticastMetadata, MulticastMetadataRequestPacket> MulticastMetadataRequestSet { get; } = new VolatileHashDictionary<MulticastMetadata, MulticastMetadataRequestPacket>(new TimeSpan(0, 3, 0));

                public VolatileHashDictionary<OmniHash, BlockRequestPacket> BlockRequestSet { get; } = new VolatileHashDictionary<OmniHash, BlockRequestPacket>(new TimeSpan(0, 30, 0));
                public VolatileHashDictionary<OmniHash, BlockLinkPacket> BlockLinkSet { get; } = new VolatileHashDictionary<OmniHash, BlockLinkPacket>(new TimeSpan(0, 30, 0));

                internal void Update()
                {
                    this.BroadcastMetadataRequestSet.Update();
                    this.UnicastMetadataRequestSet.Update();
                    this.MulticastMetadataRequestSet.Update();

                    this.BlockRequestSet.Update();
                    this.BlockLinkSet.Update();
                }
            }
        }
    }
}
