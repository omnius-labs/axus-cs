using System;
using System.Collections.Generic;
using System.Diagnostics;
using Omnix.Algorithms.Cryptography;
using Omnix.Base;
using Omnix.DataStructures;
using Omnix.Network;
using Xeus.Messages;

namespace Xeus.Core.Internal.Exchange.Primitives
{
    internal enum SessionType
    {
        Conneced,
        Accepted,
    }

    internal sealed class SessionStatus
    {
        public SessionStatus()
        {
            var random = RandomProvider.GetThreadRandom();

            this.DecrementAtHopLimitMaximum = (random.Next(0, int.MaxValue) % 2 == 0);
            this.DecrementAtHopLimitMinimum = (random.Next(0, int.MaxValue) % 2 == 0);
        }

        public ProtocolVersion Version { get; set; }
        public SessionType Type { get; set; }
        public int ThreadId { get; set; }

        public byte[] Id { get; set; }
        public OmniAddress NodeAddress { get; set; }

        public PriorityManager Priority { get; } = new PriorityManager(new TimeSpan(0, 10, 0));

        public SendInfo Send { get; } = new SendInfo();
        public ReceiveInfo Receive { get; } = new ReceiveInfo();

        public bool DecrementAtHopLimitMaximum { get; }
        public bool DecrementAtHopLimitMinimum { get; }

        public DateTime CreationTime { get; } = DateTime.UtcNow;

        public const int MaxLocationCount = 256;
        public const int MaxWantBroadcastClueCount = 256;
        public const int MaxWantUnicastClueCount = 256;
        public const int MaxWantMulticastClueCount = 256;
        public const int MaxBroadcastClueCount = 256;
        public const int MaxUnicastClueCount = 256;
        public const int MaxMulticastClueCount = 256;
        public const int MaxPublishBlockCount = 256;
        public const int MaxWantBlockCount = 256;

        public void Refresh()
        {
            this.Priority.Refresh();
            this.Send.Refresh();
            this.Receive.Refresh();
        }

        public sealed class SendInfo
        {
            internal SendInfo() { }

            public bool IsInitialized { get; set; }

            public StopwatchsInfo Stopwatchs { get; } = new StopwatchsInfo();
            public FiltersInfo Filters { get; } = new FiltersInfo();
            public QueuesInfo Queues { get; } = new QueuesInfo();

            internal void Refresh()
            {
                this.Filters.Refresh();
            }

            public sealed class StopwatchsInfo
            {
                internal StopwatchsInfo()
                {
                    this.NodeAddressesStopwatch.Start();

                    this.WantBroadcastClueStopwatch.Start();
                    this.WantUnicastClueStopwatch.Start();
                    this.WantMulticastClueStopwatch.Start();

                    this.BroadcastClueStopwatch.Start();
                    this.UnicastClueStopwatch.Start();
                    this.MulticastClueStopwatch.Start();

                    this.WantBlocksStopwatch.Start();
                    this.PublishBlocksStopwatch.Start();
                    this.BlocksStopwatch.Start();
                }

                public Stopwatch NodeAddressesStopwatch { get; } = new Stopwatch();

                public Stopwatch WantBroadcastClueStopwatch { get; } = new Stopwatch();
                public Stopwatch WantUnicastClueStopwatch { get; } = new Stopwatch();
                public Stopwatch WantMulticastClueStopwatch { get; } = new Stopwatch();

                public Stopwatch BroadcastClueStopwatch { get; } = new Stopwatch();
                public Stopwatch UnicastClueStopwatch { get; } = new Stopwatch();
                public Stopwatch MulticastClueStopwatch { get; } = new Stopwatch();

                public Stopwatch WantBlocksStopwatch { get; } = new Stopwatch();
                public Stopwatch PublishBlocksStopwatch { get; } = new Stopwatch();
                public Stopwatch BlocksStopwatch { get; } = new Stopwatch();
            }

