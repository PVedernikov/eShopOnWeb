using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.ServiceBus;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace OrderItemsReserver_ServiceBus
{
    public class Function1
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;

        public Function1(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _client = httpClientFactory.CreateClient();
            _config = config;
        }

        [FunctionName("Function1")]
        public async Task Run([ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
        {
            var containerName = "files";
            var fileName = $"Order_{Guid.NewGuid()}.json";

            try
            {
                var blobClientOptions = new BlobClientOptions();
                blobClientOptions.Retry.MaxRetries = 3;
                var blobServiceClient = new BlobServiceClient(_config["BlobConnectionString"], blobClientOptions);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(fileName);
                var result = await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(myQueueItem)));
            }
            catch (RequestFailedException ex)
            {
                // Send email via logic app
                var content = new StringContent(myQueueItem, Encoding.UTF8);
                await _client.PostAsync(_config["LogicAppUrl"], content);
                log.LogError($"Blob upload failed: {ex.Message}.");
            }
            catch (Exception ex)
            {
                // Send email via logic app
                var content = new StringContent(myQueueItem, Encoding.UTF8);
                await _client.PostAsync(_config["LogicAppUrl"], content);
                log.LogError($"Error occured: {ex.Message}. Email sent.");
            }
        }
    }
}
