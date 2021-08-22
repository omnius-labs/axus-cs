using System.Collections.Generic;
using Omnius.Core;

namespace Omnius.Xeus.Engines
{
    public record ShoutExchangerOptions
    {
        public ShoutExchangerOptions(uint maxSessionCount)
        {
            this.MaxSessionCount = maxSessionCount;
        }

        public uint MaxSessionCount { get; }
    }
}
