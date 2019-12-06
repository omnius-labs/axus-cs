using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engine
{
    public interface IMessageStorage : IPrimitiveStorage
    {
        ValueTask<OmniHash> AddPublishMessage(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default);
        void RemovePublishMessage(OmniHash hash);
        IEnumerable<PublishMessageReport> GetPublishMessageReportes();
        ValueTask<bool> TryExportPublishMessage(OmniHash hash, IBufferWriter<byte> bufferWriter);

        void AddWantMessage(OmniHash hash);
        void RemoveWantMessage(OmniHash hash);
        IEnumerable<WantMessageReport> GetWantMessageReportes();
    }
}
