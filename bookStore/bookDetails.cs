using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace bookStore
{
    public static class bookDetails
    {
        [FunctionName("bookDetails")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",  Route = "getAllBooks")] HttpRequest req,
            [CosmosDB(
            databaseName: "BookDb",
            containerName: "BookContainer",
            Connection = "CosmosDBConnection"
          
        )] IEnumerable<dynamic> inputDocument,
            ILogger log)
        {
            log.LogInformation("GetAllBooks Function Triggered success");

            return new OkObjectResult(inputDocument);
        }
    }
}
