using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static bookStore.updateBookbyId;

namespace bookStore
{
    public static class deleteBook
    {
        public class Book
        {
            public string id { get; set; }            // Cosmos requires 'id'
            public string author { get; set; }
            public string title { get; set; }
            public DateTime date_published { get; set; } // keep string to match your sample; change to DateTime? if you own schema
        }
        [FunctionName("deleteBook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "deletebook/{id}")] HttpRequest req,
            [CosmosDB(
            databaseName: "BookDb",
            containerName: "BookContainer",
            Connection = "CosmosDBConnection",
            SqlQuery = "SELECT * FROM c WHERE c.id = {id}"
        )] IEnumerable<Book> docs,
        ILogger log,
        string id)
        {
            log.LogInformation("DeleteBookById triggered. Id={Id}", id);

            var book = docs?.FirstOrDefault();
            if (book == null)
            {
                return new NotFoundObjectResult("Item not found.");
            }

            var conn = Environment.GetEnvironmentVariable("CosmosDBConnection");
            var client = new CosmosClient(conn);
            var container = client.GetContainer("BookDb", "BookContainer");

            try
            {
                await container.DeleteItemAsync<Book>(book.id, new PartitionKey(book.author));
                return new OkObjectResult("Item deleted successfully.");
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundObjectResult("Item not found (delete).");
            }
        }
    }
}
