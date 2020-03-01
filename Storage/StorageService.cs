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
            try
            {
                var conteiner = _client.GetContainerReference(_configuration["StorageAccount:Conteiner"]);
                var blob = conteiner.GetBlockBlobReference(name);
                
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