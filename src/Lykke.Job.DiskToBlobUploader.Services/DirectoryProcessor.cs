using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.DiskToBlobUploader.Core.Services;

namespace Lykke.Job.DiskToBlobUploader.Services
{
    public class DirectoryProcessor : IDirectoryProcessor
    {
        private readonly IBlobSaver _blobSaver;
        private readonly ILog _log;
        private readonly string _directoryPath;
        private readonly string _directory;
        private readonly string _fileNameTimeFormat;

        public DirectoryProcessor(
            IBlobSaver blobSaver,
            ILog log,
            string fileNameTimeFormat,
            string diskPath,
            string directory)
        {
            _blobSaver = blobSaver;
            _log = log;
            _fileNameTimeFormat = fileNameTimeFormat;
            _directory = directory;
            _directoryPath = Path.Combine(diskPath, directory);

            if (!Directory.Exists(_directoryPath))
                Directory.CreateDirectory(_directoryPath);

            Directory.SetCurrentDirectory(_directoryPath);
        }

        public async Task ProcessDirectoryAsync()
        {
            var dirs = Directory.GetDirectories(_directoryPath, "*", SearchOption.TopDirectoryOnly);
            if (dirs.Length <= 1)
                return;

            var dirsToProcess = dirs.OrderBy(i => i).ToList();

            try
            {
                for (int i = 0; i < dirsToProcess.Count - 1; ++i)
                {
                    string dir = dirsToProcess[i];

                    var files = Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly);

                    var messages = new List<string>();
                    int filesCount = 0;
                    DateTime minDate = DateTime.UtcNow;
                    foreach (var file in files)
                    {
                        DateTime fileTime = GetTimeFromFileName(file);
                        if (minDate > fileTime)
                            minDate = fileTime;
                        using (var sr = File.OpenText(file))
                        {
                            do
                            {
                                var str = sr.ReadLine();
                                if (str == null)
                                    break;
                                messages.Add(str);
                            } while (true);
                        }
                        ++filesCount;
                    }

                    string storagePath = Path.GetFileName(dir);
                    await _blobSaver.SaveToBlobAsync(messages, storagePath);

                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    await _log.WriteInfoAsync(
                        nameof(DirectoryProcessor),
                        nameof(ProcessDirectoryAsync),
                        $"Uploaded and deleted {filesCount} files for {_directory}/{storagePath}");
                }
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(DirectoryProcessor), nameof(ProcessDirectoryAsync), ex);
            }
        }

        private DateTime GetTimeFromFileName(string filePath)
        {
            string dateString = Path.GetFileNameWithoutExtension(filePath);
            if (DateTime.TryParseExact(
                dateString,
                _fileNameTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out DateTime dateTime))
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return DateTime.Parse(dateString);
        }
    }
}
