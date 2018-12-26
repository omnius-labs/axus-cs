using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Collections;
using Omnix.Configuration;

namespace Xeus.Core
{
    sealed partial class MessageManager : DisposableBase, ISettings
    {
        private BufferPool _bufferPool;
        private CoreManager _coreManager;

        private Settings _settings;

        private LockedHashSet<Signature> _searchSignatures = new LockedHashSet<Signature>();

        private Random _random = new Random();

        private readonly object _lockObject = new object();
        private volatile bool _disposed;

        public MessageManager(string configPath, CoreManager coreManager, BufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _coreManager = coreManager;

            _settings = new Settings(configPath);

            _coreManager.GetLockSignaturesEvent = (_) => this.Config.SearchSignatures;
        }

        public MessageConfig Config
        {
            get
            {
                lock (_lockObject)
                {
                    return new MessageConfig(_searchSignatures);
                }
            }
        }

        public void SetConfig(MessageConfig config)
        {
            lock (_lockObject)
            {
                _searchSignatures.Clear();
                _searchSignatures.UnionWith(config.SearchSignatures);
            }
        }

        public Task<BroadcastProfileMessage> GetProfile(Signature signature, DateTime? creationTimeLowerLimit)
        {
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var broadcastMetadata = _coreManager.GetBroadcastMetadata(signature, "Profile");
            if (broadcastMetadata == null) return Task.FromResult<BroadcastProfileMessage>(null);
            if (creationTimeLowerLimit != null && broadcastMetadata.CreationTime <= creationTimeLowerLimit) return Task.FromResult<BroadcastProfileMessage>(null);

            return Task.Run(() =>
            {
                try
                {
                    var stream = _coreManager.VolatileGetStream(broadcastMetadata.Metadata, 1024 * 1024 * 32);
                    if (stream == null) return null;

                    var content = ContentConverter.FromStream<ProfileContent>(stream, 0);
                    if (content == null) return null;

                    var result = new BroadcastProfileMessage(
                        broadcastMetadata.Certificate.GetSignature(),
                        broadcastMetadata.CreationTime,
                        content);

                    return result;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                return null;
            });
        }

        public Task<BroadcastStoreMessage> GetStore(Signature signature, DateTime? creationTimeLowerLimit)
        {
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var broadcastMetadata = _coreManager.GetBroadcastMetadata(signature, "Store");
            if (broadcastMetadata == null) return Task.FromResult<BroadcastStoreMessage>(null);
            if (creationTimeLowerLimit != null && broadcastMetadata.CreationTime <= creationTimeLowerLimit) return Task.FromResult<BroadcastStoreMessage>(null);

            return Task.Run(() =>
            {
                try
                {
                    var stream = _coreManager.VolatileGetStream(broadcastMetadata.Metadata, 1024 * 1024 * 32);
                    if (stream == null) return null;

                    var content = ContentConverter.FromStream<StoreContent>(stream, 0);
                    if (content == null) return null;

                    var result = new BroadcastStoreMessage(
                        broadcastMetadata.Certificate.GetSignature(),
                        broadcastMetadata.CreationTime,
                        content);

                    return result;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                return null;
            });
        }

        public Task<IEnumerable<UnicastCommentMessage>> GetUnicastCommentMessages(Signature signature, AgreementPrivateKey agreementPrivateKey,
            int messageCountUpperLimit, IEnumerable<MessageCondition> conditions)
        {
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            if (agreementPrivateKey == null) throw new ArgumentNullException(nameof(agreementPrivateKey));

            return Task.Run(() =>
            {
                try
                {
                    var filter = new Dictionary<Signature, HashSet<DateTime>>();
                    {
                        foreach (var item in conditions)
                        {
                            filter.GetOrAdd(item.AuthorSignature, (_) => new HashSet<DateTime>()).Add(item.CreationTime);
                        }
                    }

                    var trustedMetadatas = new List<UnicastMetadata>();
                    {
                        foreach (var unicastMetadata in _coreManager.GetUnicastMetadatas(signature, "MailMessage"))
                        {
                            if (!_searchSignatures.Contains(unicastMetadata.Certificate.GetSignature())) continue;

                            trustedMetadatas.Add(unicastMetadata);
                        }

                        trustedMetadatas.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime));
                    }

                    var results = new List<UnicastCommentMessage>();

                    foreach (var unicastMetadata in trustedMetadatas.Take(messageCountUpperLimit))
                    {
                        if (filter.TryGetValue(unicastMetadata.Certificate.GetSignature(), out var hashSet) && hashSet.Contains(unicastMetadata.CreationTime)) continue;

                        var stream = _coreManager.VolatileGetStream(unicastMetadata.Metadata, 1024 * 1024 * 1);
                        if (stream == null) continue;

                        var result = new UnicastCommentMessage(
                            unicastMetadata.Signature,
                            unicastMetadata.Certificate.GetSignature(),
                            unicastMetadata.CreationTime,
                            ContentConverter.FromCryptoStream<CommentContent>(stream, agreementPrivateKey, 0));

                        if (result.Value == null) continue;

                        results.Add(result);
                    }

                    return (IEnumerable<UnicastCommentMessage>)results.ToArray();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                return Array.Empty<UnicastCommentMessage>();
            });
        }

        public Task<IEnumerable<MulticastCommentMessage>> GetMulticastCommentMessages(Tag tag,
            int trustMessageCountUpperLimit, int untrustMessageCountUpperLimit, IEnumerable<MessageCondition> conditions)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            var now = DateTime.UtcNow;

            return Task.Run(() =>
            {
                try
                {
                    var filter = new Dictionary<Signature, HashSet<DateTime>>();
                    {
                        foreach (var item in conditions)
                        {
                            filter.GetOrAdd(item.AuthorSignature, (_) => new HashSet<DateTime>()).Add(item.CreationTime);
                        }
                    }

                    var trustedMetadatas = new List<MulticastMetadata>();
                    var untrustedMetadatas = new List<MulticastMetadata>();
                    {
                        foreach (var multicastMetadata in _coreManager.GetMulticastMetadatas(tag, "ChatMessage"))
                        {
                            if (_searchSignatures.Contains(multicastMetadata.Certificate.GetSignature()))
                            {
                                trustedMetadatas.Add(multicastMetadata);
                            }
                            else
                            {
                                untrustedMetadatas.Add(multicastMetadata);
                            }
                        }

                        trustedMetadatas.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime));
                        untrustedMetadatas.Sort((x, y) =>
                        {
                            int c;
                            if (0 != (c = y.Cost.CashAlgorithm.CompareTo(x.Cost.CashAlgorithm))) return c;
                            if (0 != (c = y.Cost.Value.CompareTo(x.Cost.Value))) return c;

                            return y.CreationTime.CompareTo(x.CreationTime);
                        });
                    }

                    var results = new List<MulticastCommentMessage>();

                    foreach (var multicastMetadata in CollectionUtils.Unite(trustedMetadatas.Take(trustMessageCountUpperLimit), untrustedMetadatas.Take(untrustMessageCountUpperLimit)))
                    {
                        if (filter.TryGetValue(multicastMetadata.Certificate.GetSignature(), out var hashSet) && hashSet.Contains(multicastMetadata.CreationTime)) continue;

                        var stream = _coreManager.VolatileGetStream(multicastMetadata.Metadata, 1024 * 1024 * 1);
                        if (stream == null) continue;

                        var content = ContentConverter.FromStream<CommentContent>(stream, 0);
                        if (content == null) continue;

                        var result = new MulticastCommentMessage(
                            multicastMetadata.Tag,
                            multicastMetadata.Certificate.GetSignature(),
                            multicastMetadata.CreationTime,
                            multicastMetadata.Cost,
                            content);

                        results.Add(result);
                    }

                    return (IEnumerable<MulticastCommentMessage>)results.ToArray();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                return Array.Empty<MulticastCommentMessage>();
            });
        }

