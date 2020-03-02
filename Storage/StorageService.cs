using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ImageResizer.Storage
{
    public class StorageService
    {
        public readonly CloudBlobClient _client;
        public readonly IConfiguration _configuration;
        public readonly ILogger<StorageService> _logger;

        public StorageService(CloudBlobClient client, IConfiguration config, ILogger<StorageService> logger)
        {
            _configuration = config;
            _client = client;
            _logger = logger;
        }

        public async Task<(bool, CloudBlockBlob)> TryGetFile(string name)
        {
            return await TryGetFile(_configuration["StorageAccount:Conteiner"], name);
        }
        
        public async Task<(bool, CloudBlockBlob)> TryGetFileCached(string name)
        {
            return await TryGetFile(_configuration["StorageAccount:ConteinerResized"], name);
        }

        public async Task<(bool, CloudBlockBlob)> TryUploadToCache(string name, byte[] imageContent, string mimeType)
        {
            try
            {
                var cacheBlob = GetFile(_configuration["StorageAccount:ConteinerResized"], name);
                await cacheBlob.UploadFromByteArrayAsync(imageContent, 0, imageContent.Length);

                cacheBlob.Properties.ContentType = mimeType;
                await cacheBlob.SetPropertiesAsync();

                return (true, cacheBlob);
            }
            catch (Exception ex)
            {                
                _logger.LogError(ex, $"Could not upload file {name}");            
            }

            return (false, null);
        }

        private async Task<(bool, CloudBlockBlob)> TryGetFile(string container, string name)
        {
            try
            {
                var blob = GetFile(container, name);
                
                if (await blob.ExistsAsync())
                {
                    return (true, blob);
                }
            }
            catch (Exception ex)
            {                
                _logger.LogError(ex, $"Could not download file {name}");
            }

            return (false, null);
        }

        private CloudBlockBlob GetFile(string conteinerName, string blobName)
        {
                            var conteiner = _client.GetContainerReference(conteinerName);
                return conteiner.GetBlockBlobReference(blobName);
        }

        public async Task<byte[]> GetBlobBytes(CloudBlockBlob blob)
        {
            using(var memory = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(memory);
                return memory.ToArray();
            }
        }
    }
}