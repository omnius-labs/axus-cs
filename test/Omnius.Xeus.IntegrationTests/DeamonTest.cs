using System.Threading.Tasks;
using Xunit;

namespace Omnius.Xeus.IntegrationTests
{
    public class TcpConnectorTest
    {
        [Fact]
        public async Task ConnectAsyncSuccessTest()
        {
            Omnius.Core.Test.TestEnvironment.GetBasePath();
        }
    }
}