        public Task Upload(ProfileContent profile, DigitalSignature digitalSignature, CancellationToken token)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (digitalSignature == null) throw new ArgumentNullException(nameof(digitalSignature));

            return _coreManager.VolatileSetStream(ContentConverter.ToStream(profile, 0), TimeSpan.FromDays(360), token)
                .ContinueWith(task =>
                {
                    _coreManager.UploadMetadata(new BroadcastMetadata("Profile", DateTime.UtcNow, task.Result, digitalSignature));
                });
        }

        public Task Upload(StoreContent store, DigitalSignature digitalSignature, CancellationToken token)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (digitalSignature == null) throw new ArgumentNullException(nameof(digitalSignature));

            return _coreManager.VolatileSetStream(ContentConverter.ToStream(store, 0), TimeSpan.FromDays(360), token)
                .ContinueWith(task =>
                {
                    _coreManager.UploadMetadata(new BroadcastMetadata("Store", DateTime.UtcNow, task.Result, digitalSignature));
                });
        }

        public Task Upload(Signature targetSignature, CommentContent comment, AgreementPublicKey agreementPublicKey, DigitalSignature digitalSignature, CancellationToken token)
        {
            if (targetSignature == null) throw new ArgumentNullException(nameof(targetSignature));
            if (comment == null) throw new ArgumentNullException(nameof(comment));
            if (digitalSignature == null) throw new ArgumentNullException(nameof(digitalSignature));

            return _coreManager.VolatileSetStream(ContentConverter.ToCryptoStream(comment, 1024 * 256, agreementPublicKey, 0), TimeSpan.FromDays(360), token)
                .ContinueWith(task =>
                {
                    _coreManager.UploadMetadata(new UnicastMetadata("MailMessage", targetSignature, DateTime.UtcNow, task.Result, digitalSignature));
                });
        }

        public Task Upload(Tag tag, CommentContent comment, DigitalSignature digitalSignature, TimeSpan miningTime, CancellationToken token)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));
            if (comment == null) throw new ArgumentNullException(nameof(comment));
            if (digitalSignature == null) throw new ArgumentNullException(nameof(digitalSignature));

            return _coreManager.VolatileSetStream(ContentConverter.ToStream(comment, 0), TimeSpan.FromDays(360), token)
                .ContinueWith(task =>
                {
                    MulticastMetadata multicastMetadata;

                    try
                    {
                        var miner = new Miner(CashAlgorithm.Version1, -1, miningTime);
                        multicastMetadata = new MulticastMetadata("ChatMessage", tag, DateTime.UtcNow, task.Result, digitalSignature, miner, token);
                    }
                    catch (MinerException)
                    {
                        return;
                    }

                    _coreManager.UploadMetadata(multicastMetadata);
                });
        }

        #region ISettings

        public void Load()
        {
            lock (_lockObject)
            {
                int version = _settings.Load("Version", () => 0);

                {
                    var searchSignatures = _settings.Load("SearchSignatures", () => Array.Empty<Signature>());

                    this.SetConfig(new MessageConfig(searchSignatures));
                }
            }
        }

        public void Save()
        {
            lock (_lockObject)
            {
                _settings.Save("Version", 0);

                {
                    var config = this.Config;

                    _settings.Save("SearchSignatures", config.SearchSignatures);
                }
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {

            }
        }
    }

    class MessageManagerException : ManagerException
    {
        public MessageManagerException() : base() { }
        public MessageManagerException(string message) : base(message) { }
        public MessageManagerException(string message, Exception innerException) : base(message, innerException) { }
    }
}
