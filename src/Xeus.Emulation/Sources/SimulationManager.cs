using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amoeba.Service;
using Amoeba.Messages;
using Omnius.Base;
using Omnius.Collections;
using Omnius.Io;
using Omnius.Net;
using Omnius.Security;

namespace Amoeba.Simulation
{
    public class SimulationManager : ManagerBase
    {
        private BufferManager _bufferManager = BufferManager.Instance;

        private Action<string> _callback;

        private Random _random = new Random();

        private string _basePath;
        private int _port;

        private CoreManager _coreManager;
        private TcpListener _tcpListener;

        private volatile bool _isDisposed;

        public SimulationManager(int port, Action<string> callback)
        {
            _basePath = Path.Combine(@"E:\Test\Test_CoreManager", port.ToString());
            _port = port;
            _callback = callback;
        }

        private static Socket Connect(IPEndPoint remoteEndPoint)
        {
            Socket socket = null;

            try
            {
                socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.SendTimeout = 1000 * 10;
                socket.ReceiveTimeout = 1000 * 10;
                socket.Connect(remoteEndPoint);

                return socket;
            }
            catch (SocketException)
            {
                if (socket != null) socket.Dispose();
            }

            throw new Exception();
        }

        public void Setup()
        {
            Directory.CreateDirectory(_basePath);

            string configPath = Path.Combine(_basePath, "CoreManager");
            string blockPath = Path.Combine(_basePath, "cache.blocks");

            Directory.CreateDirectory(configPath);

            _coreManager = new CoreManager(configPath, blockPath, _bufferManager);
            _coreManager.Load();

            _tcpListener = new TcpListener(IPAddress.Loopback, _port);
            _tcpListener.Start(3);

            // Connection
            {
                _coreManager.AcceptCapEvent += (object _, out string uri) => this.AcceptCap(out uri);
                _coreManager.ConnectCapEvent += (_, uri) => this.ConnectCap(uri);
            }

            _coreManager.SetMyLocation(new Location(new string[] { $"{IPAddress.Loopback}:{_port}" }));
            _coreManager.Start();
        }

        private Cap AcceptCap(out string uri)
        {
            uri = null;

            try
            {
                if (!_tcpListener.Pending()) return null;

                var socket = _tcpListener.AcceptSocketAsync().Result;

                var ipEndPoint = (IPEndPoint)socket.RemoteEndPoint;
                uri = $"{ipEndPoint.Address}:{ipEndPoint.Port}";

                return new SocketCap(socket);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Cap ConnectCap(string uri)
        {
            try
            {
                var regex = new Regex(@"(.*?):(.*)");
                var match = regex.Match(uri);

                var ipAddress = IPAddress.Parse(match.Groups[1].Value);
                int port = int.Parse(match.Groups[2].Value);

                return new SocketCap(Connect(new IPEndPoint(ipAddress, port)));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SetCloudLocations(IEnumerable<Location> locations)
        {
            _coreManager.SetCloudLocations(locations);
        }

        public void MessageUpload()
        {
            _callback.Invoke("----- CoreManager Message Upload Test (Start) -----");
            _callback.Invoke("");

            var random = RandomProvider.GetThreadRandom();

            _coreManager.Resize((long)1024 * 1024 * 1024 * 32).Wait();

            Metadata metadata = null;
            Hash hash;

            using (var stream = new RecyclableMemoryStream(_bufferManager))
            {
                using (var safeBuffer = _bufferManager.CreateSafeBuffer(1024 * 4))
                {
                    for (long remain = (long)1024 * 1024 * 256; 0 < remain; remain -= safeBuffer.Value.Length)
                    {
                        int length = (int)Math.Min(remain, safeBuffer.Value.Length);

                        random.NextBytes(safeBuffer.Value);
                        stream.Write(safeBuffer.Value, 0, length);
                    }
                }

                stream.Seek(0, SeekOrigin.Begin);
                hash = new Hash(HashAlgorithm.Sha256, Sha256.Compute(new WrapperStream(stream)));

                stream.Seek(0, SeekOrigin.Begin);
                metadata = _coreManager.VolatileSetStream(stream, new TimeSpan(1, 0, 0, 0), CancellationToken.None).Result;
            }

            using (var stream = metadata.Export(_bufferManager))
            using (var safeBuffer = _bufferManager.CreateSafeBuffer((int)stream.Length))
            {
                stream.Read(safeBuffer.Value, 0, (int)stream.Length);
                Console.WriteLine(NetworkConverter.ToBase64UrlString(safeBuffer.Value, 0, (int)stream.Length));
            }

            using (var stream = hash.Export(_bufferManager))
            using (var safeBuffer = _bufferManager.CreateSafeBuffer((int)stream.Length))
            {
                stream.Read(safeBuffer.Value, 0, (int)stream.Length);
                Console.WriteLine(NetworkConverter.ToBase64UrlString(safeBuffer.Value, 0, (int)stream.Length));
            }

            _callback.Invoke("----- CoreManager Message Upload Test (End) -----");
        }

        public void MessageDownload(IEnumerable<(Metadata metadata, Hash hash)> tuples)
        {
            _callback.Invoke("----- CoreManager Message Download Test (Start) -----");
            _callback.Invoke("");

            var random = RandomProvider.GetThreadRandom();

            _coreManager.Resize((long)1024 * 1024 * 1024 * 32).Wait();

            var sw = Stopwatch.StartNew();

            Parallel.ForEach(tuples, tuple =>
            {
                for (; ; )
                {
                    Thread.Sleep(1000);

                    Stream stream = null;

                    try
                    {
                        stream = _coreManager.VolatileGetStream(tuple.metadata, 1024 * 1024 * 1024);
                        if (stream == null) continue;

                        var hash = new Hash(HashAlgorithm.Sha256, Sha256.Compute(stream));
                        if (tuple.hash != hash) throw new ArgumentException("Broken");

                        _callback.Invoke($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ({sw.Elapsed.ToString("hh\\:mm\\:ss")}): Success {NetworkConverter.ToBase64UrlString(tuple.metadata.Hash.Value)}");

                        return;
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Dispose();
                        }
                    }
                }
            });

            _callback.Invoke("----- CoreManager Message Download Test (End) -----");
        }

        protected override void Dispose(bool isDisposing)
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (isDisposing)
            {
                _tcpListener.Stop();
                _tcpListener.Server.Dispose();

                _coreManager.Stop();
                _coreManager.Dispose();

                if (Directory.Exists(_basePath)) Directory.Delete(_basePath, true);
            }
        }
    }
}
