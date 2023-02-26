using System.Runtime.CompilerServices;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Serialization;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors.Internal.Repositories;

internal sealed class CachedProfileRepository : DisposableBase
{
    private IKeyValueStorage<string> _storage;
    private readonly IBytesPool _bytesPool;

    private static readonly Lazy<Base16> _base16 = new Lazy<Base16>(() => new Base16(ConvertStringCase.Lower));

    private readonly object _lockObject = new();

    public CachedProfileRepository(string dirPath, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool)
    {
        _storage = keyValueStorageFactory.Create<string>(dirPath, bytesPool);
        _bytesPool = bytesPool;
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await _storage.MigrateAsync(cancellationToken);
    }

    protected override void OnDispose(bool disposing)
    {
        _storage.Dispose();
    }

    public async IAsyncEnumerable<OmniSignature> GetSignaturesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var key in _storage.GetKeysAsync(cancellationToken))
        {
            yield return OmniSignature.Parse(key);
        }
    }

    public async ValueTask UpsertAsync(CachedProfile content, CancellationToken cancellationToken = default)
    {
        await _storage.WriteAsync(content.Signature.ToString(), content, cancellationToken);
    }

    public async ValueTask<CachedProfile?> TryReadAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        return await _storage.TryReadAsync<CachedProfile>(signature.ToString(), cancellationToken);
    }

    public async ValueTask<bool> TryDeleteAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        return await _storage.TryDeleteAsync(signature.ToString(), cancellationToken);
    }

    public async ValueTask ShrinkAsync(IEnumerable<OmniSignature> excludedSignatures, CancellationToken cancellationToken = default)
    {
        var excludedKeys = excludedSignatures.Select(n => n.ToString()).ToArray();
        await _storage.ShrinkAsync(excludedKeys, cancellationToken);
    }
}
