using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Cryptography;
using Xeus.Core.Internal.Contents.Primitives;
using Xeus.Core.Tests.Primitives;
using Xeus.Messages;
using Xunit;

namespace Xeus.Core.Tests
{
    public class BlockStorageTests : TestsBase
    {
        [Fact]
        public async Task Test()
        {
            var random = new Random();
            var options = new XeusOptions(UnitTestEnvironment.TempDirectoryPath);
            using var blockStorage = new BlockStorage(options, BufferPool.Shared);
            var sizeList = new List<int>(new[] { 0, 1, 10, 100, 1000, 10000 });

            for (int i = 0; i < 32; i++)
            {
                sizeList.Add(random.Next(0, 10));
                sizeList.Add(random.Next(0, 256));
                sizeList.Add(random.Next(0, 1024 * 32));
            }

            await blockStorage.LoadAsync();
            blockStorage.Resize(1024 * 1024 * 1024);

            foreach (var size in sizeList)
            {
                var hashList = new List<OmniHash>();

                {
                    using var memoryOwner = BufferPool.Shared.Rent(size);
                    random.NextBytes(memoryOwner.Memory.Span);
                    var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memoryOwner.Memory.Span));

                    Assert.True(blockStorage.TrySet(hash, memoryOwner.Memory.Span));

                    hashList.Add(hash);
                }
            }
        }
    }
}
