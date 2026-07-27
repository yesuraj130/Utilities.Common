using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Utilities.Common
{
    public static class NtDirectory
    {
        #region NativeMethods
        [DllImport("ntdll.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern NtStatus NtQueryDirectoryFileEx(IntPtr fileHandle, IntPtr _event, IntPtr apcRoutine, IntPtr apcContext, ref IO_STATUS_BLOCK ioStatusBlock, IntPtr fileInformation, int length, FileInformationClass fileInformationClass, QueryFlags queryFlags, IntPtr fileName);

        private enum NtStatus : uint
        {
            Success = 0x00000000,
            NoMoreFile = 0x80000006,
            NoSuchFile = 0xC000000F,
            BufferTooSmall = 0xC0000023,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_STATUS_BLOCK
        {
            public uint Status;         //The NTSTATUS code response
            public IntPtr Information;  //Pointer to actual bytes returned
        }

        private enum FileInformationClass : uint
        {
            FileDirectoryInformation = 1,
            FileFullDirectoryInformation = 2,
            FileBothDirectoryInformation = 3,
            FileNamesInformation = 12,
            FileIdBothDirectoryInformation = 37
        }

        [Flags]
        private enum QueryFlags : uint
        {
            None = 0x00000000,
            RestartScan = 0x00000001,
            ReturnSingleEntry = 0x00000002,
            IndexSpecified = 0x00000004,
            ReturnOnNoMatches = 0x00000008,
            NoOnDiskEnumeration = 0x00000010,
        }


        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr CreateFileW(string fileName, DesiredAccess desiredAccess, ShareMode shareMode, IntPtr securityAttributes, CreationDisposition creationDisposition, FlagsAndAttributes flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool CloseHandle(IntPtr hObject);

        [Flags]
        private enum DesiredAccess : uint
        {
            FileListDirectory = 0x0001,
            GenericWrite = 0x40000000,
            GenericRead = 0x80000000,
        }

        [Flags]
        private enum ShareMode : uint
        {
            None = 0x00000000,
            Read = 0x00000001,
            Write = 0x00000002,
            Delete = 0x00000004,
            All = Read | Write | Delete,
        }

        private enum CreationDisposition : uint
        {
            CreateNew = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5,
        }

        [Flags]
        private enum FlagsAndAttributes : uint
        {
            Normal = 0x00000080,
            BackupSemantics = 0x02000000, // Required to open folder handles
        }

        [Flags]
        private enum GetFinalPathFlags : uint
        {
            FileNameNormalized = 0x0,
            VolumeNameDos = 0x0,
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern uint GetFinalPathNameByHandleW(IntPtr hFile, char[] lpszFilePath, uint cchFilePath, GetFinalPathFlags dwFlags);


        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        #endregion

        private static List<FileSystemEntry> ParseFileAndFolderEntries(IntPtr buffer, string parentFolderPath)
        {
            var results = new List<FileSystemEntry>();
            //var currentBuffer = (byte*)buffer;
            var currentBuffer = buffer;
            while (true)
            {
                //parse all meta data fields using the Struct layout
                //var info = *(FileIdBothDirInformation*)currentBuffer;
                var info = Marshal.PtrToStructure<FileIdBothDirInformation>(currentBuffer);

                //parse fileName
                //var namePtr = (char*)(currentBuffer + FileIdBothDirInformation.fileNameOffset);
                //var entryName = new string(namePtr, 0, (int)(info.FileNameLength / 2));
                string entryName = Marshal.PtrToStringUni(IntPtr.Add(currentBuffer, FileIdBothDirInformation.fileNameOffset), (int)(info.FileNameLength / 2));

                if (!string.IsNullOrEmpty(entryName) && entryName != "." && entryName != "..")
                {
                    results.Add(new FileSystemEntry(info, entryName, parentFolderPath));
                }

                if (info.NextEntryOffset == 0) break;
                //currentBuffer += info.NextEntryOffset;
                currentBuffer = IntPtr.Add(currentBuffer, (int)info.NextEntryOffset);
            }
            return results;
        }
        private static List<FileSystemEntry> ParseFileEntries(IntPtr buffer, string parentFolderPath)
        {
            var results = new List<FileSystemEntry>();
            //var currentBuffer = (byte*)buffer;
            var currentBuffer = buffer;
            while (true)
            {
                //parse all meta data fields using the Struct layout
                //var info = *(FileIdBothDirInformation*)currentBuffer;
                var info = Marshal.PtrToStructure<FileIdBothDirInformation>(currentBuffer);

                if ((info.FileAttributes & (uint)FileAttributes.Directory) == 0)
                {
                    //parse fileName
                    //var namePtr = (char*)(currentBuffer + FileIdBothDirInformation.fileNameOffset);
                    //var entryName = new string(namePtr, 0, (int)(info.FileNameLength / 2));
                    string entryName = Marshal.PtrToStringUni(IntPtr.Add(currentBuffer, FileIdBothDirInformation.fileNameOffset), (int)(info.FileNameLength / 2));

                    if (!string.IsNullOrEmpty(entryName) && entryName != "." && entryName != "..")
                    {
                        results.Add(new FileSystemEntry(info, entryName, parentFolderPath));
                    }
                }

                if (info.NextEntryOffset == 0) break;
                //currentBuffer += info.NextEntryOffset;
                currentBuffer = IntPtr.Add(currentBuffer, (int)info.NextEntryOffset);
            }
            return results;
        }
        private static List<FileSystemEntry> ParseFolderEntries(IntPtr buffer, string parentFolderPath)
        {
            var results = new List<FileSystemEntry>();
            //var currentBuffer = (byte*)buffer;
            var currentBuffer = buffer;
            while (true)
            {
                //parse all meta data fields using the Struct layout
                //var info = *(FileIdBothDirInformation*)currentBuffer;
                var info = Marshal.PtrToStructure<FileIdBothDirInformation>(currentBuffer);

                if ((info.FileAttributes & (uint)FileAttributes.Directory) != 0)
                {
                    //parse fileName
                    //var namePtr = (char*)(currentBuffer + FileIdBothDirInformation.fileNameOffset);
                    //var entryName = new string(namePtr, 0, (int)(info.FileNameLength / 2));
                    string entryName = Marshal.PtrToStringUni(IntPtr.Add(currentBuffer, FileIdBothDirInformation.fileNameOffset), (int)(info.FileNameLength / 2));

                    if (!string.IsNullOrEmpty(entryName) && entryName != "." && entryName != "..")
                    {
                        results.Add(new FileSystemEntry(info, entryName, parentFolderPath));
                    }
                }

                if (info.NextEntryOffset == 0) break;
                //currentBuffer += info.NextEntryOffset;
                currentBuffer = IntPtr.Add(currentBuffer, (int)info.NextEntryOffset);
            }
            return results;
        }

        private static IntPtr CreateUnicodeString(string str)
        {
            UNICODE_STRING unicode;

            unicode.Length = (ushort)(str.Length * 2);
            unicode.MaximumLength = (ushort)((str.Length * 2) + 2);
            unicode.Buffer = Marshal.StringToHGlobalUni(str);

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<UNICODE_STRING>());
            Marshal.StructureToPtr(unicode, ptr, false);

            return ptr;
        }
        private static void FreeUnicodeString(IntPtr unicodePtr)
        {
            if (unicodePtr == IntPtr.Zero) return;
            var unicode = Marshal.PtrToStructure<UNICODE_STRING>(unicodePtr);
            if (unicode.Buffer != IntPtr.Zero) Marshal.FreeHGlobal(unicode.Buffer);

            Marshal.FreeHGlobal(unicodePtr);
        }
        private static string GetCanonicalDirectoryPath(string folderPath)
        {
            var handle = CreateFileW(folderPath, DesiredAccess.FileListDirectory, ShareMode.All, IntPtr.Zero, CreationDisposition.OpenExisting, FlagsAndAttributes.BackupSemantics, IntPtr.Zero);
            if (handle == IntPtr.Zero || handle == invalidHandle) return null;

            try
            {
                uint bufferSize = 1024;
                var buffer = new char[bufferSize];
                var length = GetFinalPathNameByHandleW(handle, buffer, bufferSize, 0);
                if (length == 0) return null;

                if (length > bufferSize) //If the path is longer than bufferSize, the API returns the exact required size
                {
                    bufferSize = length + 1; //Allocate length +1 to guarantee breathing room for the null terminator
                    buffer = new char[bufferSize]; // Reallocate to the exact required size
                    length = GetFinalPathNameByHandleW(handle, buffer, bufferSize, 0);
                    if (length == 0) return null;
                }

                var finalPath = new string(buffer, 0, (int)length);

                if (finalPath.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase)) finalPath = @"\\" + finalPath.Substring(8);  // remove \\?\ prefix
                else if (finalPath.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase)) finalPath = finalPath.Substring(4);
                if (finalPath.Length > 3 && finalPath.EndsWith("\\")) finalPath = finalPath.TrimEnd('\\');
                return finalPath;
            }
            finally
            {
                 QueueCloseHandle(handle);
            }
        }

        private const int maxCloseHandleThreads = 32;
        private const int threadIdleTimeout = 500; //ms (milli second)
        private static int activeCloseHandleThreadsCount;
        private static readonly BlockingCollection<IntPtr> closeHandleQueue = new BlockingCollection<IntPtr>(boundedCapacity: 1000);
        private static void QueueCloseHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            try
            {
                if (closeHandleQueue.TryAdd(handle))
                {
                    var currentThreadsCount = System.Threading.Volatile.Read(ref activeCloseHandleThreadsCount);

                    if (currentThreadsCount < maxCloseHandleThreads)
                    {
                        var predictedThreadsCount = System.Threading.Interlocked.Increment(ref activeCloseHandleThreadsCount);
                        if (predictedThreadsCount > maxCloseHandleThreads)
                        {
                            System.Threading.Interlocked.Decrement(ref activeCloseHandleThreadsCount);
                            return;
                        }

                        _ = Task.Run(ProcessCloseHandleQueue);
                    }
                }
                else
                {
                    CloseHandle(handle);
                }
            }
            catch (InvalidOperationException)
            {
                CloseHandle(handle);
            }

            static void ProcessCloseHandleQueue()
            {
                try
                {
                    while (!closeHandleQueue.IsCompleted)
                    {
                        if (!closeHandleQueue.TryTake(out var handle, threadIdleTimeout)) break;
                        CloseHandle(handle);
                    }
                }
                finally
                {
                    System.Threading.Interlocked.Decrement(ref activeCloseHandleThreadsCount);
                }
            }
        }


        private static readonly IntPtr invalidHandle = new IntPtr(-1);
        private static IEnumerable<List<FileSystemEntry>> GetFileSystemEntriesInternal(string folderPath, string searchPattern, bool getFiles, bool getFolders, int bufferSize)
        {
            var handle = CreateFileW(folderPath, DesiredAccess.FileListDirectory, ShareMode.All, IntPtr.Zero, CreationDisposition.OpenExisting, FlagsAndAttributes.BackupSemantics, IntPtr.Zero);
            if (handle == IntPtr.Zero || handle == invalidHandle) throw new InvalidOperationException("Failed to open directory");

            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
            var searchPatternPointer = searchPattern is not null && searchPattern != "*" ? CreateUnicodeString(searchPattern) : IntPtr.Zero;
            try
            {
                var iosb = new IO_STATUS_BLOCK();
                var restart = QueryFlags.RestartScan;

                while (true)
                {
                    var status = NtQueryDirectoryFileEx(handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref iosb, buffer, bufferSize, FileInformationClass.FileIdBothDirectoryInformation, restart, searchPatternPointer);
                    if (status == NtStatus.NoSuchFile || status == NtStatus.NoMoreFile) break;
                    if (status is not NtStatus.Success and not NtStatus.NoMoreFile) throw new InvalidOperationException($"NtQueryDirectoryFile failed: 0x{status:X}");
                    restart = QueryFlags.None;

                    if (getFiles && getFolders) yield return ParseFileAndFolderEntries(buffer, folderPath);
                    else if (getFiles) yield return ParseFileEntries(buffer, folderPath);
                    else yield return ParseFolderEntries(buffer, folderPath);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                FreeUnicodeString(searchPatternPointer);
                QueueCloseHandle(handle);
            }

            yield break;
        }
        private static IEnumerable<List<FileSystemEntry>> GetFileSystemEntriesRecursiveInternal(string rootFolderPath, bool getFiles, bool getFolders, IList<string> excludeFolders, int bufferSize, int recursiveThreads)
        {
            var results = new BlockingCollection<List<FileSystemEntry>>();
            var visited = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase) { [rootFolderPath] = 0 };
            var pendingDirectories = new BlockingCollection<string> { rootFolderPath };
            var excludedSet = excludeFolders is not null ? new HashSet<string>(excludeFolders, StringComparer.OrdinalIgnoreCase) : null;
            var workerTasks = new ConcurrentBag<Task>();

            var runningThreadsCount = 0;
            var pendingDirectoriesCount = 1;  //1 for rootFolderPath

            void TrySpawnWorker()
            {
                if (runningThreadsCount >= recursiveThreads || results.IsAddingCompleted) return;
                if (System.Threading.Interlocked.Increment(ref runningThreadsCount) > recursiveThreads)
                {
                    System.Threading.Interlocked.Decrement(ref runningThreadsCount);
                    return;
                }

                var workerTask = Task.Run(() =>
                {
                    try
                    {
                        //foreach (var currentPath in pendingDirectories.GetConsumingEnumerable())
                        while (!pendingDirectories.IsCompleted && !results.IsAddingCompleted)
                        {
                            if (!pendingDirectories.TryTake(out var currentPath, 50)) break;

                            try
                            {
                                foreach (var currentLevelEntries in GetFileSystemEntriesInternal(currentPath, null, getFiles, true, bufferSize))
                                {
                                    if (results.IsAddingCompleted) break;
                                    foreach (var entry in currentLevelEntries)
                                    {
                                        if (!entry.IsDirectory) continue;
                                        if (excludedSet is not null && excludedSet.Contains(entry.FullPath)) continue;
                                        if (!visited.TryAdd(entry.FullPath, 0)) continue;

                                        if (entry.IsReParsePoint)
                                        {
                                            var canonicalPath = GetCanonicalDirectoryPath(entry.FullPath);
                                            if (canonicalPath is not null && !visited.TryAdd(canonicalPath, 0)) continue;
                                        }

                                        System.Threading.Interlocked.Increment(ref pendingDirectoriesCount);
                                        pendingDirectories.Add(entry.FullPath);
                                        TrySpawnWorker();
                                    }

                                    if (!results.IsAddingCompleted)
                                    {
                                        if (getFiles && getFolders) results.Add(currentLevelEntries);
                                        else if (getFiles) results.Add(currentLevelEntries.Where(x => x.IsFile).ToList());
                                        else results.Add(currentLevelEntries); // not filtering files because, it is already a result of ParseFolderEntries
                                    }
                                }
                            }
                            finally
                            {
                                if (System.Threading.Interlocked.Decrement(ref pendingDirectoriesCount) == 0)
                                {
                                    try { pendingDirectories.CompleteAdding(); } catch (ObjectDisposedException) { } //safe to swallow ObjectDisposedException
                                    try { results.CompleteAdding(); } catch (ObjectDisposedException) { }
                                }
                            }
                        }
                    }
                    finally
                    {
                        System.Threading.Interlocked.Decrement(ref runningThreadsCount);
                    }
                });
                workerTasks.Add(workerTask);
            }

            TrySpawnWorker();

            try
            {
                foreach (var batch in results.GetConsumingEnumerable())
                {
                    yield return batch;
                }
            }
            finally
            {
                // If the caller breaks out early, we tell the background threads to stop adding items
                try { pendingDirectories.CompleteAdding(); } catch (ObjectDisposedException) { } //safe to swallow ObjectDisposedException
                try { results.CompleteAdding(); } catch (ObjectDisposedException) { }
                try
                {
                    Task.WhenAll(workerTasks).Wait();
                }
                finally
                {
                    pendingDirectories.Dispose();
                    results.Dispose();
                }
            }
        }

        public static IEnumerable<List<FileSystemEntry>> GetFileSystemEntries(string folderPath, string searchPattern = null, int bufferSize = 256 * 1024) => GetFileSystemEntriesInternal(folderPath, searchPattern, true, true, bufferSize);
        public static IEnumerable<List<FileSystemEntry>> GetFiles(string folderPath, string searchPattern = null, int bufferSize = 256 * 1024) => GetFileSystemEntriesInternal(folderPath, searchPattern, true, false, bufferSize);
        public static IEnumerable<List<FileSystemEntry>> GetDirectories(string folderPath, string searchPattern = null, int bufferSize = 256 * 1024) => GetFileSystemEntriesInternal(folderPath, searchPattern, false, true, bufferSize);

        public static IEnumerable<List<FileSystemEntry>> GetFileSystemEntriesRecursive(string folderPath, IList<string> excludeFolders = null, int bufferSize = 256 * 1024, int recursiveThreads = 64) => GetFileSystemEntriesRecursiveInternal(folderPath, true, true, excludeFolders, bufferSize, recursiveThreads);
        public static IEnumerable<List<FileSystemEntry>> GetFilesRecursive(string folderPath, IList<string> excludeFolders = null, int bufferSize = 256 * 1024, int recursiveThreads = 64) => GetFileSystemEntriesRecursiveInternal(folderPath, true, false, excludeFolders, bufferSize, recursiveThreads);
        public static IEnumerable<List<FileSystemEntry>> GetDirectoriesRecursive(string folderPath, IList<string> excludeFolders = null, int bufferSize = 256 * 1024, int recursiveThreads = 64) => GetFileSystemEntriesRecursiveInternal(folderPath, false, true, excludeFolders, bufferSize, recursiveThreads);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct FileIdBothDirInformation //FileIdBothDirInformation
    {
        public uint NextEntryOffset;     // Offset to next record (0 if last)
        public uint FileIndex;           // Byte offset within parent directory

        public long CreationTime;        // FILETIME format
        public long LastAccessTime;
        public long LastWriteTime;
        public long ChangeTime;

        public long EndOfFile;           // File size in bytes
        public long AllocationSize;      // Disk space allocated

        public uint FileAttributes;
        public uint FileNameLength;      // Length of FileName string in bytes
        public uint EaSize;              // Extended Attribute size

        public byte ShortNameLength;     // Length of short name in bytes
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string ShortName;  // 8.3 Name string

        public long FileId;              // The 64-bit NTFS unique File Identifier

        public static readonly int fileNameOffset = Marshal.SizeOf<FileIdBothDirInformation>(); // The actual variable-length FileName string starts here in memory.
    }

    public class FileSystemEntry
    {
        private FileIdBothDirInformation info;
        public string EntryName => Path.GetFileName(FullPath);
        public string FullPath { get; }
        internal FileSystemEntry(FileIdBothDirInformation fileIdBothDirInformation, string name, string parentPath)
        {
            info = fileIdBothDirInformation;
            FullPath = Path.Combine(parentPath, name);
        }

        public DateTime CreationTime => DateTime.FromFileTimeUtc(info.CreationTime);
        public DateTime LastWriteTime => DateTime.FromFileTimeUtc(info.LastWriteTime);
        public DateTime LastAccessTime => DateTime.FromFileTimeUtc(info.LastAccessTime);

        public bool IsReParsePoint => (info.FileAttributes & (uint)FileAttributes.ReparsePoint) != 0;
        public bool IsDirectory => (info.FileAttributes & (uint)FileAttributes.Directory) != 0;
        public bool IsFile => !IsDirectory;

        public bool IsHidden => (info.FileAttributes & (uint)FileAttributes.Hidden) != 0;
        public long FileSize => info.EndOfFile;

        private static readonly string[] formattedSizeSuffixes = { "Bytes", "KB", "MB", "GB", "TB" };
        public string FormattedSize
        {
            get
            {
                if (IsDirectory) return "<DIR>";

                double size = info.EndOfFile;
                var counter = 0;
                while (size >= 1024 && counter < formattedSizeSuffixes.Length - 1)
                {
                    counter++;
                    size /= 1024;
                }

                return $"{size:0.##} {formattedSizeSuffixes[counter]}";
            }
        }
    }
}
