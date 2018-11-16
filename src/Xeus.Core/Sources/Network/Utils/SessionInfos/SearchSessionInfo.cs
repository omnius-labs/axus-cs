using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Amoeba.Messages;
using Omnius.Collections;
using Omnius.Security;

namespace Amoeba.Service
{
    sealed partial class NetworkManager
    {
        internal enum SearchProtocolVersion : uint
        {
            Version0 = 0,
        }

        internal enum SearchMessageId
        {
            LocationsPublish = 0,

            LocationsRequest = 1,
            LocationsResult = 2,

            MetadatasRequest = 3,
            MetadatasResult = 4,
        }

        internal sealed class SearchSessionInfo
        {
            public string Uri { get; set; }
            public int ThreadId { get; set; }

            public SearchProtocolVersion Version { get; set; }
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

                public Stopwatch LocationPublishStopwatch { get; private set; } = Stopwatch.StartNew();
                public Stopwatch LocationRequestStopwatch { get; private set; } = Stopwatch.StartNew();
                public Stopwatch LocationResultStopwatch { get; private set; } = Stopwatch.StartNew();
                public Stopwatch MetadataResultStopwatch { get; private set; } = Stopwatch.StartNew();

                public LockedQueue<Signature> PushBroadcastMetadataRequestQueue { get; private set; } = new LockedQueue<Signature>();
                public LockedQueue<Signature> PushUnicastMetadataRequestQueue { get; private set; } = new LockedQueue<Signature>();
                public LockedQueue<Tag> PushMulticastMetadataRequestQueue { get; private set; } = new LockedQueue<Tag>();

                public void Update()
                {

                }
            }

            public sealed class ReceiveInfo
            {
                public bool IsReceivedVersion { get; set; }
                public bool IsReceivedProfile { get; set; }

                public Stopwatch Stopwatch { get; private set; } = new Stopwatch();

                public VolatileHashSet<Location> PulledLocationPublishSet { get; private set; } = new VolatileHashSet<Location>(new TimeSpan(0, 10, 0));
                public VolatileHashSet<Metadata> PulledLocationRequestSet { get; private set; } = new VolatileHashSet<Metadata>(new TimeSpan(0, 10, 0));
                public VolatileHashDictionary<Metadata, Location> PulledLocationResultSet { get; private set; } = new VolatileHashDictionary<Metadata, Location>(new TimeSpan(0, 10, 0));

                public VolatileHashSet<Signature> PulledBroadcastMetadataRequestSet { get; private set; } = new VolatileHashSet<Signature>(new TimeSpan(0, 3, 0));
                public VolatileHashSet<Signature> PulledUnicastMetadataRequestSet { get; private set; } = new VolatileHashSet<Signature>(new TimeSpan(0, 3, 0));
                public VolatileHashSet<Tag> PulledMulticastMetadataRequestSet { get; private set; } = new VolatileHashSet<Tag>(new TimeSpan(0, 3, 0));

                public void Update()
                {
                    this.PulledLocationPublishSet.Update();
                    this.PulledLocationRequestSet.Update();
                    this.PulledLocationResultSet.Update();

                    this.PulledBroadcastMetadataRequestSet.Update();
                    this.PulledUnicastMetadataRequestSet.Update();
                    this.PulledMulticastMetadataRequestSet.Update();
                }
            }
        }
    }
}
