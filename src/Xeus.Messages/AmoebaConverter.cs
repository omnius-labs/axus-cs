using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Omnix.Base;
using Omnix.Io;
using Omnix.Security;
using Omnix.Serialization;

namespace Amoeba.Messages
{
    public static class AmoebaConverter
    {
        enum ConvertCompressionAlgorithm
        {
            None = 0,
            Deflate = 1,
        }

        private static readonly BufferManager _bufferManager = BufferManager.Instance;
        private static readonly Regex _base64Regex = new Regex(@"^([a-zA-Z0-9\-_]*).*?$", RegexOptions.Compiled | RegexOptions.Singleline);

        private static Stream ToStream<T>(int version, MessageBase<T> item)
                where T : MessageBase<T>
        {
            Stream stream = null;
            
            try
            {
                stream = new RangeStream(item.Export(_bufferManager));

                var dic = new Dictionary<byte, Stream>();

                try
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    RecyclableMemoryStream deflateBufferStream = null;

                    try
                    {
                        deflateBufferStream = new RecyclableMemoryStream(_bufferManager);

                        using (var deflateStream = new DeflateStream(deflateBufferStream, CompressionMode.Compress, true))
                        using (var safeBuffer = _bufferManager.CreateSafeBuffer(1024 * 4))
                        {
                            int length;

                            while ((length = stream.Read(safeBuffer.Value, 0, safeBuffer.Value.Length)) > 0)
                            {
                                deflateStream.Write(safeBuffer.Value, 0, length);
                            }
                        }

                        deflateBufferStream.Seek(0, SeekOrigin.Begin);

                        dic.Add((byte)ConvertCompressionAlgorithm.Deflate, deflateBufferStream);
                    }
                    catch (Exception)
                    {
                        if (deflateBufferStream != null)
                        {
                            deflateBufferStream.Dispose();
                        }
                    }
                }
                catch (Exception)
                {

                }

                dic.Add((byte)ConvertCompressionAlgorithm.None, stream);

                var list = dic.ToList();

                list.Sort((x, y) =>
                {
                    int c = x.Value.Length.CompareTo(y.Value.Length);
                    if (c != 0) return c;

                    return x.Key.CompareTo(y.Key);
                });

                for (int i = 1; i < list.Count; i++)
                {
                    list[i].Value.Dispose();
                }

                var headerStream = new RecyclableMemoryStream(_bufferManager);
                Varint.SetUInt64(headerStream, (uint)version);
                Varint.SetUInt64(headerStream, list[0].Key);

                var dataStream = new UniteStream(headerStream, list[0].Value);

                var crcStream = new MemoryStream(Crc32_Castagnoli.Compute(new WrapperStream(dataStream)));
                return new UniteStream(dataStream, crcStream);
            }
            catch (Exception ex)
            {
                if (stream != null)
                {
                    stream.Dispose();
                }

                throw new ArgumentException(ex.Message, ex);
            }
        }

        private static T FromStream<T>(int version, Stream stream)
            where T : MessageBase<T>
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);

                // Check
                {
                    var verifyCrc = Crc32_Castagnoli.Compute(new RangeStream(stream, 0, stream.Length - 4, true));
                    var orignalCrc = new byte[4];

                    using (var crcStream = new RangeStream(stream, stream.Length - 4, 4, true))
                    {
                        crcStream.Read(orignalCrc, 0, orignalCrc.Length);
                    }

                    if (!BytesOperations.SequenceEqual(verifyCrc, orignalCrc))
                    {
                        throw new ArgumentException("Crc Error");
                    }
                }

                stream.Seek(0, SeekOrigin.Begin);

                if (version != (int)Varint.GetUInt64(stream)) throw new ArgumentException("version");
                int type = (int)Varint.GetUInt64(stream);

                using (var dataStream = new RangeStream(stream, stream.Position, stream.Length - stream.Position - 4, true))
                {
                    if (type == (int)ConvertCompressionAlgorithm.None)
                    {
                        return MessageBase<T>.Import(dataStream, _bufferManager);
                    }
                    else if (type == (int)ConvertCompressionAlgorithm.Deflate)
                    {
                        using (var deflateBufferStream = new RecyclableMemoryStream(_bufferManager))
                        {
                            using (var deflateStream = new DeflateStream(dataStream, CompressionMode.Decompress, true))
                            using (var safeBuffer = _bufferManager.CreateSafeBuffer(1024 * 4))
                            {
                                int length;

                                while ((length = deflateStream.Read(safeBuffer.Value, 0, safeBuffer.Value.Length)) > 0)
                                {
                                    deflateBufferStream.Write(safeBuffer.Value, 0, length);

                                    if (deflateBufferStream.Length > 1024 * 1024 * 32) throw new Exception("too large");
                                }
                            }

                            deflateBufferStream.Seek(0, SeekOrigin.Begin);

                            return MessageBase<T>.Import(deflateBufferStream, _bufferManager);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("ArgumentException");
                    }
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message, e);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        private static string ToBase64String(Stream stream)
        {
            using (var targetStream = new RangeStream(stream, true))
            using (var safeBuffer = _bufferManager.CreateSafeBuffer((int)targetStream.Length))
            {
                targetStream.Seek(0, SeekOrigin.Begin);
                targetStream.Read(safeBuffer.Value, 0, (int)targetStream.Length);

                return NetworkConverter.ToBase64UrlString(safeBuffer.Value, 0, (int)targetStream.Length);
            }
        }

        private static Stream FromBase64String(string value)
        {
            var match = _base64Regex.Match(value);
            if (!match.Success) throw new ArgumentException();

            value = match.Groups[1].Value;

            return new MemoryStream(NetworkConverter.FromBase64UrlString(value));
        }

        public static string ToLocationString(Location item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            try
            {
                using (var stream = AmoebaConverter.ToStream<Location>(0, item))
                {
                    return "Location:" + AmoebaConverter.ToBase64String(stream);
                }
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }

        public static Location FromLocationString(string item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!item.StartsWith("Location:")) throw new ArgumentException("item");

            try
            {
                using (var stream = AmoebaConverter.FromBase64String(item.Remove(0, "Location:".Length)))
                {
                    return AmoebaConverter.FromStream<Location>(0, stream);
                }
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }

        public static string ToTagString(Tag item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            try
            {
                using (var stream = AmoebaConverter.ToStream<Tag>(0, item))
                {
                    return "Tag:" + AmoebaConverter.ToBase64String(stream);
                }
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }

        public static Tag FromTagString(string item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!item.StartsWith("Tag:")) throw new ArgumentException("item");

            try
            {
                using (var stream = AmoebaConverter.FromBase64String(item.Remove(0, "Tag:".Length)))
                {
                    return AmoebaConverter.FromStream<Tag>(0, stream);
                }
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }

        public static string ToSeedString(Seed item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            try
            {
                using (var stream = AmoebaConverter.ToStream<Seed>(0, item))
                {
                    return "Seed:" + AmoebaConverter.ToBase64String(stream);
                }
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }

        public static Seed FromSeedString(string item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!item.StartsWith("Seed:")) throw new ArgumentException("item");

            try
            {
                using (var stream = AmoebaConverter.FromBase64String(item.Remove(0, "Seed:".Length)))
                {
                    return AmoebaConverter.FromStream<Seed>(0, stream);
                }
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }

        public static Stream ToBoxStream(Box item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            try
            {
                return AmoebaConverter.ToStream<Box>(0, item);
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }

        public static Box FromBoxStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            try
            {
                return AmoebaConverter.FromStream<Box>(0, stream);
            }
            catch (Exception)
            {
                throw new FormatException();
            }
        }
    }
}
