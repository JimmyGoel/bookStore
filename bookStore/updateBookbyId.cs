using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace bookStore
{
    public static class updateBookbyId
    {
        public class Book
        {
            public string id { get; set; }            // Cosmos requires 'id'
            public string author { get; set; }
            public string title { get; set; }
            public DateTime date_published { get; set; } // keep string to match your sample; change to DateTime? if you own schema
        }


        [FunctionName("updateBookbyId")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "updateBook/{id}")] HttpRequest req,
        // Input binding: pull the existing doc(s) by id
        [CosmosDB(
            databaseName: "BookDb",
            containerName: "BookContainer",
            Connection = "CosmosDBConnection",
            SqlQuery = "SELECT * FROM c WHERE c.id = {id}"
        )] IEnumerable<Book> existingDocs,
        // Output binding: write (upsert) updated doc
        [CosmosDB(
            databaseName: "BookDb",
            containerName: "BookContainer",
            Connection = "CosmosDBConnection")] IAsyncCollector<Book> outputDocs,
        ILogger log,
        string id)
        {
            log.LogInformation("UpdateBookById Function Triggered. Id={Id}", id);

            // Locate existing (Cosmos query returns 0..n; we take first)
            var existing = existingDocs?.FirstOrDefault();
            if (existing == null)
            {
                return new NotFoundObjectResult($"Book with id '{id}' not found.");
            }

            // Read request body (JSON)
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                return new BadRequestObjectResult("Request body required.");
            }

            // Deserialize into loose dynamic so missing props are null
            dynamic payload = JsonConvert.DeserializeObject(body);

            // Merge: payload overrides, else keep existing
            var updated = new Book
            {
                id = id,
                author = payload?.author != null ? (string)payload.author : existing.author,
                title = payload?.title != null ? (string)payload.title : existing.title,
                date_published = payload?.date_published != null ? payload.date_published : DateTime.UtcNow
            };

            // Upsert to Cosmos
            await outputDocs.AddAsync(updated);

            return new OkObjectResult(updated);
        }
    }
}
