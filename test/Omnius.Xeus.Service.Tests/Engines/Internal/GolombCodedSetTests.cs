using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network;
using Xunit;

namespace Omnius.Xeus.Service.Engines.Internal
{
    public class GolombCodedSetTests
    {
        /// <summary>
        /// GolombCodedSetの作成とインポートとエクスポートが成功することを確認する
        /// </summary>
        [Fact]
        public void CreateAndExportAndImportSuccessTest()
        {
            var falsePositiveRate = 64;
            var itemCount = 100;
            var minValue = 0;
            var maxValue = 1000;

            var collection = new List<int>();
            var random = new Random(0);
            var computeHash = new Func<int, uint>((value) => (uint)value);

            for (int i = 0; i < itemCount; i++)
            {
                collection.Add(random.Next(minValue, maxValue));
            }

            var createdGolombCodedSet = new GolombCodedSet<int>(collection, falsePositiveRate, computeHash);

            using var hub = new BytesHub(BytesPool.Shared);
            createdGolombCodedSet.Export(hub.Writer);

            var importedGolombCodedSet = GolombCodedSet<int>.Import(hub.Reader.GetSequence(), falsePositiveRate, computeHash);

            for (int i = minValue; i < maxValue; i++)
            {
                Assert.Equal(createdGolombCodedSet.Contains(i), importedGolombCodedSet.Contains(i));
            }
        }
    }
}
