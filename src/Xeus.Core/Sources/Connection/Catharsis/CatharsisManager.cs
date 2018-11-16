using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using Amoeba.Messages;
using Newtonsoft.Json;
using Omnius.Base;
using Omnius.Collections;
using Omnius.Configuration;
using Omnius.Io;
using Omnius.Utils;

namespace Amoeba.Service
{
    partial class ConnectionManager
    {
        public sealed class CatharsisManager : StateManagerBase, ISettings
        {
            private BufferManager _bufferManager;

            private Settings _settings;

            private CatharsisConfig _config;

            private HashSet<SearchRange<Ipv4>> _ipv4RangeSet;

            private TimerScheduler _watchTimer;
            private TimerScheduler _updateTimer;

            private VolatileHashDictionary<Ipv4, bool> _ipv4ResultMap;

            private volatile ManagerState _state = ManagerState.Stop;

            private readonly object _lockObject = new object();
            private volatile bool _disposed;

            public CatharsisManager(string configPath, BufferManager bufferManager)
            {
                _bufferManager = bufferManager;

                _settings = new Settings(configPath);

                _watchTimer = new TimerScheduler(this.WatchThread);
                _updateTimer = new TimerScheduler(() => _ipv4ResultMap?.Update());
                _updateTimer.Start(new TimeSpan(0, 30, 0));

                _ipv4ResultMap = new VolatileHashDictionary<Ipv4, bool>(new TimeSpan(0, 30, 0));
            }

