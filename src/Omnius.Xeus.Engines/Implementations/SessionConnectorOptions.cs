using System.Collections.Generic;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Tasks;

namespace Omnius.Xeus.Engines
{
    public record SessionConnectorOptions
    {
        public IReadOnlyCollection<IConnectionConnector>? Connectors { get; init; }

        public OmniDigitalSignature? DigitalSignature { get; init; }

        public IBatchActionDispatcher? BatchActionDispatcher { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }
}
