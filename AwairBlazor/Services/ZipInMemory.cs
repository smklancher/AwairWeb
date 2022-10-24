using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwairBlazor.Services
{
    public class ZipInMemory
    {
        private readonly ZipArchive archive;
        private readonly MemoryStream memoryStream;
        private byte[]? completedBytes;

        public ZipInMemory()
        {
            memoryStream = new MemoryStream();
            archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
        }

        public bool IsComplete { get; private set; } = false;

        public void AddFile(string entryName, byte[] fileBytes)
        {
            var archiveEntry = archive.CreateEntry(entryName);

            using (var entryStream = archiveEntry.Open())
            {
                // ok to write the whole buffer at once?
                entryStream.Write(fileBytes, 0, fileBytes.Length);
            }
        }

        public void AddFile(string entryName, string content)
        {
            var archiveEntry = archive.CreateEntry(entryName);

            using (var entryStream = archiveEntry.Open())
            using (var streamWriter = new StreamWriter(entryStream))
            {
                streamWriter.Write(content);
            }
        }

        public void AddFile(string entryName, Action<Stream> writeToStreamAction)
        {
            var archiveEntry = archive.CreateEntry(entryName);

            using (var entryStream = archiveEntry.Open())
            {
                writeToStreamAction(entryStream);
            }
        }

        public void AddFileFromFile(string file) => archive.CreateEntryFromFile(file, Path.GetFileName(file));

        public void AddFileFromFile(string entryName, string file) => archive.CreateEntryFromFile(file, entryName);

        public void AddFileFromFileDirtyRead(string entryName, string file)
        {
            // to get log files actively locked for writing, need to open as a filestream with FileShare.ReadWrite
            // https://stackoverflow.com/a/9760751/221018

            var archiveEntry = archive.CreateEntry(entryName);

            using (var entryStream = archiveEntry.Open())
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.CopyTo(entryStream);
                }
            }
        }

        public void AddFolder(string folderPath, string searchPattern, string folderNameInZip, bool topDirectoryOnly)
        {
            if (Directory.Exists(folderPath))
            {
                var files = Directory.EnumerateFiles(folderPath, searchPattern, topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var relativefile = file.Replace(folderPath, string.Empty).TrimStart('\\');
                    var zipPath = $@"{folderNameInZip}\{relativefile}";  //Path.Combine requires rooted path
                    Trace.WriteLine($"{zipPath} : {relativefile} : {file}");
                    AddFileFromFileDirtyRead(zipPath, file);
                }
            }
            else
            {
                //Log.WriteLine($"Does not exist or no permissions: {folderPath}");
            }
        }

        public string CompleteAndReturnBase64()
        {
            FinishZipFile();

            var ret = Convert.ToBase64String(completedBytes ?? new byte[] { });
            var size = ret.Length * sizeof(char);
            Trace.WriteLine($"Bytes of zip as base64: {size:n0}");

            return ret;
        }

        public byte[]? CompleteAndReturnBytes()
        {
            FinishZipFile();

            return completedBytes;
        }

        private void FinishZipFile()
        {
            if (!IsComplete)
            {
                IsComplete = true;

                // leave memory stream open, but dispose ZipArchive stream before access
                // https://stackoverflow.com/a/17939367/221018
                archive.Dispose();
                completedBytes = memoryStream.ToArray();
                memoryStream.Dispose();

                var size = completedBytes?.Length ?? 0;
                Trace.WriteLine($"Bytes of zip: {size:n0}");
            }
        }
    }
}
