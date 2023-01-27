using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO.Pipes;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace OrderItemsReserver
{
    public class Function1
    {
        private readonly IConfiguration _config;

        public Function1(IConfiguration config)
        {
            _config = config;
        }

        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                var orderId = req.Query.ContainsKey("orderid") && !string.IsNullOrEmpty(req.Query["orderid"].Last())
                    ? req.Query["orderid"].Last()
                    : "NotSet";

                log.LogInformation($"C# HTTP trigger function; Order {orderId} processing");

                var connStr = _config["BlobConnectionString"];
                var containerName = "files";

                var blobServiceClient = new BlobServiceClient(connStr);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                await containerClient.CreateIfNotExistsAsync();

                var fileName = $"Order_{orderId}_{Guid.NewGuid()}.json";
                var blobClient = containerClient.GetBlobClient(fileName);

                var result = await blobClient.UploadAsync(req.Body);
                log.LogInformation($"C# HTTP trigger function; Order {orderId} created");
                return new OkObjectResult(fileName);
            }
            catch( Exception ex )
            {
                log.LogError($"==========> Error: {ex.Message}");
                return new BadRequestResult();
            }
        }
    }
}
