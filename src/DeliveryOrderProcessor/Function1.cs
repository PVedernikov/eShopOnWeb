using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Linq;
using DeliveryOrderProcessor.Models;
using System.Reflection.PortableExecutable;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace DeliveryOrderProcessor
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
            var containerId = "Orders";

            var cosmosClient = new CosmosClient(_config["CosmosEndpointUrl"], _config["CosmosPrimeryKey"]);
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(_config["CosmosDatabaseId"]);
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/Orders"); ;

            var reader = new StreamReader(req.Body, System.Text.Encoding.UTF8);
            var orderJson = reader.ReadToEnd();
            var order = JsonConvert.DeserializeObject<Order>(orderJson);
            if(!order.id.HasValue)
                order.id = Guid.NewGuid();

            var response = await container.CreateItemAsync(order);

            return new OkObjectResult(order.id);
        }
    }
}
