using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.UnitTestToolkit;
using Omnius.Xeus.Engines.Models;
using Xunit;

namespace Omnius.Xeus.Engines.Storages
{
    public class WantDeclaredMessageStorageTests
    {
        [Fact]
        public async Task RegisterAndUnregisterSuccessTest()
        {
            using var deleter = FixtureFactory.GenTempDirectory(out var tempPath);
            var options = new WantDeclaredMessageStorageOptions(tempPath);
            await using var storage = await WantDeclaredMessageStorage.Factory.CreateAsync(options, BytesPool.Shared);

            var registeredSignatures = new List<OmniSignature>();

            // ランダムなサインを登録する
            foreach (var i in Enumerable.Range(0, 10))
            {
                var signature = new OmniSignature(FixtureFactory.GetRandomString(32), new OmniHash(OmniHashAlgorithmType.Sha2_256, FixtureFactory.GetRandomBytes(32)));
                await storage.RegisterWantMessageAsync(signature);
                registeredSignatures.Add(signature);
            }

            // 登録されたサインを取得する
            {
                var gotSignatures = (await storage.GetSignaturesAsync()).ToList();

                registeredSignatures.Sort((x, y) => BytesOperations.Compare(x.Hash.Value.Span, y.Hash.Value.Span));
                gotSignatures.Sort((x, y) => BytesOperations.Compare(x.Hash.Value.Span, y.Hash.Value.Span));
                AssertEx.EqualJson(registeredSignatures, gotSignatures);
            }

            // 登録されたサインの登録を解除する
            foreach (var signature in registeredSignatures)
            {
                await storage.UnregisterWantMessageAsync(signature);
            }

            // 登録されたサインが0であることを確認する
            {
                var gotSignatures = (await storage.GetSignaturesAsync()).ToList();
                Assert.Empty(gotSignatures);
            }
        }

        [Fact]
        public async Task WriteAndReadSuccessTest()
        {
            using var deleter = FixtureFactory.GenTempDirectory(out var tempPath);
            var options = new WantDeclaredMessageStorageOptions(tempPath);
            await using var storage = await WantDeclaredMessageStorage.Factory.CreateAsync(options, BytesPool.Shared);

            var registeredDigitalSignatures = new List<OmniDigitalSignature>();

            // 電子署名を作成しサインを登録する
            foreach (var i in Enumerable.Range(0, 10))
            {
                var digitalSignature = OmniDigitalSignature.Create(
                    FixtureFactory.GetRandomString(32),
                    OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256);
                await storage.RegisterWantMessageAsync(digitalSignature.GetOmniSignature());
                registeredDigitalSignatures.Add(digitalSignature);
            }

            var notRegisteredDigitalSignatures = new List<OmniDigitalSignature>();

            // 電子署名を作成する(サインは登録しない)
            foreach (var i in Enumerable.Range(0, 10))
            {
                var digitalSignature = OmniDigitalSignature.Create(
                    FixtureFactory.GetRandomString(32),
                    OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256);
                notRegisteredDigitalSignatures.Add(digitalSignature);
            }

            var wroteMessages = new List<DeclaredMessage>();

            // 登録したサインのメッセージを書き込む
            foreach (var digitalSignature in registeredDigitalSignatures)
            {
                var message = DeclaredMessage.Create(
                    FixtureFactory.GetRandomDateTimeUtc(new DateTime(2000, 1, 1), new DateTime(2100, 1, 1)),
                    new MemoryOwner<byte>(FixtureFactory.GetRandomBytes(1024)),
                    digitalSignature);
                await storage.WriteMessageAsync(message);
                wroteMessages.Add(message);
            }

            var notWroteMessages = new List<DeclaredMessage>();

            // 登録されていないサインのメッセージを書き込む
            foreach (var digitalSignature in notRegisteredDigitalSignatures)
            {
                var message = DeclaredMessage.Create(
                    FixtureFactory.GetRandomDateTimeUtc(new DateTime(2000, 1, 1), new DateTime(2100, 1, 1)),
                    new MemoryOwner<byte>(FixtureFactory.GetRandomBytes(1024)),
                    digitalSignature);
                await storage.WriteMessageAsync(message);
                notWroteMessages.Add(message);
            }

            // 登録されているサインのみ取得できることを確認する
            {
                var registeredSignatures = registeredDigitalSignatures.Select(n => n.GetOmniSignature()).ToList();
                var gotSignatures = (await storage.GetSignaturesAsync()).ToList();

                registeredSignatures.Sort((x, y) => BytesOperations.Compare(x.Hash.Value.Span, y.Hash.Value.Span));
                gotSignatures.Sort((x, y) => BytesOperations.Compare(x.Hash.Value.Span, y.Hash.Value.Span));
                AssertEx.EqualJson(registeredSignatures, gotSignatures);
            }

            // 登録されたメッセージを読み込む
            foreach (var digitalSignature in registeredDigitalSignatures)
            {
                var message = await storage.ReadMessageAsync(digitalSignature.GetOmniSignature());
                Assert.NotNull(message);
                Assert.Contains(message, wroteMessages);
            }

            // 登録されていないメッセージを読み込む
            foreach (var digitalSignature in notRegisteredDigitalSignatures)
            {
                var message = await storage.ReadMessageAsync(digitalSignature.GetOmniSignature());
                Assert.Null(message);
            }
        }
    }
}