            public sealed class FiltersInfo
            {
                public VolatileBloomFilter<BroadcastClue> WantBroadcastCluesFilter { get; } = new VolatileBloomFilter<BroadcastClue>(MaxBroadcastClueCount * 2 * 10, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 10, 0), new TimeSpan(1, 0, 0));
                public VolatileBloomFilter<UnicastClue> WantUnicastCluesFilter { get; } = new VolatileBloomFilter<UnicastClue>(MaxUnicastClueCount * 2 * 10, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 10, 0), new TimeSpan(1, 0, 0));
                public VolatileBloomFilter<MulticastClue> WantMulticastCluesFilter { get; } = new VolatileBloomFilter<MulticastClue>(MaxMulticastClueCount * 2 * 10, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 10, 0), new TimeSpan(1, 0, 0));

                public VolatileBloomFilter<OmniHash> WantBlocksFilter { get; } = new VolatileBloomFilter<OmniHash>(MaxWantBlockCount * 2 * 10, 0.0001, 3, (n) => n.GetHashCode(), new TimeSpan(0, 10, 0), new TimeSpan(1, 0, 0));

                internal void Refresh()
                {
                    this.WantBroadcastCluesFilter.Refresh();
                    this.WantUnicastCluesFilter.Refresh();
                    this.WantMulticastCluesFilter.Refresh();

                    this.WantBlocksFilter.Refresh();
                }
            }

            public sealed class QueuesInfo
            {
                internal QueuesInfo() { }

                public LockedHashDictionary<OmniSignature, RelayOption> WantBroadcastClueMap { get; } = new LockedHashDictionary<OmniSignature, RelayOption>();
                public LockedHashDictionary<OmniSignature, RelayOption> WantUnicastClueMap { get; } = new LockedHashDictionary<OmniSignature, RelayOption>();
                public LockedHashDictionary<OmniSignature, RelayOption> WantMulticastClueMap { get; } = new LockedHashDictionary<OmniSignature, RelayOption>();

                public LockedHashDictionary<OmniHash, RelayOption> WantBlocksMap { get; } = new LockedHashDictionary<OmniHash, RelayOption>();
                public LockedHashDictionary<OmniHash, RelayOption> PublishBlocksMap { get; } = new LockedHashDictionary<OmniHash, RelayOption>();
            }
        }

        public sealed class ReceiveInfo
        {
            internal ReceiveInfo() { }

            public bool IsInitialized { get; set; }

            public StopwatchesInfo Stopwatches { get; } = new StopwatchesInfo();
            public QueuesInfo Queues { get; } = new QueuesInfo();
            public VolatileHashSet<OmniAddress> NodeAddressSet { get; } = new VolatileHashSet<OmniAddress>(new TimeSpan(0, 30, 0));

            internal void Refresh()
            {
                this.Queues.Refresh();
                this.NodeAddressSet.Refresh();
            }

            public sealed class StopwatchesInfo
            {
                internal StopwatchesInfo()
                {
                    this.ReceiveBlockStopwatch.Start();
                }

                public Stopwatch ReceiveBlockStopwatch { get; } = new Stopwatch();
            }

            public sealed class QueuesInfo
            {
                internal QueuesInfo() { }

                public VolatileHashDictionary<OmniSignature, RelayOption> WantBroadcastClueMap { get; } = new VolatileHashDictionary<OmniSignature, RelayOption>(new TimeSpan(0, 5, 0));
                public VolatileHashDictionary<OmniSignature, RelayOption> WantUnicastClueMap { get; } = new VolatileHashDictionary<OmniSignature, RelayOption>(new TimeSpan(0, 5, 0));
                public VolatileHashDictionary<OmniSignature, RelayOption> WantMulticastClueMap { get; } =new VolatileHashDictionary<OmniSignature, RelayOption>(new TimeSpan(0, 5, 0));

                public VolatileHashDictionary<OmniHash, RelayOption> WantBlocksMap { get; } = new VolatileHashDictionary<OmniHash, RelayOption>(new TimeSpan(0, 30, 0));

                internal void Refresh()
                {
                    this.WantBroadcastClueMap.Refresh();
                    this.WantUnicastClueMap.Refresh();
                    this.WantMulticastClueMap.Refresh();

                    this.WantBlocksMap.Refresh();
                }
            }
        }
    }
}
