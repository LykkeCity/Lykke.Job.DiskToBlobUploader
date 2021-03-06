﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Lykke.Job.DiskToBlobUploader.Core.Services;

namespace Lykke.Job.DiskToBlobUploader.Services
{
    public class BlobSaver : IBlobSaver
    {
        private readonly CloudBlobContainer _blobContainer;
        private readonly BlobRequestOptions _blobRequestOptions = new BlobRequestOptions
        {
            MaximumExecutionTime = TimeSpan.FromMinutes(15),
        };
        private readonly byte[] _eolBytes = Encoding.UTF8.GetBytes("\r\n\r\n");

        private DateTime _lastBatchSave = DateTime.MinValue;

        public BlobSaver(
            string blobConnectionString,
            string directory,
            bool isPublicContainer)
        {
            var blobClient = CloudStorageAccount.Parse(blobConnectionString).CreateCloudBlobClient();
            _blobContainer = blobClient.GetContainerReference(directory.ToLower());
            if (!_blobContainer.ExistsAsync().GetAwaiter().GetResult())
                _blobContainer.CreateAsync(
                    isPublicContainer ? BlobContainerPublicAccessType.Container : BlobContainerPublicAccessType.Off,
                    null,
                    null)
                    .GetAwaiter().GetResult();
        }

        public async Task SaveToBlobAsync(IEnumerable<byte[]> blocks, string storagePath)
        {
            var blob = await InitBlobAsync(storagePath);

            using (var stream = new MemoryStream())
            {
                foreach (var block in blocks)
                {
                    stream.Write(block, 0, block.Length);
                    stream.Write(_eolBytes, 0, _eolBytes.Length);
                }
                stream.Position = 0;
                await blob.AppendFromStreamAsync(stream, null, _blobRequestOptions, null);
            }
        }

        private async Task<CloudAppendBlob> InitBlobAsync(string storagePath)
        {
            var blob = _blobContainer.GetAppendBlobReference(storagePath);
            if (await blob.ExistsAsync())
                return blob;

            try
            {
                await blob.CreateOrReplaceAsync(AccessCondition.GenerateIfNotExistsCondition(), null, null);
                blob.Properties.ContentType = "text/plain";
                blob.Properties.ContentEncoding = Encoding.UTF8.WebName;
                await blob.SetPropertiesAsync(null, _blobRequestOptions, null);
            }
            catch (StorageException)
            {
            }

            return blob;
        }
    }
}
