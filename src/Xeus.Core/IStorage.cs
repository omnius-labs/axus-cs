using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Network;
using System.Collections;
using Omnix.Cryptography;
using System.Collections.Generic;

namespace Xeus.Core
{
    public interface IStorage : ISettings, IEnumerable<OmniHash>
    {
    }
}
