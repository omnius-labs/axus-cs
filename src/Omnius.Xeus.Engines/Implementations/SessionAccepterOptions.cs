using System.Collections.Generic;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Tasks;

namespace Omnius.Xeus.Engines
{
    public record SessionAccepterOptions
    {
        public SessionAccepterOptions(OmniDigitalSignature digitalSignature)
        {
            this.DigitalSignature = digitalSignature;
        }

        public OmniDigitalSignature DigitalSignature { get; }
    }
}
