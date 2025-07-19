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
using static System.Reflection.Metadata.BlobBuilder;

namespace bookStore
{
    public static class createBook
    {
        [FunctionName("createBook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addBook")] HttpRequest req,
             [CosmosDB(
            databaseName: "BookDb",
            containerName: "BookContainer",
            Connection = "CosmosDBConnection"

        )] IAsyncCollector<dynamic> books,
            ILogger log)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(body);

            if (data?.title == null || data?.author == null)
                return new BadRequestObjectResult("Missing title or author");

            var newBook = new
            {
                id = Guid.NewGuid().ToString(),
                title = (string)data.title,
                author = (string)data.author,
                date_publish = DateTime.UtcNow
            };

            await books.AddAsync(newBook);
            return new OkObjectResult(newBook);
        }
    }
}
