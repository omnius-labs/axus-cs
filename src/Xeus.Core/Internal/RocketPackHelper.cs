using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Omnix.Base;
using Omnix.Io;
using Omnix.Serialization.RocketPack;

namespace Xeus.Core.Internal
{
   public static class RocketPackHelper
    {
        public static Stream MessageToStream<T>(RocketPackMessageBase<T> message)
            where T : RocketPackMessageBase<T>
        {
            Stream stream = null;
            var hub = new Hub();

            try
            {
                message.Export(hub.Writer, BufferPool.Shared);
                hub.Writer.Complete();

                var sequence = hub.Reader.GetSequence();
                var position = sequence.Start;

                stream = new RecyclableMemoryStream(BufferPool.Shared);

                while (sequence.TryGet(ref position, out var memory))
                {
                    stream.Write(memory.Span);
                }

                stream.Seek(0, SeekOrigin.Begin);

                hub.Reader.Complete();
            }
            finally
            {
                hub.Reset();
            }

            return stream;
        }
    }
}
