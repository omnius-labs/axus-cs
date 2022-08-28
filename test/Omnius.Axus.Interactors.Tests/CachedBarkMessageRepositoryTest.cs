using FluentAssertions;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.UnitTestToolkit;
using Xunit;

namespace Omnius.Axus.Interactors;

public class CachedBarkMessageRepositoryTest
{
    [Fact]
    public async Task InsertBulkAndFetchByTagTest()
    {
        var bytesPool = BytesPool.Shared;
        var tempDirDeleter = FixtureFactory.GenTempDirectory(out var tempDir);
        var repo = new CachedBarkMessageRepository(tempDir, bytesPool);

        await repo.MigrateAsync();

        var insertMessages = new[] {
            GenRandomCachedBarkMessage(),
            GenRandomCachedBarkMessage(),
            GenRandomCachedBarkMessage()
        };
        repo.InsertBulk(insertMessages);

        var fetchedMessages = repo.FetchByTag("tag");
        insertMessages.Should().BeEquivalentTo(fetchedMessages);
    }

    private static CachedBarkMessage GenRandomCachedBarkMessage()
    {
        return new CachedBarkMessage(
            OmniDigitalSignature.Create("aaa", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256).GetOmniSignature(),
            new BarkMessage(
                "tag",
                Timestamp64.FromDateTime(DateTime.UtcNow),
                "comment",
                new OmniHash(OmniHashAlgorithmType.Sha2_256, FixtureFactory.GetRandomBytes(32))
            )
        );
    }
}
