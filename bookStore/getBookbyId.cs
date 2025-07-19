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
    public static class getBookbyId
    {
        [FunctionName("getBookbyId")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getBookbyId/{id}")] HttpRequest req,
             [CosmosDB(
            databaseName: "BookDb",
            containerName: "BookContainer",
            Connection = "CosmosDBConnection",
              SqlQuery = "SELECT * FROM c where c.id={id}")] IEnumerable<dynamic> inputDocument,
            ILogger log)
        {

            log.LogInformation("GetAllBooks Function Triggered");

            return new OkObjectResult(inputDocument);
        }
    }
}
