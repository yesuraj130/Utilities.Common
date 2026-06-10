
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Common.Extensions
{
    public static class ZipExt
    {


        private static MemoryStream InternalUnZipStream(Stream zipStream)
        {
            var unzipMemoryStream = new MemoryStream();

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                using (var entryStream = archive.Entries[0].Open())
                {
                    entryStream.CopyTo(unzipMemoryStream);
                }
            }

            unzipMemoryStream.Seek(0, SeekOrigin.Begin);
            return unzipMemoryStream;
        }
        private static void InternalZipStream(Stream sourceUnZipStream, Stream DestinationZipMemoryStream, string FileName)
        {
            var entryName = "Entry1";
            if (FileName != null) entryName = Path.GetFileName(FileName);
            using (var archive = new ZipArchive(DestinationZipMemoryStream, ZipArchiveMode.Create))
            {
                using (var entryStream = archive.CreateEntry(entryName).Open())
                {
                    sourceUnZipStream.Seek(0, SeekOrigin.Begin);
                    sourceUnZipStream.CopyTo(entryStream);
                }
            }
        }

        public static DeflateStream DeflateStream(this Stream outputStream) => new DeflateStream(outputStream, CompressionMode.Compress, true);
        public static DeflateStream UnDeflateStream(this Stream inputStream) => new DeflateStream(inputStream, CompressionMode.Decompress, true);

        public static ZipArchiveStream ZipStream(this Stream outputStream) => new ZipArchiveStream(outputStream, ZipArchiveMode.Create, true);
        public static ZipArchiveStream UnZipStream(this Stream zipStream) => new ZipArchiveStream(zipStream, ZipArchiveMode.Read, true);



        public static MemoryStream UnZip(string Location)
        {
            using (var Fs = new FileStream(Location, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return InternalUnZipStream(Fs);
            }
        }

        [Obsolete("Obsolete")]
        public static void Zip(Stream sourceUnZipStream, string Location, bool closeStream)
        {
            using (var fileStream = new FileStream(Location, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                if (closeStream) using (sourceUnZipStream) InternalZipStream(sourceUnZipStream, fileStream, Location);
                else InternalZipStream(sourceUnZipStream, fileStream, Location);
            }
        }

        public static void Zip(Stream sourceUnZipStream, string Location)
        {
            using (var fileStream = new FileStream(Location, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                InternalZipStream(sourceUnZipStream, fileStream, Location);
            }
        }

    }

    public class ZipArchiveStream : Stream
    {
        public Stream BaseStream { get; }
        private readonly ZipArchive zipArchive;
        private readonly ZipArchiveEntry zipEntry;
        private Stream ZipStream { get; }

        public ZipArchiveStream(Stream stream, ZipArchiveMode zipArchiveMode, bool leaveOpen)
        {
            BaseStream = stream;
            zipArchive = new ZipArchive(stream, zipArchiveMode, leaveOpen);
            if (zipArchiveMode == ZipArchiveMode.Create) zipEntry = zipArchive.CreateEntry("data");
            else zipEntry = zipArchive.Entries[0];
            ZipStream = zipEntry.Open();
        }
        public override void Flush() => ZipStream.Flush();

        public override long Seek(long offset, SeekOrigin origin) => ZipStream.Seek(offset, origin);
        public override void SetLength(long value) => ZipStream.SetLength(value);
        public override int Read(byte[] buffer, int offset, int count) => ZipStream.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => ZipStream.Write(buffer, offset, count);

        public override bool CanRead => ZipStream.CanRead;
        public override bool CanSeek => ZipStream.CanSeek;
        public override bool CanWrite => ZipStream.CanWrite;
        public override long Length => ZipStream.Length;
        public override long Position { get => ZipStream.Position; set => ZipStream.Position = value; }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ZipStream.Dispose();
                zipArchive.Dispose();
            }
            base.Dispose(disposing);
        }
        public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject) => ZipStream.BeginRead(array, offset, numBytes, userCallback, stateObject);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => ZipStream.ReadAsync(buffer, offset, count, cancellationToken);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => ZipStream.WriteAsync(buffer, offset, count, cancellationToken);
        public override Task FlushAsync(CancellationToken cancellationToken) => ZipStream.FlushAsync(cancellationToken);
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => ZipStream.BeginWrite(buffer, offset, count, callback, state);

        public override bool CanTimeout => ZipStream.CanTimeout;
        public override void Close() { ZipStream.Close(); base.Close(); }
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => ZipStream.CopyToAsync(destination, bufferSize, cancellationToken);
        public override System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType) => ZipStream.CreateObjRef(requestedType);
        public override int EndRead(IAsyncResult asyncResult) => ZipStream.EndRead(asyncResult);
        public override void EndWrite(IAsyncResult asyncResult) => ZipStream.EndWrite(asyncResult);
        //public override string ToString() => base.ToString();
        //public override bool Equals(object obj) => zipStream.Equals(obj);
        //public override int GetHashCode() => zipStream.GetHashCode();
        public override object InitializeLifetimeService() => ZipStream.InitializeLifetimeService();
        //protected override WaitHandle CreateWaitHandle() => base.CreateWaitHandle();
        //protected override void ObjectInvariant() => base.ObjectInvariant();
        public override int ReadByte() => ZipStream.ReadByte();
        public override int ReadTimeout { get => ZipStream.ReadTimeout; set => ZipStream.ReadTimeout = value; }
        public override void WriteByte(byte value) => ZipStream.WriteByte(value);
        public override int WriteTimeout { get => ZipStream.WriteTimeout; set => ZipStream.WriteTimeout = value; }

    }
}
