using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface IPublishMessageStorageFactory
    {
        ValueTask<IPublishMessageStorage> Create(string configPath, IBufferPool<byte> bufferPool);
    }

    public interface IPublishMessageStorage : IReadOnlyStorage
    {
        public static IPublishMessageStorageFactory Factory { get; }

        ValueTask<OmniHash> AddPublishMessage(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default);
        void RemovePublishMessage(OmniHash hash);
        IEnumerable<PublishMessageReport> GetPublishMessageReportes();
        ValueTask<bool> TryExportPublishMessage(OmniHash hash, IBufferWriter<byte> bufferWriter);

        void AddWantMessage(OmniHash hash);
        void RemoveWantMessage(OmniHash hash);
        IEnumerable<WantMessageReport> GetWantMessageReportes();
    }
}
