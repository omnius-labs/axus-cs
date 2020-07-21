using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Engines
{
    public interface IPublishStorage
    {
        IEnumerable<ResourceTag> GetPublishTags();
    }
}
