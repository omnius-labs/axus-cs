using System.Collections.Generic;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Tasks;

namespace Omnius.Xeus.Service.Engines
{
    public record SessionConnectorOptions
    {
        public SessionConnectorOptions(OmniDigitalSignature digitalSignature)
        {
            this.DigitalSignature = digitalSignature;
        }

        public OmniDigitalSignature DigitalSignature { get; }
    }
}
