using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Ui.Console.Helpers;

namespace Omnius.Xeus.Ui.Desktop.Resources
{
    public class UiSettings
    {
        public IEnumerable<OmniSignature> GetTrustedSignatures()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OmniSignature> GetBlockedSignatures()
        {
            throw new NotImplementedException();
        }

        public int GetDepthSearchProfile()
        {
            throw new NotImplementedException();
        }
    }
}
