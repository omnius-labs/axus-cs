using System.Linq;

using System.IO;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Internal;
using Xunit;

namespace Omnius.Xeus.Service.Repositories
{
    public class WantDeclaredMessageRepositoryTest
    {
        [Fact]
        public async Task Single_AddWant_And_GetWants_And_RemoveWant_Test()
        {
            using var _ = TestHelpers.GenTempDirectory(out var tempPath);

            var options = new WantDeclaredMessageRepositoryOptions(tempPath);
            var repo = await WantDeclaredMessageRepository.Factory.CreateAsync(options, BytesPool.Shared);

            var addedHash = new OmniHash(OmniHashAlgorithmType.Sha2_256, TestHelpers.GetRandomBytes(32));
            repo.AddWant(addedHash);
            var gotWants1 = repo.GetWants();
            Assert.Single(gotWants1);
            Assert.Equal(addedHash, gotWants1.First());

            repo.RemoveWant(addedHash);
            var gotWants2 = repo.GetWants();
            Assert.Empty(gotWants2);
        }
    }
}
