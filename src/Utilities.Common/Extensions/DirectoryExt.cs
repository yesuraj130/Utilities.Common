using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Utilities.Common.Extensions
{
    public static class DirectoryExt
    {
        public static bool IsLatestCopy(string Source, string Destination, string SearchPattern, /*SearchOption SearchOption,*/ out string[] UpdatedFiles, out string[] AllFiles, int MaxDepth)
        {
            ConcurrentBag<string> TempUpdatedFiles = new ConcurrentBag<string>();
            ConcurrentBag<string> TempAllFiles = new ConcurrentBag<string>();


            if (Directory.Exists(Source))
            {
                InternalIsLatestCopyTopDirectory(Source, Destination, SearchPattern, /*SearchOption,*/ MaxDepth, 0, TempUpdatedFiles, TempAllFiles);
            }
            UpdatedFiles = TempUpdatedFiles.ToArray();
            AllFiles = TempAllFiles.ToArray();
            return UpdatedFiles.Length > 0;

        }

        public static bool GetFilesInDirectory(string Source, string SearchPattern, /*SearchOption SearchOption,*/ out string[] AllFiles, int MaxDepth)
        {
            ConcurrentBag<string> TempAllFiles = new ConcurrentBag<string>();


            if (Directory.Exists(Source))
            {
                InternalGetFilesTopDirectory(Source, SearchPattern, /*SearchOption,*/ MaxDepth, 0, TempAllFiles);

            }
            AllFiles = TempAllFiles.ToArray();
            return AllFiles.Length > 0;

        }

        public static bool IsWritable(string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)) { }
                return true;
            }
            catch
            {
                if (throwIfFails) throw;
                else return false;
            }
        }



        private static void InternalIsLatestCopyTopDirectory(string Source, string Destination, string SearchPattern, /*SearchOption SearchOption,*/ int MaxDepth, int CurrentDepth, ConcurrentBag<string> UpdatedFiles, ConcurrentBag<string> AllFiles)
        {
            Directory.CreateDirectory(Destination);
            Parallel.ForEach(Directory.GetFiles(Source, SearchPattern, SearchOption.TopDirectoryOnly).AsParallel(), (FilePath) =>
            {
                AllFiles.Add(FilePath);
                if (FileExt.IsLatestCopyWithoutCheck(FilePath, Path.Combine(Destination, Path.GetFileName(FilePath))))
                {
                    UpdatedFiles.Add(Path.Combine(Destination, Path.GetFileName(FilePath)));
                }
            });

            CurrentDepth++;
            if (/*SearchOption == SearchOption.AllDirectories &&*/ CurrentDepth <= MaxDepth)
            {
                Parallel.ForEach(Directory.GetDirectories(Source).AsParallel(), (x) =>
                {
                    InternalIsLatestCopyTopDirectory(x, Path.Combine(Destination, Path.GetFileName(x)), SearchPattern, /*SearchOption,*/ MaxDepth, CurrentDepth, UpdatedFiles, AllFiles);
                });
            }
        }

        private static void InternalGetFilesTopDirectory(string Source, string SearchPattern, /*SearchOption SearchOption,*/ int MaxDepth, int CurrentDepth, ConcurrentBag<string> AllFiles)
        {
            Parallel.ForEach(Directory.GetFiles(Source, SearchPattern, SearchOption.TopDirectoryOnly), (FilePath) =>
            {
                AllFiles.Add(FilePath);
            });

            CurrentDepth++;
            if (/*SearchOption == SearchOption.AllDirectories &&*/ CurrentDepth <= MaxDepth)
            {
                foreach (var FolderPath in Directory.GetDirectories(Source))
                {
                    InternalGetFilesTopDirectory(FolderPath, SearchPattern, /*SearchOption,*/ MaxDepth, CurrentDepth, AllFiles);
                }

                //Parallel.ForEach(, e (FilePath) =>
                //{

                //});
            }
        }


        public static string[] FindExtraFiles(string Actual, string Comparer, string SearchPattern, /*SearchOption SearchOption,*/ string[] AllFilesInActual, int MaxDepth)
        {
            if (!Actual.EndsWith($"{Path.DirectorySeparatorChar}")) Actual += Path.DirectorySeparatorChar;
            if (!Comparer.EndsWith($"{Path.DirectorySeparatorChar}")) Comparer += Path.DirectorySeparatorChar;

            GetFilesInDirectory(Comparer, SearchPattern, /*SearchOption,*/ out var AllFilesInComparer, MaxDepth);

            List<string> FilesInActual = new List<string>();
            foreach (var x in AllFilesInActual)
            {
                FilesInActual.Add(x.Replace(Actual, ""));
            }

            List<string> FilesInComparer = new List<string>();
            foreach (var x in AllFilesInComparer)
            {
                FilesInComparer.Add(x.Replace(Comparer, ""));
            }


            List<string> t = new List<string>();
            foreach (var File in FilesInComparer)
            {
                if (!FilesInActual.Contains(File))
                {
                    var temp = File.ToUpper();
                    if (FilesInActual.Any(x => x.ToUpper() == temp)) continue;
                    t.Add(File);
                }
            }
            return t.ToArray();

        }
    }
}
