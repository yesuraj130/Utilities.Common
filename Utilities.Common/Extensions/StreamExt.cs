
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Utilities.Common.Extensions
{
    public static class StreamExt
    {
        public static MemoryStream GetStreamCopy(this string FilePath, int timeout)
        {
            MemoryStream Result = new MemoryStream();
            try
            {
                using (FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.CopyTo(Result);
                }
            }
            catch (IOException)
            {
                Thread.Sleep(timeout);
                using (FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.CopyTo(Result);
                }
            }

            Result.Seek(0, SeekOrigin.Begin);
            return Result;
        }

        public static FileStream Open(string FilePath)
        {
            return new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public static MemoryStream GetStreamCopy(this string FilePath)
        {
            MemoryStream Result = new MemoryStream();
            using (FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fileStream.CopyTo(Result);
            }
            Result.Seek(0, SeekOrigin.Begin);
            return Result;
        }

        public static MemoryStream GetStreamCopy(this Stream fileStream)
        {
            MemoryStream Result = new MemoryStream();
            fileStream.CopyTo(Result);
            Result.Seek(0, SeekOrigin.Begin);
            fileStream.Seek(0, SeekOrigin.Begin);
            return Result;
        }

        public static void SeekBegin(this Stream stream)
        {
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
        }

        public static void SetEndLength(this Stream stream)
        {
            if (stream.CanSeek && stream.CanWrite) stream.SetLength(stream.Position);
        }

        public static MemoryStream ToZipStream(this MemoryStream sourceMemoryStream, string FileName)
        {
            var zipMemoryStream = new MemoryStream();
            using (var archive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry(FileName);
                using (var entryStream = entry.Open())
                {
                    sourceMemoryStream.CopyTo(entryStream);
                }
            }
            sourceMemoryStream.Seek(0, SeekOrigin.Begin);
            zipMemoryStream.Seek(0, SeekOrigin.Begin);
            return zipMemoryStream;
        }
        //public static MemoryStream ToZipStream(this MemoryStream sourceMemoryStream, string FileName)
        //{
        //    MemoryStream outputMemStream = new MemoryStream();
        //    ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

        //    //zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
        //    byte[] bytes = null;

        //    // loops through the PDFs I need to create

        //        var newEntry = new ZipEntry(FileName);
        //        //newEntry.DateTime = DateTime.Now;

        //        zipStream.PutNextEntry(newEntry);


        //    sourceMemoryStream.CopyTo(zipStream);



        //    zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
        //    zipStream.Close();          // Must finish the ZipOutputStream before using outputMemStream.

        //    outputMemStream.Position = 0;

        //    return outputMemStream;

        //    //var zipMemoryStream = new MemoryStream();
        //    //using (var archive = new ZipFile(zipMemoryStream, true))
        //    //{
        //    //    var entry = new ZipEntry(FileName);
        //    //    entry.
        //    //    archive.Add(FileName);
        //    //    using (var entryStream = entry.Open())
        //    //    {
        //    //        sourceMemoryStream.CopyTo(entryStream);
        //    //    }
        //    //}
        //    //sourceMemoryStream.Seek(0, SeekOrigin.Begin);
        //    //zipMemoryStream.Seek(0, SeekOrigin.Begin);
        //    //return zipMemoryStream;
        //}

        public static MemoryStream ToUnZipStream(this MemoryStream sourceMemoryStream)
        {
            var zipMemoryStream = new MemoryStream();
            using (var archive = new ZipArchive(sourceMemoryStream, ZipArchiveMode.Read, true))
            {
                using (var entryStream = archive.Entries[0].Open())
                {
                    entryStream.CopyTo(zipMemoryStream);
                }
            }
            sourceMemoryStream.Seek(0, SeekOrigin.Begin);
            zipMemoryStream.Seek(0, SeekOrigin.Begin);
            return zipMemoryStream;
        }

        //public static MemoryStream UnZipStream(this MemoryStream sourceMemoryStream)
        //{
        //    var zipMemoryStream = new MemoryStream();
        //    using (var archive = new ZipArchive(sourceMemoryStream, ZipArchiveMode.Read, true))
        //    {
        //        using (var entryStream = archive.Entries[0].Open())
        //        {
        //            entryStream.CopyTo(zipMemoryStream);
        //        }
        //    }
        //    sourceMemoryStream.Seek(0, SeekOrigin.Begin);
        //    zipMemoryStream.Seek(0, SeekOrigin.Begin);
        //    return zipMemoryStream;
        //}

        private static Stream UnZipStream(this Stream zipStream)
        {

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, true))
            {
                return archive.Entries[0].Open();

            }

        }




        //public static MemoryStream ToUnZipStream(this MemoryStream zipStream)
        //{
        //    var zipMemoryStream = new MemoryStream();
        //    using (var zipInputStream = new ZipInputStream(zipStream))
        //    {

        //        if (zipInputStream.GetNextEntry() is ZipEntry zipEntry)
        //        {
        //            var entryFileName = zipEntry.Name;
        //            // To remove the folder from the entry:
        //            //var entryFileName = Path.GetFileName(entryFileName);
        //            // Optionally match entrynames against a selection list here
        //            // to skip as desired.
        //            // The unpacked length is available in the zipEntry.Size property.

        //            // 4K is optimum
        //            //var buffer = new byte[4096];

        //            // Manipulate the output filename here as desired.
        //            //var fullZipToPath = Path.Combine(outFolder, entryFileName);
        //            //var directoryName = Path.GetDirectoryName(fullZipToPath);
        //            //if (directoryName.Length > 0)
        //            //    Directory.CreateDirectory(directoryName);

        //            // Skip directory entry
        //            //if (Path.GetFileName(fullZipToPath).Length == 0)
        //            //{
        //            //    continue;
        //            //}

        //            // Unzip file in buffered chunks. This is just as fast as unpacking
        //            // to a buffer the full size of the file, but does not waste memory.
        //            // The "using" will close the stream even if an exception occurs.
        //            zipInputStream.CopyTo(zipMemoryStream);
        //            zipStream.Seek(0, SeekOrigin.Begin);
        //            zipMemoryStream.Seek(0, SeekOrigin.Begin);

        //            //using (FileStream streamWriter = File.Create(fullZipToPath))
        //            //{
        //            //    StreamUtils.Copy(zipInputStream, streamWriter, buffer);
        //            //}
        //        }
        //    }
        //    return zipMemoryStream;
        //}

        public static bool IsZipStream(this MemoryStream sourceMemoryStream)
        {
            try
            {
                using (var archive = new ZipArchive(sourceMemoryStream, ZipArchiveMode.Read, true))
                {
                    return archive.Entries.Count > 0;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                sourceMemoryStream.Seek(0, SeekOrigin.Begin);
            }
        }
        //public static bool IsZipStream(this MemoryStream sourceMemoryStream)
        //{
        //    try
        //    {
        //        using (var archive = new ZipArchive(sourceMemoryStream, ZipArchiveMode.Read, true))
        //        {
        //            return archive.Entries.Count > 0;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    finally
        //    {
        //        sourceMemoryStream.Seek(0, SeekOrigin.Begin);
        //    }
        //}
    }
}