            public CatharsisConfig Config
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _config;
                    }
                }
            }

            public void SetConfig(CatharsisConfig config)
            {
                lock (_lockObject)
                {
                    _config = config;
                }

                _watchTimer.RunOnce();
            }

            public bool Check(IPAddress ipAddress)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ipv4 = new Ipv4(ipAddress);

                    lock (_lockObject)
                    {
                        return !_ipv4ResultMap.GetOrAdd(ipv4, (_) => _ipv4RangeSet.Any(n => n.Verify(ipv4)));
                    }
                }

                return true;
            }

            private void WatchThread()
            {
                try
                {
                    this.UpdateIpv4RangeSet();
                }
                catch (Exception)
                {

                }
            }

            private void UpdateIpv4RangeSet()
            {
                for (; ; )
                {
                    CatharsisConfig config;

                    lock (_lockObject)
                    {
                        config = _config;
                    }

                    var ipv4RangeSet = new HashSet<SearchRange<Ipv4>>();

                    // Ipv4
                    {
                        // path
                        foreach (string path in config.Ipv4Config.Paths)
                        {
                            try
                            {
                                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                                using (var reader = new StreamReader(stream, new UTF8Encoding(false)))
                                {
                                    ipv4RangeSet.UnionWith(this.ParseIpv4Ranges(reader));
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Warning(e);
                            }
                        }

                        // Url
                        foreach (string url in config.Ipv4Config.Urls)
                        {
                            try
                            {
                                using (var stream = this.GetStream(url))
                                using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                                using (var reader = new StreamReader(gzipStream))
                                {
                                    ipv4RangeSet.UnionWith(this.ParseIpv4Ranges(reader));
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Warning(e);
                            }
                        }
                    }

                    lock (_lockObject)
                    {
                        if (_config != config) continue;

                        _ipv4RangeSet.Clear();
                        _ipv4RangeSet.UnionWith(ipv4RangeSet);

                        _ipv4ResultMap.Clear();

                        return;
                    }
                }
            }

            private IEnumerable<SearchRange<Ipv4>> ParseIpv4Ranges(StreamReader reader)
            {
                var list = new List<SearchRange<Ipv4>>();

                using (reader)
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        int index = line.LastIndexOf(':');
                        if (index == -1) continue;

                        var items = line.Substring(index + 1).Split('-');
                        if (items.Length != 2) return null;

                        var ipv4s = new Ipv4[2];
                        ipv4s[0] = new Ipv4(IPAddress.Parse(items[0]));
                        ipv4s[1] = new Ipv4(IPAddress.Parse(items[1]));

                        int c = ipv4s[0].CompareTo(ipv4s[1]);

                        if (c < 0)
                        {
                            list.Add(new SearchRange<Ipv4>(ipv4s[0], ipv4s[1]));
                        }
                        else
                        {
                            list.Add(new SearchRange<Ipv4>(ipv4s[1], ipv4s[0]));
                        }
                    }
                }

                return list;
            }

            private Stream GetStream(string url)
            {
                var recyclableMemoryStream = new RecyclableMemoryStream(_bufferManager);

                try
                {
                    using (var client = new HttpClient())
                    {
                        using (var stream = client.GetStreamAsync(url).Result)
                        {
                            if (stream.Length > 1024 * 1024 * 32) throw new Exception("too large");

                            using (var safeBuffer = _bufferManager.CreateSafeBuffer(1024 * 4))
                            {
                                int length;

                                while ((length = stream.Read(safeBuffer.Value, 0, safeBuffer.Value.Length)) > 0)
                                {
                                    recyclableMemoryStream.Write(safeBuffer.Value, 0, length);

                                    if (recyclableMemoryStream.Length > 1024 * 1024 * 32) throw new Exception("too large");
                                }
                            }

                            recyclableMemoryStream.Seek(0, SeekOrigin.Begin);
                            return recyclableMemoryStream;
                        }
                    }
                }
                catch (Exception)
                {
                    recyclableMemoryStream.Dispose();

                    throw;
                }
            }

            public override ManagerState State
            {
                get
                {
                    return _state;
                }
            }

            private readonly object _stateLockObject = new object();

            public override void Start()
            {
                lock (_stateLockObject)
                {
                    lock (_lockObject)
                    {
                        if (this.State == ManagerState.Start) return;
                        _state = ManagerState.Start;

                        _watchTimer.Start(new TimeSpan(0, 0, 0), new TimeSpan(3, 0, 0));
                    }
                }
            }

            public override void Stop()
            {
                lock (_stateLockObject)
                {
                    lock (_lockObject)
                    {
                        if (this.State == ManagerState.Stop) return;
                        _state = ManagerState.Stop;
                    }

                    _watchTimer.Stop();
                }
            }

            #region ISettings

            public void Load()
            {
                lock (_lockObject)
                {
                    int version = _settings.Load("Version", () => 0);

                    _config = _settings.Load("Config", () => new CatharsisConfig(new CatharsisIpv4Config(Array.Empty<string>(), Array.Empty<string>())));

                    _ipv4RangeSet = _settings.Load("Ipv4RangeSet", () => new HashSet<SearchRange<Ipv4>>());
                }
            }

            public void Save()
            {
                lock (_lockObject)
                {
                    _settings.Save("Version", 0);

                    _settings.Save("Config", _config);

                    _settings.Save("Ipv4RangeSet", _ipv4RangeSet);
                }
            }

            #endregion

            [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
            readonly struct Ipv4 : IComparable<Ipv4>, IEquatable<Ipv4>
            {
                [JsonConstructor]
                public Ipv4(uint value)
                {
                    this.Value = value;
                }

                public Ipv4(IPAddress ipAddress)
                {
                    if (ipAddress.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException(nameof(ipAddress));
                    this.Value = NetworkConverter.ToUInt32(ipAddress.GetAddressBytes());
                }

                [JsonProperty]
                private uint Value { get; }

                public override int GetHashCode()
                {
                    return (int)this.Value;
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is Ipv4)) return false;
                    return this.Equals((Ipv4)obj);
                }

                public bool Equals(Ipv4 other)
                {
                    if (this.Value != other.Value)
                    {
                        return false;
                    }

                    return true;
                }

                public int CompareTo(Ipv4 other)
                {
                    return this.Value.CompareTo(other.Value);
                }

                public override string ToString()
                {
                    return new IPAddress(NetworkConverter.GetBytes(this.Value)).ToString();
                }
            }

            [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
            readonly struct SearchRange<T> : IEquatable<SearchRange<T>>
                where T : IComparable<T>, IEquatable<T>
            {
                [JsonConstructor]
                public SearchRange(T min, T max)
                {
                    this.Min = min;
                    this.Max = (max.CompareTo(this.Min) < 0) ? this.Min : max;
                }

                [JsonProperty]
                public T Min { get; }

                [JsonProperty]
                public T Max { get; }

                public bool Verify(T value)
                {
                    if (value.CompareTo(this.Min) < 0 || value.CompareTo(this.Max) > 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                public override int GetHashCode()
                {
                    return this.Min.GetHashCode() ^ this.Max.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is SearchRange<T>)) return false;
                    return this.Equals((SearchRange<T>)obj);
                }

                public bool Equals(SearchRange<T> other)
                {
                    if (!this.Min.Equals(other.Min) || !this.Max.Equals(other.Max))
                    {
                        return false;
                    }

                    return true;
                }

                public override string ToString()
                {
                    return string.Format("Min = {0}, Max = {1}", this.Min, this.Max);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (_disposed) return;
                _disposed = true;

                if (disposing)
                {
                    if (_watchTimer != null)
                    {
                        _watchTimer.Dispose();
                        _watchTimer = null;
                    }

                    if (_updateTimer != null)
                    {
                        _updateTimer.Dispose();
                        _updateTimer = null;
                    }
                }
            }
        }
    }
}
