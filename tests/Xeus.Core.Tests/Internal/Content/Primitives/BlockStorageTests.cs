using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omnix.Algorithms.Cryptography;
using Omnix.Base;
using Xeus.Core.Primitives;
using Xeus.Messages;
using Xunit;

namespace Xeus.Core.Internal.Content.Primitives
{
    public class BlockStorageTests : TestsBase
    {
        [Fact]
        public async Task RandomReadAndWriteTest()
        {
            var random = new Random();
            using var blockStorage = new BlockStorage(UnitTestEnvironment.TempDirectoryPath, BufferPool.Shared);
            var sizeList = new List<int>(new[] { 0, 1, 10, 100, 1000, 10000 });

            for (int i = 0; i < 32; i++)
            {
                sizeList.Add(random.Next(0, 10));
                sizeList.Add(random.Next(0, 256));
                sizeList.Add(random.Next(0, 1024 * 32));
            }

            await blockStorage.LoadAsync();
            blockStorage.Resize(1024 * 1024 * 1024);

            var hashList = new List<OmniHash>();

            foreach (var size in sizeList)
            {
                using var memoryOwner = BufferPool.Shared.Rent(size);
                random.NextBytes(memoryOwner.Memory.Span);
                var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memoryOwner.Memory.Span));

                Assert.True(blockStorage.TrySet(hash, memoryOwner.Memory.Span));

                hashList.Add(hash);
            }

            foreach (var hash in hashList)
            {
                Assert.True(blockStorage.TryGet(hash, out var memoryOwner));
                Assert.True(BytesOperations.SequenceEqual(hash.Value.Span, Sha2_256.ComputeHash(memoryOwner.Memory.Span)));
            }
        }
    }
}

