//using System;
//using System.IO;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using System.Net.Http;
//using Microsoft.Azure.Cosmos;
//using System.ComponentModel;
//using Azure;
//using System.Collections.Generic;

//namespace FiDelApp
//{


//    public static class FiDelApp
//    {
//        private static readonly HttpClient httpClient = new HttpClient();
//        private static readonly string cosmosDbEndpoint = Environment.GetEnvironmentVariable("DBENDPOINT", EnvironmentVariableTarget.Process);
//        private static readonly string cosmosDbKey = Environment.GetEnvironmentVariable("DBKEY", EnvironmentVariableTarget.Process);
//        private static readonly string databaseId = Environment.GetEnvironmentVariable("DBID", EnvironmentVariableTarget.Process);
//        private static readonly string containerId = Environment.GetEnvironmentVariable("CONTAINERID", EnvironmentVariableTarget.Process);
//        private static readonly CosmosClient cosmosClient = new CosmosClient(cosmosDbEndpoint, cosmosDbKey);
//        private static readonly Microsoft.Azure.Cosmos.Container cosmosContainer = cosmosClient.GetContainer(databaseId, containerId);

//        [FunctionName("FiDelApp")]
//        public static async Task<IActionResult> Run(
//            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
//            ILogger log)
//        {
//            try
//            {
//                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

//                // Deserialize the JSON request body
//                var requestDocument = JsonConvert.DeserializeObject<RequestDocument>(requestBody);

//                // Validate that the ID is provided
//                if (string.IsNullOrEmpty(requestDocument.id))
//                {
//                    return new BadRequestObjectResult("The 'id' property is required.");
//                }

//                // Insert the JSON document into Cosmos DB
//                var response = await cosmosContainer.CreateItemAsync(requestDocument);

//                return new OkObjectResult("Data inserted successfully! Well Done! (12/07/2023)");
//            }
//            catch (Exception ex)
//            {
//                log.LogError(ex, "An error occurred while inserting data into Cosmos DB.");
//                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
//            }
//        }
//    }



using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using Azure;
using System.Collections.Generic;

namespace FiDelApp
{
    public static class FiDelApp
    {
        private static readonly HttpClient httpClient = new HttpClient();

        //Database Settings  
        private static readonly CosmosClient cosmosClient;
        private static readonly Microsoft.Azure.Cosmos.Container cosmosContainer;

        //Static constructor  
        static FiDelApp()
        {
            string cosmosDbEndpoint = Environment.GetEnvironmentVariable("DBENDPOINT", EnvironmentVariableTarget.Process);
            string cosmosDbKey = Environment.GetEnvironmentVariable("DBKEY", EnvironmentVariableTarget.Process);
            string databaseId = Environment.GetEnvironmentVariable("DBID", EnvironmentVariableTarget.Process);
            string containerId = Environment.GetEnvironmentVariable("CONTAINERID", EnvironmentVariableTarget.Process);

            cosmosClient = new CosmosClient(cosmosDbEndpoint, cosmosDbKey);
            cosmosContainer = cosmosClient.GetContainer(databaseId, containerId);
        }

        [FunctionName("FiDelApp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var requestDocument = JsonConvert.DeserializeObject<RequestDocument>(requestBody);

                if (string.IsNullOrEmpty(requestDocument.id))
                    return new BadRequestObjectResult("The 'id' property is required.");

                var response = await cosmosContainer.CreateItemAsync(requestDocument);
                return new OkObjectResult("Data inserted successfully! Well Done! (12/07/2023)");
            }
            catch (CosmosException ex)
            {
                log.LogError(ex, $"CosmosDB service error. Error Details: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An unexpected error occurred.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
    public class RequestDocument
    {
        public string id { get; set; }
        public string categoryId { get; set; }
        public string categoryName { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public double Price { get; set; }
        public List<Tag> tags { get; set; }
    }
    public class Tag
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}